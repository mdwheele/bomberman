using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Prefabs
{
    static partial class Prefabs
    {
        private static void MakeGame()
        {
            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    "MainGame",
                    new GameState()
                    {
                        TimeRemaining = GameConstants.GameLength
                    },
                    new InputMap(
                        new KeyValuePair<Keys, int>[]
                        {
                            new KeyValuePair<Keys, int>(Keys.Space, InputActions.RestartGame),
                        }
                        )
                        {
                        }
                    // We'll add the RestartGame action handler dynamically after
                    )
                );
        }
    }
}
