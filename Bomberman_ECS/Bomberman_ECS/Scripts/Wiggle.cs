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
        // Makes an entity wiggle (its position)
        private static int WigglePositionId = "Wiggle_CurrentPos".CRC32Hash();
        private static int WigglePeriodId = "Wiggle_Period".CRC32Hash();
        private static int WiggleExtentId = "Wiggle_Extent".CRC32Hash();
        private static int WiggleXTargetId = "Wiggle_XTarget".CRC32Hash();
        private static int WiggleYTargetId = "Wiggle_YTarget".CRC32Hash();
        private static int WiggleXStartId = "Wiggle_XStart".CRC32Hash();
        private static int WiggleYStartId = "Wiggle_YStart".CRC32Hash();
        public static void Wiggle_Init(ScriptContainer scriptContainer, float period, float extent)
        {
            scriptContainer.PropertyBag.SetValue(WigglePeriodId, period);
            scriptContainer.PropertyBag.SetValue(WiggleExtentId, extent);
        }
        public static void Wiggle(EntityManager em, ScriptContainer scriptContainer, int entityLiveId, GameTime gameTime)
        {
            PropertyBag propertyBag = scriptContainer.PropertyBag;
            Placement placement = (Placement)em.GetComponent(em.GetEntityByLiveId(entityLiveId), ComponentTypeIds.Placement);

            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float period = propertyBag.GetSingleValue(WigglePeriodId);
            float position = propertyBag.GetSingleValue(PulsatePositionId, float.MaxValue);
            bool needReset = false;
            if (position == float.MaxValue)
            {
                needReset = true;
                position = 0;
            }
            position += ellapsed;
            if (position > period)
            {
                position -= period;
                needReset = true;
            }
            scriptContainer.PropertyBag.SetValue(PulsatePositionId, position);

            if (needReset)
            {
                float extent = propertyBag.GetSingleValue(WiggleExtentId);

                // Choose a new destination
                float xNewTarget = (float)(random.NextDouble() * 2f - 1f) * extent;
                float yNewTarget = (float)(random.NextDouble() * 2f - 1f) * extent;
                propertyBag.SetValue(WiggleXTargetId, xNewTarget);
                propertyBag.SetValue(WiggleYTargetId, yNewTarget);
                propertyBag.SetValue(WiggleXStartId, placement.AdditionalVisualPosition.X);
                propertyBag.SetValue(WiggleYStartId, placement.AdditionalVisualPosition.Y);
            }

            float xTarget = propertyBag.GetSingleValue(WiggleXTargetId);
            float yTarget = propertyBag.GetSingleValue(WiggleYTargetId);
            float xStart = propertyBag.GetSingleValue(WiggleXStartId);
            float yStart = propertyBag.GetSingleValue(WiggleYStartId);

            float progression = position / period;
            float x = MathHelper.Lerp(xStart, xTarget, progression);
            float y = MathHelper.Lerp(yStart, yTarget, progression);
            placement.AdditionalVisualPosition = new Vector3(x, y, 0);
        }
    }
}
