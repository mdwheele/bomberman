using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // Handles:
    //      - The player physically picking up powerups
    //      - Processing powerups by adjusting player stats
    class PowerUpSystem : EntitySystem
    {
        public PowerUpSystem(EntityGrid grid)
            : base(SystemOrders.Update.PowerUp, 
                new int[] { ComponentTypeIds.PowerUp, ComponentTypeIds.Placement },
                null
                )
        {
            this.entityGrid = grid;
        }

        private EntityGrid entityGrid;

        protected override void Initialize()
        {
            powerUpComponents = EntityManager.GetComponentManager<PowerUp>(ComponentTypeIds.PowerUp);
            placementComponents = EntityManager.GetComponentManager<Placement>(ComponentTypeIds.Placement);
        }

        private IComponentMapper<PowerUp> powerUpComponents;
        private IComponentMapper<Placement> placementComponents;

        private List<int> uniqueIdWorker = new List<int>();
        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            // We could use the physics system to handle grabbing powerups, but it is fairly straightforward to just do it here with our grid.
            foreach (int liveId in entityIdCollection)
            {
                Placement placement = placementComponents.GetComponentFor(liveId);
                entityGrid.QueryUniqueIdsAt(placement.Position.ToInteger(), uniqueIdWorker);
                foreach (int uniqueIdOverlap in uniqueIdWorker)
                {
                    Entity possiblePlayerEntity = EntityManager.GetEntityByUniqueId(uniqueIdOverlap);
                    PlayerInfo player = (PlayerInfo)EntityManager.GetComponent(possiblePlayerEntity, ComponentTypeIds.Player);
                    if (player != null)
                    {
                        // A player will pick this up!
                        PickUpPowerUp(possiblePlayerEntity.LiveId, player, powerUpComponents.GetComponentFor(liveId));
                        // Delete this powerup.
                        EntityManager.DelayFreeByLiveId(liveId, EntityFreeOptions.Deep);
                        break;
                    }
                }
            }
        }

        private void PickUpPowerUp(int entityLiveIdPlayer, PlayerInfo player, PowerUp powerUp)
        {
            Entity playerEntity = EntityManager.GetEntityByLiveId(entityLiveIdPlayer);
            //EntityManager.SendMessage(playerEntity, Messages.PlaySound, "PowerUp".CRC32Hash(), 1f, null);
            EntityManager.SendMessage(playerEntity, Messages.PlaySound, powerUp.SoundId, 1f, 0.3f, null);

            switch (powerUp.Type)
            {
                case PowerUpType.BombDown:
                    player.PermittedSimultaneousBombs = Math.Max(1, player.PermittedSimultaneousBombs - 1);
                    break;

                case PowerUpType.BombUp:
                    player.PermittedSimultaneousBombs++;
                    break;

                case PowerUpType.FireUp:
                    player.BombState.Range++;
                    break;

                case PowerUpType.FireDown:
                    player.BombState.Range = Math.Max(1, player.BombState.Range - 1);
                    break;

                case PowerUpType.SpeedUp:
                    player.MaxSpeed *= GameConstants.PlayerSpeedUp;
                    break;

                case PowerUpType.SpeedDown:
                    player.MaxSpeed = Math.Max(GameConstants.PlayerMinSpeed, player.MaxSpeed / GameConstants.PlayerSpeedUp);
                    break;

                case PowerUpType.FullFire:
                    player.BombState.Range = GameConstants.InfiniteRange;
                    break;

                case PowerUpType.PowerBomb:
                    player.FirstBombInfinite = true;
                    break;

                case PowerUpType.DangerousBomb:
                    player.BombState.PropagationDirection = PropagationDirection.All;
                    player.BombState.IsHardPassThrough = true;
                    break;

                case PowerUpType.PassThroughBomb:
                    player.BombState.IsPassThrough = true;
                    break;

                case PowerUpType.LandMineBomb:
                    player.FirstBombLandMine = true;
                    break;

                case PowerUpType.RemoteBomb:
                    if (!player.RemoteTrigger)
                    {
                        player.RemoteTrigger = true; // This is necessary so that we don't add countdowns to bombs.
                        // Also add remote trigger input handling to this guy.
                        InputHandlers inputHandlers = (InputHandlers)EntityManager.GetComponent(entityLiveIdPlayer, ComponentTypeIds.InputHandlers);
                        if (inputHandlers != null)
                        {
                            inputHandlers.InputHandlerIds.Add("RemoteTrigger".CRC32Hash());
                        }
                    }
                    break;
            }
        }
    }
}
