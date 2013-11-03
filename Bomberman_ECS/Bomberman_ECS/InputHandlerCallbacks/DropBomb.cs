using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;
using Bomberman_ECS.Prefabs;

namespace Bomberman_ECS.InputHandlerCallbacks
{
    static partial class Callbacks
    {
        public static void DropBomb(EntityManager em, int entityLiveId, List<int> actions, List<int> states)
        {
            if (actions.Contains(InputActions.Drop))
            {
                actions.Remove(InputActions.Drop);

                Entity playerEntity = em.GetEntityByLiveId(entityLiveId);
                int playerUniqueId = playerEntity.UniqueId;

                // First, we need to figure out if we can actually place a bomb. We might be at our limit.
                IComponentMapper<Bomb> bombs = em.GetComponentManager<Bomb>(ComponentTypeIds.Bomb);
                int existingBombsForThisPlayer = 0;
                // TODO: The enumerator produces garbage
                foreach (Bomb existingBomb in bombs.EnumerateComponents())
                {
                    if (existingBomb.OwnerUniqueId == playerUniqueId)
                    {
                        existingBombsForThisPlayer++;
                    }
                }

                PlayerInfo player = (PlayerInfo)em.GetComponent(entityLiveId, ComponentTypeIds.Player);
                if (player != null)
                {
                    Placement placementSource = (Placement)em.GetComponent(entityLiveId, ComponentTypeIds.Placement);

                    bool canPlace = (existingBombsForThisPlayer < player.PermittedSimultaneousBombs);
                    if (canPlace)
                    {
                        // Also, if there is already a bomb there, can't place.
                        Point spot = placementSource.Position.ToInteger();
                        MessageData data = new MessageData(spot.X, spot.Y, ComponentTypeIds.Bomb);
                        em.SendMessage(Messages.QueryComponentAtGrid, ref data, null);
                        canPlace = !data.Handled || (data.Int32 == EntityManager.InvalidEntityUniqueId);
                    }

                    if (canPlace)
                    {
                        bool firstBomb = (existingBombsForThisPlayer == 0);
                        bool replacedTexture = false;

                        bool placedLandMine = false;
                        string bombTemplate = "BasicBomb";
                        if (player.FirstBombLandMine)
                        {
                            bombTemplate = "LandMineBomb";
                            placedLandMine = true;
                            replacedTexture = true;
                        }

                        Entity bombEntity = em.AllocateForGeneratedContent(bombTemplate, Universe.TopLevelGroupUniqueIdBase);
                        Placement placementBomb = (Placement)em.GetComponent(bombEntity, ComponentTypeIds.Placement);
                        Aspect aspectBomb = (Aspect)em.GetComponent(bombEntity, ComponentTypeIds.Aspect);
                        if ((placementBomb != null) && (placementSource != null))
                        {
                            placementBomb.SetPositionWholeNumber(placementSource.Position);
                        }

                        em.SendMessage(bombEntity, Messages.PlaySound, "DropBomb".CRC32Hash(), 0.5f, null);

                        // Set ourselves as the owner of the bomb.
                        Bomb bomb = (Bomb)em.GetComponent(bombEntity, ComponentTypeIds.Bomb);
                        if (bomb != null)
                        {
                            bomb.OwnerUniqueId = playerUniqueId;

                            bomb.State = player.BombState;

                            if (firstBomb)
                            {
                                if (player.FirstBombInfinite)
                                {
                                    bomb.State.Range = GameConstants.InfiniteRange;
                                    // Mark it specially, since this is a power bomb!
                                    aspectBomb.ModelNameId = "BombPower".CRC32Hash();
                                    replacedTexture = true;
                                }
                                // REVIEW: What about when it's a landmine???
                            }

                            if (player.BombState.IsPassThrough)
                            {
                                aspectBomb.ModelNameId = "BombSpiked".CRC32Hash();
                                replacedTexture = true;
                            }

                            if (bomb.State.PropagationDirection == PropagationDirection.All)
                            {
                                // Limit the range
                                bomb.State.Range = Math.Min(bomb.State.Range, GameConstants.MaxDangerousBombRange);
                                // And override the model
                                aspectBomb.ModelNameId = "BombDangerous".CRC32Hash();
                                replacedTexture = true;
                            }

                            // Don't have a countdown if the player can trigger bombs remotely.
                            if (player.RemoteTrigger || placedLandMine)
                            {
                                bomb.Countdown = GameConstants.InfiniteTime;
                            }

                            // Show remote control bombs as different, but only if we don't mark the
                            // bomb differently otherwise.
                            if (player.RemoteTrigger && !replacedTexture)
                            {
                                aspectBomb.ModelNameId = "BombRC".CRC32Hash();
                            }
                        }
                    }
                    else
                    {
                        em.SendMessage(playerEntity, Messages.PlaySound, "Buzzer".CRC32Hash(), 1f, null);
                    }
                }
            }
        }
    }
}
