using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    struct TexAndPos
    {
        public Texture2D Texture;
        public Point StartEndSourceRectH;
        public Point StartEndSourceRectV;
        public Vector3 Position;
        public Vector2 Size;
        public Color Tint;
        public float Rotation;
        public SpriteEffects SpriteEffects;
    }

    class RenderSystem : EntitySystem
    {
        public RenderSystem(Game game, Camera camera, SpriteBatch spriteBatch, SpriteMapper spriteMapper)
            : base(SystemOrders.Update.Render, 
                new int[] { ComponentTypeIds.Aspect, ComponentTypeIds.Placement }
                )
        {
            this.spriteMapper = spriteMapper;
            this.spriteBatch = spriteBatch;
            DrawOrder = SystemOrders.Draw.Render;
            this.camera = camera;

            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = new List<TexAndPos>();
            }
        }

        protected override void Initialize()
        {
            placementComponents = EntityManager.GetComponentManager<Placement>(ComponentTypeIds.Placement);
            aspectComponents = EntityManager.GetComponentManager<Aspect>(ComponentTypeIds.Aspect);
        }

        private IComponentMapper<Placement> placementComponents;
        private IComponentMapper<Aspect> aspectComponents;
        private Camera camera;
        private SpriteMapper spriteMapper;

        // We support a limited number of layers.
        private const int MaxLayers = 6;
        private List<TexAndPos>[] textures = new List<TexAndPos>[MaxLayers];

        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            EntityManager em = this.EntityManager;
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i].Clear();
            }

            foreach (int liveId in entityIdCollection)
            {
                Placement placement = placementComponents.GetComponentFor(liveId);
                Aspect aspect = aspectComponents.GetComponentFor(liveId);

                if (placement.Visible)
                {
                    int frameIndex = aspect.FrameIndex;
                    int varietyIndex = aspect.VarietyIndex;
                    
                    Sprite sprite = spriteMapper.GetSpriteForId(aspect.ModelNameId);
                    Texture2D texture = sprite.Texture;
                    Rectangle rect;
                    sprite.GetBounds(varietyIndex, frameIndex, out rect);
                    int framePixelWidth = rect.Width;
                    int framePixelHeight = rect.Height;
                    int startPixelX = rect.X;
                    int startPixelY = rect.Y;

                    Color leColor = aspect.Tint;

                    TexAndPos texAndPos = new TexAndPos()
                    {
                        Size = aspect.Size / framePixelWidth,
                        Texture = texture,
                        Tint = leColor,
                        Rotation = MathHelper.ToRadians(placement.OrientationAngle + placement.AdditionalVisualOrientation),
                        StartEndSourceRectH = new Point(startPixelX, framePixelWidth),
                        StartEndSourceRectV = new Point(startPixelY, framePixelHeight),
                        SpriteEffects = aspect.SpriteEffects
                    };
                    texAndPos.Position = placement.Position;
                    texAndPos.Position += placement.AdditionalVisualPosition;

                    if (placement.Layer < textures.Length)
                    {
                        textures[placement.Layer].Add(texAndPos);
                    }
                }
            }
        }

        private SpriteBatch spriteBatch;
        protected override void OnDraw(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            spriteBatch.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            // We render in the order we say. This *could* have performance issues with texture switching.
            for (int i = 0; i < textures.Length; i++)
            {
                List<TexAndPos> temp = textures[i];

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, null, null, camera.View);

                foreach (TexAndPos texAndPos in temp)
                {
                    Vector2 origin = new Vector2(texAndPos.StartEndSourceRectH.Y / 2f, texAndPos.StartEndSourceRectV.Y / 2f);
                    Vector2 drawOffset = new Vector2(0.5f);
                    spriteBatch.Draw(texAndPos.Texture,
                        drawOffset + new Vector2(texAndPos.Position.X, texAndPos.Position.Y),
                        new Rectangle(texAndPos.StartEndSourceRectH.X, texAndPos.StartEndSourceRectV.X, texAndPos.StartEndSourceRectH.Y, texAndPos.StartEndSourceRectV.Y),
                        texAndPos.Tint,
                        texAndPos.Rotation,
                        origin,
                        texAndPos.Size, texAndPos.SpriteEffects, texAndPos.Position.Z);
                }
                spriteBatch.End();
            }
        }
    }
}
