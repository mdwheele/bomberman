using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;
using Bomberman_ECS.Util;

namespace Bomberman_ECS.MessageHandlers
{
    static partial class Handlers
    {
        public static void KillPlayer(EntityManager em, int entityLiveId, uint message, ref MessageData data)
        {
            // TODO: This would send a message to the PlayerSystem, which could make a dying animation.
            // (or we could do it here)
            Debug.Assert((message == Messages.HitByInitialExplosion) || (message == Messages.InExplosion) || (message == Messages.DirectKill));
            em.DelayFreeByLiveId(entityLiveId, EntityFreeOptions.Deep);
            data.Handled = true;
        }

        public static void SimplyDestroy(EntityManager em, int entityLiveId, uint message, ref MessageData data)
        {
            Debug.Assert((message == Messages.HitByInitialExplosion) || (message == Messages.InExplosion) || (message == Messages.DirectKill));
            em.DelayFreeByLiveId(entityLiveId, EntityFreeOptions.Deep);
            data.Handled = true;
        }

        public static void TriggerOnExplosion(EntityManager em, int entityLiveId, uint message, ref MessageData data)
        {
            Debug.Assert(message == Messages.HitByInitialExplosion);
            em.SendMessage(em.GetEntityByLiveId(entityLiveId), Messages.TriggerBomb, null);
            data.Handled = true;
        }

        private static Random random = new Random();
        struct PowerUpProbabilities
        {
            public PowerUpProbabilities(string templateName, float probablity)
            {
                TemplateName = templateName;
                Probability = probablity;
            }
            public string TemplateName;
            public float Probability;
        }
        private static PowerUpProbabilities[] powerUpTemplates = new PowerUpProbabilities[]
        {
            new PowerUpProbabilities("BombUp", 1f),
            new PowerUpProbabilities("FireUp", 1f),
            new PowerUpProbabilities("SpeedUp", 1f),
            new PowerUpProbabilities("FullFire", 0.1f),

            new PowerUpProbabilities("PowerBomb", 0.5f),
            new PowerUpProbabilities("DangerousBomb", 0.5f),
            new PowerUpProbabilities("PassThroughBomb", 0.5f),
            new PowerUpProbabilities("RemoteBomb", 0.5f),

            new PowerUpProbabilities("LandMine", 0.4f),
            new PowerUpProbabilities("BombDown", 0.2f),
            new PowerUpProbabilities("FireDown", 0.2f),
            new PowerUpProbabilities("SpeedDown", 0.2f),
        };

        public static void DestroyOnExplosionAndRevealPowerUp(EntityManager em, int entityLiveId, uint message, ref MessageData data)
        {
            // TODO: There is a bug where two power ups can appear in the same spot if that block is hit by
            // two explosions in the same update cycle. We need to check if the block is on the delete list (since 
            // we delay deletion). An alternative is that we could just remove the handler?
            Debug.Assert(message == Messages.HitByInitialExplosion);
            em.DelayFreeByLiveId(entityLiveId, EntityFreeOptions.Deep);

            if (random.NextDouble() < GameConstants.PowerUpChance)
            {
                // Choose based on weights
                float totalWeight = 0f;
                foreach (PowerUpProbabilities pup in powerUpTemplates)
                {
                    totalWeight += pup.Probability;
                }
                float choice = (float)random.NextDouble();
                string template = null;
                float accWeight = 0f;
                foreach (PowerUpProbabilities pup in powerUpTemplates)
                {
                    accWeight += pup.Probability;
                    if (choice <= (accWeight / totalWeight))
                    {
                        template = pup.TemplateName;
                        break;
                    }
                }

                Entity entityPowerUp = em.AllocateForGeneratedContent(template, Universe.TopLevelGroupUniqueIdBase);
                Helpers.TransferPlacement(em, entityLiveId, entityPowerUp.LiveId);
                // Depending on our game settings, power ups may or may not be affected by explosions
                if (true) // For now we'll always assume they are
                {
                    bool allocatedNew;
                    MessageHandler messageHandler = (MessageHandler)em.AddComponentToEntity(entityPowerUp, ComponentTypeIds.MessageHandler, out allocatedNew);
                    messageHandler.Add(new MessageAndHandler(Messages.HitByInitialExplosion, "SimplyDestroy".CRC32Hash()));
                }
            }
            data.Handled = true;
        }
    }
}
