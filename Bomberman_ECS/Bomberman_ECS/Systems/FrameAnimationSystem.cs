using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;
using Microsoft.Xna.Framework;

namespace Bomberman_ECS.Systems
{
    // To avoid putting special code in Region activating/deactivation, we override OnEntityAdded
    //  (which happens when something is deserialized)
    // If the expiry timer is done, then we mark it for deletion.
    class FrameAnimationSystem : EntitySystem
    {
        public FrameAnimationSystem(SpriteMapper spriteMapper)
            : base(SystemOrders.Update.FrameAnimation, new int[] { ComponentTypeIds.FrameAnimation, ComponentTypeIds.Aspect },
            null)
        {
            this.spriteMapper = spriteMapper;
            DrawOrder = SystemOrders.Draw.FrameAnimation;
        }

        protected override void Initialize()
        {
            animationComponents = EntityManager.GetComponentManager<FrameAnimation>(ComponentTypeIds.FrameAnimation);
            aspectComponents = EntityManager.GetComponentManager<Aspect>(ComponentTypeIds.Aspect);
        }

        private SpriteMapper spriteMapper;
        private IComponentMapper<FrameAnimation> animationComponents;
        private IComponentMapper<Aspect> aspectComponents;

        private static Random random = new Random();

        private void ChooseNextFrame(FrameAnimation animation, Aspect aspect, int frameCount)
        {
            if (animation.RandomFrame)
            {
                int illegalFrame = aspect.FrameIndex;
                int nextFrame = random.Next(0, frameCount - 1);

                if (nextFrame >= illegalFrame)
                {
                    nextFrame++;
                }
                aspect.FrameIndex = nextFrame;
                aspect.FrameIndex %= frameCount;
            }
            else
            {
                if (animation.Direction > 0)
                {
                    aspect.FrameIndex++;
                    if (animation.Loop)
                    {
                        aspect.FrameIndex %= frameCount;
                    }
                }
                else if (animation.Direction < 0)
                {
                    aspect.FrameIndex--;
                    if (animation.Loop)
                    {
                        aspect.FrameIndex = (aspect.FrameIndex + frameCount) % frameCount;
                    }
                }

                aspect.FrameIndex = Math.Max(0, Math.Min(aspect.FrameIndex, frameCount - 1));
            }
        }

        protected override void OnDraw(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (int liveId in entityIdCollection)
            {
                FrameAnimation animation = animationComponents.GetComponentFor(liveId);
                Aspect aspect = aspectComponents.GetComponentFor(liveId);

                int frameCount = spriteMapper.GetSpriteForId(aspect.ModelNameId).FrameCount;
                if (animation != null)
                {
                    float period = 1f / animation.FrameRate;
                    animation.EllapsedTime += ellapsed;
                    if (animation.EllapsedTime >= period)
                    {
                        animation.EllapsedTime -= period;
                        ChooseNextFrame(animation, aspect, frameCount);
                    }
                }
                else
                {
                    ChooseNextFrame(animation, aspect, frameCount);
                }
            }
        }
    }
}
