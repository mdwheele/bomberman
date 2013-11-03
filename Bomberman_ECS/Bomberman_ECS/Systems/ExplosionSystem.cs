using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // Handles explosions, their propagation, and triggering their consequences.
    class ExplosionSystem : EntitySystem
    {
        public ExplosionSystem(EntityGrid entityGrid)
            : base(
            SystemOrders.Update.Explosion,
            new int[] { ComponentTypeIds.Explosion, ComponentTypeIds.Placement }
        )
        {
            this.entityGrid = entityGrid;
        }

        // These indices need to correspond to PropagationDirection
        private static Point[] propagationOffsets = new Point[]
        {
            new Point(-1, 0),       // Left
            new Point(-1, -1),      // Upper left, etc...
            new Point(0, -1),
            new Point(1, -1),
            new Point(1, 0),
            new Point(1, 1),
            new Point(0, 1),
            new Point(-1, 1),
        };
        // Given propagation in a direction, where do we propagate next?
        private static PropagationDirection[] inheritedPropagationDirections = new PropagationDirection[]
        {
            PropagationDirection.Left,
            PropagationDirection.UpperLeft | PropagationDirection.Left | PropagationDirection.Up,
            PropagationDirection.Up,
            PropagationDirection.UpperRight | PropagationDirection.Right | PropagationDirection.Up,
            PropagationDirection.Right,
            PropagationDirection.BottomRight | PropagationDirection.Bottom | PropagationDirection.Right,
            PropagationDirection.Bottom,
            PropagationDirection.BottomLeft | PropagationDirection.Bottom | PropagationDirection.Left,
        };
 
        private EntityGrid entityGrid;

        protected override void Initialize()
        {
            placementComponents = EntityManager.GetComponentManager<Placement>(ComponentTypeIds.Placement);
            explosionComponents = EntityManager.GetComponentManager<Explosion>(ComponentTypeIds.Explosion);
        }

        private IComponentMapper<Placement> placementComponents;
        private IComponentMapper<Explosion> explosionComponents;
        List<int> liveIdsRemoveWorker = new List<int>();
        List<int> explosionTemplateWorker = new List<int>();
        List<Explosion> propagateListWorker = new List<Explosion>();
        List<Placement> propagateListWorkerPlacement = new List<Placement>();

        protected override void OnProcessEntities(Microsoft.Xna.Framework.GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            liveIdsRemoveWorker.Clear();
            explosionTemplateWorker.Clear();
            propagateListWorker.Clear();
            propagateListWorkerPlacement.Clear();

            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (int liveId in entityIdCollection)
            {
                Explosion explosion = explosionComponents.GetComponentFor(liveId);

                explosion.Countdown -= ellapsed;
                explosion.PropagationCountDown -= ellapsed;

                // Apply any impacts. These may affect propagation, so do it first.
                Placement placement = placementComponents.GetComponentFor(liveId);
                int x = (int)Math.Round(placement.Position.X);
                int y = (int)Math.Round(placement.Position.Y);
                ApplyImpacts(x, y, explosion);

                // Check to see if it's time to propagate. Propagation creates new entities, which we can't do inside the enumeration loop
                if ((explosion.PropagationCountDown <= 0f) && (explosion.State.PropagationDirection != PropagationDirection.None) && (explosion.State.Range > 0))
                {
                    explosionTemplateWorker.Add(EntityManager.GetEntityByLiveId(liveId).TemplateId);
                    propagateListWorker.Add(explosion);
                    propagateListWorkerPlacement.Add(placement);
                }

                // Or expire...
                if (explosion.Countdown <= 0f)
                {
                    liveIdsRemoveWorker.Add(liveId);
                }
            }

            PropagateExplosions(explosionTemplateWorker, propagateListWorker, propagateListWorkerPlacement);

            // Remove the expired explosions
            // REVIEW: Since removing something after a timer is a frequent operation, we could generalize this too.
            EntityManager.DelayFreeByLiveId(liveIdsRemoveWorker, EntityFreeOptions.Deep);
        }

        private void PropagateExplosions(List<int> templateIds, List<Explosion> propagateList, List<Placement> propagateListPlacement)
        {
            for (int explosionIndex = 0; explosionIndex < propagateList.Count; explosionIndex++)
            {
                Explosion explosion = propagateList[explosionIndex];
                Placement placement = propagateListPlacement[explosionIndex];
                Vector3 currentPosition = placement.Position;

                // Consider all directions
                for (int considerIndex = 0; considerIndex < propagationOffsets.Length; considerIndex++)
                {
                    PropagationDirection consideredDirection = (PropagationDirection)(0x1 << considerIndex);
                    if ((explosion.State.PropagationDirection & consideredDirection) == consideredDirection)
                    {
                        Point offset = propagationOffsets[considerIndex];
                        Vector3 newPosition = new Vector3(currentPosition.X + offset.X, currentPosition.Y + offset.Y, 0f);

                        // We need to check that there aren't any hard barriers here. Explosions shouldn't go there (unless we have hard pass-through)
                        ExplosionBarrier barrier = GetBarrierAtLocation(newPosition.ToInteger());
                        bool shouldGoHere = (barrier != ExplosionBarrier.Hard) || explosion.State.IsHardPassThrough;
                        if (shouldGoHere)
                        {
                            // When creating the new wexplosion, use the same template as the previous.
                            Entity newEntity = EntityManager.AllocateForGeneratedContent(EntityTemplateManager.GetTemplateById(templateIds[explosionIndex]), Universe.TopLevelGroupUniqueIdBase);
                            Explosion newExplosion = (Explosion)EntityManager.GetComponent(newEntity, ComponentTypeIds.Explosion);
                            newExplosion.State = explosion.State;
                            newExplosion.State.Range--; // Reduce the range of course...
                            // If we explode into a soft block, we don't propagate (unless we're passthrough)
                            if (!ShouldExplosionPropagateThroughBarrier(barrier, explosion))
                            {
                                newExplosion.State.Range = 0;
                            }
                            newExplosion.State.PropagationDirection = inheritedPropagationDirections[considerIndex];

                            // Assign its position
                            Placement newPlacement = (Placement)EntityManager.GetComponent(newEntity, ComponentTypeIds.Placement);
                            newPlacement.Position = newPosition;
                            newPlacement.OrientationAngle = (float)(random.NextDouble() * 360.0);
                        }
                    }
                }
                explosion.State.PropagationDirection = PropagationDirection.None; // Mark this explosion so it doesn't propagate anymore!
            }
        }

        private Random random = new Random();

        private bool ShouldExplosionPropagateThroughBarrier(ExplosionBarrier barrier, Explosion explosion)
        {
            bool propagate = false;
            if (barrier == ExplosionBarrier.None)
            {
                propagate = true;
            }
            else
            {
                if ((barrier == ExplosionBarrier.Soft) && (explosion.State.IsPassThrough || explosion.State.IsHardPassThrough))
                {
                    propagate = true;
                }
                if ((barrier == ExplosionBarrier.Hard) && explosion.State.IsHardPassThrough)
                {
                    propagate = true;
                }
            }
            return propagate;
        }

        private List<int> barrierQueryList = new List<int>();
        // Returns the strongest barrier at that location
        private ExplosionBarrier GetBarrierAtLocation(Point point)
        {
            ExplosionBarrier strongestBarrier = ExplosionBarrier.None;
            entityGrid.QueryUniqueIdsAt(point.X, point.Y, barrierQueryList);
            foreach (int uniqueId in barrierQueryList)
            {
                Entity there = EntityManager.TryGetEntityByUniqueId(uniqueId);
                if (there != null)
                {
                    ExplosionImpact explosionImpact = (ExplosionImpact)EntityManager.GetComponent(there, ComponentTypeIds.ExplosionImpact);
                    if (explosionImpact != null)
                    {
                        strongestBarrier = (ExplosionBarrier)Math.Max((int)explosionImpact.Barrier, (int)strongestBarrier);
                    }
                }
            }
            return strongestBarrier;
        }

        private List<int> uniqueIdWorker = new List<int>();
        private void ApplyImpacts(int x, int y, Explosion explosion)
        {
            entityGrid.QueryUniqueIdsAt(x, y, uniqueIdWorker);
            foreach (int uniqueId in uniqueIdWorker)
            {
                Entity entityHere = EntityManager.GetEntityByUniqueId(uniqueId);
                ExplosionImpact impact = (ExplosionImpact)EntityManager.GetComponent(entityHere, ComponentTypeIds.ExplosionImpact);
                //if ((impact != null) && impact.ShouldSendMessage)
                if (impact != null)
                {
                    // We differentiate between being in the path of an explosion vs being hit by the initial blast (since the explosion
                    // lasts for a while). For most objects it may not matter - but consider that destroying a soft block may reveal a powerup.
                    // Powerups are destroyed by explosions, but we only want to destroy existing powerups. Instead of marking powerups specially,
                    // we can just have them react to the "initial explosion". That way, powerups that we reveal haven't been created yet.
                    if (!explosion.MadeInitialBlast)
                    {
                        EntityManager.SendMessage(entityHere, Messages.HitByInitialExplosion, null);
                    }
                    EntityManager.SendMessage(entityHere, Messages.InExplosion, null);
                }
            }

            explosion.MadeInitialBlast = true;
        }
    }
}
