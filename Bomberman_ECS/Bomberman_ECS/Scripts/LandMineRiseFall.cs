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
        private static int LandMineIsTriggeredId = "LandMine_IsTriggered".CRC32Hash();
        private static int LandMineFuseTimeId = "LandMine_FuseTime".CRC32Hash();
        private static int LandMineFuseCountdownId = "LandMine_FuseCountdown".CRC32Hash();
        private static int LandMineAreaClearedId = "LandMine_AreaCleared".CRC32Hash();
        public static void LandMineRiseFall_Init(ScriptContainer scriptContainer, float fuseTime)
        {
            scriptContainer.PropertyBag.SetValue(LandMineFuseTimeId, fuseTime);
        }

        // Outline:
        //      - Each frame, it checks to see if someone is on it. If so, it's triggered.
        //      - If it's not triggered, it advances animation to max, at a certain framerate (could use FrameAnimation)
        //      - If it's triggered, it counts down
        public static void LandMineRiseFall(EntityManager em, ScriptContainer scriptContainer, int entityLiveId, GameTime gameTime)
        {
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool isTriggered = scriptContainer.PropertyBag.GetBooleanValue(LandMineIsTriggeredId);
            if (!isTriggered)
            {
                Placement placement = (Placement)em.GetComponent(entityLiveId, ComponentTypeIds.Placement);
                Point spot = placement.Position.ToInteger();
                MessageData data = new MessageData(spot.X, spot.Y, ComponentTypeIds.Player);
                em.SendMessage(Messages.QueryComponentAtGrid, ref data, null);
                if (data.Handled)
                {
                    // Before allowing it to be triggered, the area must be cleared (so the player who dropped it doesn't trigger it)
                    // Check to see if a Player component is on there, and if so set outselves to triggered.
                    if (data.Int32 == EntityManager.InvalidEntityUniqueId)
                    {
                        // No one on it. Now it's ok to be triggered.
                        scriptContainer.PropertyBag.SetValue(LandMineAreaClearedId, true);
                    }

                    bool areaCleared = scriptContainer.PropertyBag.GetBooleanValue(LandMineAreaClearedId);
                    if (areaCleared && (data.Int32 != EntityManager.InvalidEntityUniqueId))
                    {
                        // Someone is on it.
                        isTriggered = true;
                        scriptContainer.PropertyBag.SetValue(LandMineIsTriggeredId, isTriggered);
                        scriptContainer.PropertyBag.SetValue(LandMineFuseCountdownId, scriptContainer.PropertyBag.GetSingleValue(LandMineFuseTimeId));
                    }
                }
            }

            Entity entity = em.GetEntityByLiveId(entityLiveId);
            if (isTriggered)
            {
                float timeLeft = scriptContainer.PropertyBag.GetSingleValue(LandMineFuseCountdownId);
                timeLeft -= ellapsed;
                scriptContainer.PropertyBag.SetValue(LandMineFuseCountdownId, timeLeft);

                if (timeLeft <= 0f)
                {
                    em.SendMessage(entity, Messages.TriggerBomb, null); // BOOM
                }
            }

            // Move up or down based on whether we're triggered.
            bool allocatedNew;
            FrameAnimation frameAnimation = (FrameAnimation)em.AddComponentToEntity(entity, ComponentTypeIds.FrameAnimation, out allocatedNew);
            frameAnimation.Direction = isTriggered ? -1 : 1;
            frameAnimation.Loop = false;
            frameAnimation.FrameRate = GameConstants.LandMineAnimationFrameRate;

        }
    }
}
