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
        private static void MakeExplosions()
        {
            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "Explosion",
                    new Placement()
                    {
                        Layer = 4,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "Explosion".CRC32Hash(),
                        Tint = Color.White,
                        Size = new Vector2(1.7f),
                    },
                    new Explosion()
                    {
                        Countdown = GameConstants.ExplosionDuration,
                        PropagationCountDown = GameConstants.PropagationTime,
                    },
                    new FrameAnimation()
                    {
                        FrameRate = GameConstants.ExplosionFrameRate,
                    }
                    )
                );

            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "ExplosionBlue",
                    new Placement()
                    {
                        Layer = 4,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "ExplosionBlue".CRC32Hash(),
                        Tint = Color.White,
                        Size = new Vector2(1.7f),
                    },
                    new Explosion()
                    {
                        Countdown = GameConstants.ExplosionDuration,
                        PropagationCountDown = GameConstants.PropagationTime,
                    },
                    new FrameAnimation()
                    {
                        FrameRate = GameConstants.ExplosionFrameRate,
                    }
                    )
                );

        }
    }
}
