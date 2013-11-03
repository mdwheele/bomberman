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
        private static void MakeBricks()
        {
            // Hard blocks are permanent.
            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "HardBlock",
                    new Placement()
                    {
                        Layer = 1,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "Brick".CRC32Hash(),
                        Tint = new Color(196, 196, 196, 255),
                        Size = new Vector2(1.12f),
                    },
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.Hard,
                    },
                    new Physics()
                    {
                        BoundingVolumeType = BoundingVolumeType.Box,
                        IsDynamic = false,
                        Size = 1f,
                        CollisionCategories = CollisionCategory.Bricks,
                        CollidesWidth = CollisionCategory.AllPlayers,
                    }
                    )
                );

            // Soft blocks disintegrate when hit by an explosion.
            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "SoftBlock",
                    new Placement()
                    {
                        Layer = 1,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "SoftBrick".CRC32Hash(),
                        Tint = new Color(128, 128, 128, 255),
                        Size = new Vector2(1.12f),
                    },
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.Soft,
                        ShouldSendMessage = true,
                    },
                    new Physics()
                    {
                        BoundingVolumeType = BoundingVolumeType.Box,
                        IsDynamic = false,
                        Size = 1f,
                        CollisionCategories = CollisionCategory.Bricks,
                        CollidesWidth = CollisionCategory.AllPlayers,
                    },
                    new MessageHandler(
                        new MessageAndHandler(Messages.HitByInitialExplosion, "DestroyOnExplosionAndRevealPowerUp".CRC32Hash())
                        )
                    {
                    }
                    )
                );

            // Death blocks fall from the sky at the end.
            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "DeathBlock",
                    new Placement()
                    {
                        Layer = 5,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "EndBrick".CRC32Hash(),
                        Tint = new Color(196, 196, 196, 255),
                        Size = new Vector2(1.12f),
                    },
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.Hard,
                    },
                    new Physics()
                    {
                        BoundingVolumeType = BoundingVolumeType.Box,
                        IsDynamic = false,
                        Size = 1f,
                        CollisionCategories = CollisionCategory.Bricks,
                        CollidesWidth = CollisionCategory.AllPlayers,
                    }
                    )
                );
        }
    }
}
