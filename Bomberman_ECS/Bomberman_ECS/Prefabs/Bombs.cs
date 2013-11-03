using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Prefabs
{
    static partial class Prefabs
    {
        private static void MakeBombs()
        {
            ScriptContainer scriptContainer1 = new ScriptContainer("Pulsate".CRC32Hash()) { };
            Scripts.Scripts.Pulsate_Init(scriptContainer1, period: GameConstants.PulsationPeriod);

            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "BasicBomb",
                    new Placement()
                    {
                        Layer = 2,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "Bomb".CRC32Hash(),            // Possibly Over-ridden when we create the bomb.
                        Tint = Color.White,
                        Size = new Vector2(1.12f),
                    },
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.None,
                        ShouldSendMessage = true,
                    },
                    new MessageHandler(
                        new MessageAndHandler(Messages.HitByInitialExplosion, "TriggerOnExplosion".CRC32Hash())
                        )
                    {
                    },
                    new Bomb()
                    {
                        Countdown = GameConstants.BombSecondsToExplode,
                        // Other things are filled in when we create the bomb.
                    },
                    scriptContainer1,
                    new Physics()
                    {
                        IsSensor = true,
                        BoundingVolumeType = BoundingVolumeType.Box,
                        Size = 1f,
                        IsDynamic = false,  // REVIEW: We'll need to modify this in the case of throwing bombs.
                        CollisionCategories = CollisionCategory.Bombs,
                        CollidesWidth = CollisionCategory.AllPlayers,
                    }
                    )
                );

            // Land mines are different enough that we'll use a different prefab
            ScriptContainer scriptContainer2 = new ScriptContainer("LandMineRiseFall".CRC32Hash()) { };
            Scripts.Scripts.LandMineRiseFall_Init(scriptContainer2, GameConstants.LandMineFuseTime);

            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "LandMineBomb",
                    new Placement()
                    {
                        Layer = 2,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "LandMine".CRC32Hash(),            // Possibly Over-ridden when we create the bomb.
                        Tint = new Color(196, 196, 196, 255),
                        Size = new Vector2(1.12f),
                    },
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.None,
                        ShouldSendMessage = true,
                    },
                    new MessageHandler(
                        new MessageAndHandler(Messages.HitByInitialExplosion, "TriggerOnExplosion".CRC32Hash())
                        )
                    {
                    },
                    new Bomb()
                    {
                        Countdown = GameConstants.BombSecondsToExplode,
                        // Other things are filled in when we create the bomb.
                    },
                    scriptContainer2
                    // No physics for land mines, since we can walk over them.
                    )
                );
        }
    }
}
