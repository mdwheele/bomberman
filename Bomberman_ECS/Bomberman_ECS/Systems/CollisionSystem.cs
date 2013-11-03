using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;
using Bomberman_ECS.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics;

namespace Bomberman_ECS.Systems
{
    // This interfaces with the underlying physics library.
    class CollisionSystem : EntitySystem
    {
#if DEBUG
        public CollisionSystem(GraphicsDevice device, ContentManager content, Camera camera)
#else
        public CollisionSystem()
#endif
            : base(
            SystemOrders.Update.Physics,
            new int[] { ComponentTypeIds.Placement, ComponentTypeIds.Physics },
            new uint[] { Messages.SetVelocity }
            )
        {
            FarseerPhysics.Settings.UseFPECollisionCategories = true;
            physicsWorld = new FarseerPhysics.Dynamics.World(Vector2.Zero, new FarseerPhysics.Collision.AABB(Vector2.Zero, new Vector2(20)));
            collisionBodyPool = new CollisionBodyPool(physicsWorld);

#if DEBUG
            this.device = device;
            this.content = content;
            this.camera = camera;
#endif
        }

        protected override void Initialize()
        {
            placementComponents = EntityManager.GetComponentManager<Placement>(ComponentTypeIds.Placement);
            physicsComponents = EntityManager.GetComponentManager<Physics>(ComponentTypeIds.Physics);
        }

        private World physicsWorld;
        private IComponentMapper<Placement> placementComponents;
        private IComponentMapper<Physics> physicsComponents;
        private CollisionBodyPool collisionBodyPool;
        // TODO GC PERF: can we store indices into CollisionBodyPool?
        private Body[] bodySlots = new Body[EntityManager.MaxEntities];
        private Dictionary<int, int> bodyIdToLiveId = new Dictionary<int, int>();
        private static Vector2[] boxVerticesWorker = new Vector2[4];

        /*
        private void ValidatePhysics()
        {
            foreach (KeyValuePair<int, int> pair in bodyIdToLiveId)
            {
                Debug.Assert(bodySlots[pair.Value].BodyId == pair.Key);
            }
        }*/

        private Body GetBodyFromBoundingVolume(Placement placement, Physics physics, Vector2 initialPosition)
        {
            Body body = null;
            BodyType bodyType = physics.IsDynamic ? BodyType.Dynamic : BodyType.Static;

            float halfSize = physics.Size * 0.5f;
            Debug.Assert(halfSize > Settings.Epsilon);

            if (physics.BoundingVolumeType == BoundingVolumeType.Circle)
            {
                body = collisionBodyPool.ActivateBodyCircle(halfSize, bodyType, initialPosition);
            }
            else if (physics.BoundingVolumeType == BoundingVolumeType.Box)
            {
                boxVerticesWorker[3] = initialPosition - new Vector2(-halfSize, halfSize);
                boxVerticesWorker[2] = initialPosition - new Vector2(halfSize, halfSize);
                boxVerticesWorker[1] = initialPosition - new Vector2(halfSize, -halfSize);
                boxVerticesWorker[0] = initialPosition - new Vector2(-halfSize, -halfSize);

                // REVIEW: Allocating memory:
                body = collisionBodyPool.ActivateBodyPolygon(new FarseerPhysics.Common.Vertices(boxVerticesWorker), bodyType);
                body.Position = initialPosition; // TODO: Move this into ActivateBodyPolygon?
            }

            return body;
        }

        protected override void OnEntityAdded(Entity entity, int liveId)
        {
            Placement placement = placementComponents.GetComponentFor(entity);
            Physics physics = physicsComponents.GetComponentFor(entity);

            // Give it an initial position (after this, the collision engine will drive the position)
            Vector3 initialPosition = placement.Position;

            Body body = GetBodyFromBoundingVolume(placement, physics, initialPosition.XY());

            // REVIEW: Might want to set this prior to getting the body?
            // body.CollidesWith = placement.Visible ? Category.All : Category.None;
            // Update categories
            body.IsSensor = physics.IsSensor;
            //Debug.Assert(body.OnSeparation == null);
            if (physics.IsSensor)
            {
                body.OnSeparation += body_OnSeparation;
            }
            body.CollidesWith = (Category)physics.CollidesWidth;
            body.CollisionCategories = (Category)physics.CollisionCategories;

            Debug.Assert(bodySlots[liveId] == null);
            bodySlots[liveId] = body;
            Debug.Assert(!bodyIdToLiveId.ContainsKey(body.BodyId));
            bodyIdToLiveId[body.BodyId] = liveId;
        }

        // This is a horrible hack to allow bombs to overlap players, until the player moves off. At this point,
        // The bomb will begin colliding with others again.
        void body_OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            RemoveSensor(fixtureA.Body);
            RemoveSensor(fixtureB.Body);
        }
        private void RemoveSensor(Body body)
        {
            Physics physics = physicsComponents.GetComponentFor(bodyIdToLiveId[body.BodyId]);
            if (physics.IsSensor)
            {
                physics.IsSensor = false;
                body.IsSensor = false;
                body.OnSeparation -= body_OnSeparation;
            }
        }

        protected override void OnEntityRemoved(Entity entity, int liveId)
        {
            if (bodySlots[liveId] != null)
            {
                Physics physics = physicsComponents.GetComponentFor(liveId);
                if (physics.IsSensor)
                {
                    bodySlots[liveId].OnSeparation -= body_OnSeparation;
                }

                Debug.Assert(bodyIdToLiveId[bodySlots[liveId].BodyId] == liveId);
                bodyIdToLiveId.Remove(bodySlots[liveId].BodyId);
                collisionBodyPool.DeactivateBody(bodySlots[liveId]);
                bodySlots[liveId] = null;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            //ValidatePhysics();

            EntityManager em = EntityManager;
            // Push any dirty positions to the physics simulation
            foreach (int liveId in entityIdCollection)
            {
                Placement placement = placementComponents.GetComponentFor(liveId);
                if (placement.IsDirty(PlacementDirtyBits.Physics))
                {
                    bodySlots[liveId].Position = placement.Position.XY();
                }
            }

            // Advance our physics simulation
            physicsWorld.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));

            // For dynamic bodies, we'll now update our position from the physics simulation
            foreach (int liveId in entityIdCollection)
            {
                Physics physics = physicsComponents.GetComponentFor(liveId);
                if (physics.IsDynamic)
                {
                    Placement placement = placementComponents.GetComponentFor(liveId);

                    // TODO: We're ignoring rotation here. We'll add it later if necessary
                    Vector2 physicsPosition = bodySlots[liveId].Position;
                    placement.SetPositionNoDirty(new Vector3(physicsPosition, 0), PlacementDirtyBits.Physics);
                }

                // Update categories
                // bodySlots[liveId].CollidesWith = (Category)physics.CollidesWidth;
                // bodySlots[liveId].CollisionCategories = (Category)physics.CollisionCategories;
                bodySlots[liveId].IsSensor = physics.IsSensor;
            }

            //ValidatePhysics();
        }

        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            switch (message)
            {
                case Messages.SetVelocity:
                    {
                        Vector2 impulse = data.Vector2;
                        Body body = bodySlots[target.LiveId];
                        body.LinearVelocity = impulse;
                        data.Handled = true;
                    }
                    break;
            }
            return 0;
        }

#if DEBUG
        GraphicsDevice device;
        ContentManager content;
        Camera camera;
        bool showPhysics = false;
        Diagnostics.DebugViewXNA physicsDebugView;
#endif

#if DEBUG
        protected override void OnDraw(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            showPhysics = (Keyboard.GetState().IsKeyDown(Keys.F1));
            if (showPhysics && (physicsDebugView == null))
            {
                physicsDebugView = new Diagnostics.DebugViewXNA(physicsWorld);
                physicsDebugView.LoadContent(device, content);

                physicsDebugView.AppendFlags(DebugViewFlags.Shape);
                physicsDebugView.AppendFlags(DebugViewFlags.Controllers);
                physicsDebugView.AppendFlags(DebugViewFlags.Joint);
            }

            if (showPhysics)
            {
                Matrix view = camera.View;
                Matrix projection = camera.Projection;
                physicsDebugView.RenderDebugData(ref projection, ref view);
            }
        }
#endif
    }
}
