using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Scripts
{
    static partial class Scripts
    {
        // Makes the size of an entity throb
        private static int PulsatePositionId = "Pulsate_CurrentPos".CRC32Hash();
        private static int PulsatePeriodId = "Pulsate_Period".CRC32Hash();
        public static void Pulsate_Init(ScriptContainer scriptContainer, float period)
        {
            scriptContainer.PropertyBag.SetValue(PulsatePeriodId, period);
        }
        public static void Pulsate(EntityManager em, ScriptContainer scriptContainer, int entityLiveId, GameTime gameTime)
        {
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float period = scriptContainer.PropertyBag.GetSingleValue(PulsatePeriodId);
            float position = scriptContainer.PropertyBag.GetSingleValue(PulsatePositionId, 0f);
            position += ellapsed;
            position %= period;
            scriptContainer.PropertyBag.SetValue(PulsatePositionId, position);

            float progression = (float)Math.Abs(Math.Sin(position * MathHelper.TwoPi / period));

            Aspect aspect = (Aspect)em.GetComponent(em.GetEntityByLiveId(entityLiveId), ComponentTypeIds.Aspect);
            if (aspect != null)
            {
                Vector2 size = aspect.Size;
                size.Y = MathHelper.Lerp(GameConstants.PulsationSizeMin, GameConstants.PulsationSizeMax, progression);
                size.X = MathHelper.Lerp(GameConstants.PulsationSizeMax, GameConstants.PulsationSizeMin, progression);
                aspect.Size = size;
            }
        }
    }
}
