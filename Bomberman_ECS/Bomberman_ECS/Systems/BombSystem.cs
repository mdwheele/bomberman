using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // Handles triggering bombs, and the transition to explosions.
    // Bombs can be triggered as follows:
    //  - countdown timer
    //  - explicitly via TriggerBomb message
    class BombSystem : EntitySystem
    {
        public BombSystem()
            : base(SystemOrders.Update.Grid, 
                new int[] { ComponentTypeIds.Bomb, ComponentTypeIds.Placement },
                new uint[] { Messages.TriggerBomb, Messages.RemoteTriggerAllPlayerBombs }
                )
        {
        }

        protected override void Initialize()
        {
            placementComponents = EntityManager.GetComponentManager<Placement>(ComponentTypeIds.Placement);
            bombComponents = EntityManager.GetComponentManager<Bomb>(ComponentTypeIds.Bomb);
        }

        private IComponentMapper<Placement> placementComponents;
        private IComponentMapper<Bomb> bombComponents;

        private List<int> removeIdWorker = new List<int>();
        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            removeIdWorker.Clear();
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (int liveId in entityIdCollection)
            {
                Placement placement = placementComponents.GetComponentFor(liveId);
                Bomb bomb = bombComponents.GetComponentFor(liveId);
                bomb.Countdown -= ellapsed;
                if (bomb.Countdown <= 0f)
                {
                    removeIdWorker.Add(liveId);
                    TriggerBomb(placementComponents.GetComponentFor(liveId), bomb);
                }
            }

            EntityManager.DelayFreeByLiveId(removeIdWorker, EntityFreeOptions.Deep);
        }

        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            switch (message)
            {
                case Messages.TriggerBomb:
                    {
                        Placement placement = placementComponents.GetComponentFor(target);
                        Bomb bomb = bombComponents.GetComponentFor(target);
                        if ((placement != null) && (bomb != null))
                        {
                            TriggerBomb(placement, bomb);
                            EntityManager.DelayFreeByLiveId(target.LiveId, EntityFreeOptions.Deep);
                            data.Handled = true;
                        }
                    }
                    break;

                case Messages.RemoteTriggerAllPlayerBombs:
                    {
                        int playerUniqueId = target.UniqueId;
                        foreach (int bombLiveId in GetEntityLiveIds())
                        {
                            Bomb bomb = bombComponents.GetComponentFor(bombLiveId);
                            if (bomb.OwnerUniqueId == playerUniqueId)
                            {
                                // Why not just send ourselves a message. That's easy.
                                EntityManager.SendMessage(EntityManager.GetEntityByLiveId(bombLiveId), Messages.TriggerBomb, null);
                            }
                        }
                        data.Handled = true;
                    }
                    break;
            }
            return 0;
        }

        private void TriggerBomb(Placement placement, Bomb bomb)
        {
            // We delay free bombs, so it's possible the same bomb could be triggered twice in the same update cycle.
            // To prevent this, we have a Triggered property on Bomb.
            if (!bomb.Triggered)
            {
                bomb.Triggered = true;
                string explosionTemplate = "Explosion";
                if (bomb.State.IsPassThrough && !bomb.State.IsHardPassThrough)  // Hard pass through is not blue
                {
                    explosionTemplate = "ExplosionBlue";
                }
                Entity explosionEntity = EntityManager.AllocateForGeneratedContent(explosionTemplate, Universe.TopLevelGroupUniqueIdBase);
                Explosion explosion = (Explosion)EntityManager.GetComponent(explosionEntity, ComponentTypeIds.Explosion);

                EntityManager.SendMessage(explosionEntity, Messages.PlaySound, "Explosion".CRC32Hash(), GameConstants.ExplosionVolume, null);

                explosion.State = bomb.State;

                Placement placementExplosion = placementComponents.GetComponentFor(explosionEntity);
                placementExplosion.Position = placement.Position;
            }
        }
    }
}
