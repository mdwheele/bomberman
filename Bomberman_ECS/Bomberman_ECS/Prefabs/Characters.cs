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
        private static void MakeCharacters()
        {
            MakeCharacter("Player1",
                        Color.Orange,
                        1,
                        new InputMap
                        (
                        new KeyValuePair<Keys, int>[]
                        {
                            new KeyValuePair<Keys, int>(Keys.Space, InputActions.Drop),
                            new KeyValuePair<Keys, int>(Keys.Enter, InputActions.RemoteTrigger),
                        },
                        new KeyValuePair<Keys, int>[]
                        {
                            new KeyValuePair<Keys, int>(Keys.Down, InputStates.MoveDown),
                            new KeyValuePair<Keys, int>(Keys.Up, InputStates.MoveUp),
                            new KeyValuePair<Keys, int>(Keys.Right, InputStates.MoveRight),
                            new KeyValuePair<Keys, int>(Keys.Left, InputStates.MoveLeft),
                        }
                        )
                        { }
                        );

            MakeCharacter("Player2",
                        Color.Pink,
                        2,
                        new InputMap
                        (
                        new KeyValuePair<Keys, int>[]
                        {
                            new KeyValuePair<Keys, int>(Keys.Q, InputActions.Drop),
                            new KeyValuePair<Keys, int>(Keys.E, InputActions.RemoteTrigger),
                        },
                        new KeyValuePair<Keys, int>[]
                        {
                            new KeyValuePair<Keys, int>(Keys.S, InputStates.MoveDown),
                            new KeyValuePair<Keys, int>(Keys.W, InputStates.MoveUp),
                            new KeyValuePair<Keys, int>(Keys.D, InputStates.MoveRight),
                            new KeyValuePair<Keys, int>(Keys.A, InputStates.MoveLeft),
                        }
                        ) { }
                        );


            MakeCharacter("Player3",
                        Color.Cyan,
                        3,
                        new InputMap
                        (
                        new KeyValuePair<Buttons, int>[]
                        {
                            new KeyValuePair<Buttons, int>(Buttons.A, InputActions.Drop),
                            new KeyValuePair<Buttons, int>(Buttons.B, InputActions.RemoteTrigger),
                        },
                        new KeyValuePair<Buttons, int>[]
                        {
                            new KeyValuePair<Buttons, int>(Buttons.LeftThumbstickDown, InputStates.MoveDown),
                            new KeyValuePair<Buttons, int>(Buttons.LeftThumbstickUp, InputStates.MoveUp),
                            new KeyValuePair<Buttons, int>(Buttons.LeftThumbstickRight, InputStates.MoveRight),
                            new KeyValuePair<Buttons, int>(Buttons.LeftThumbstickLeft, InputStates.MoveLeft),
                        }
                        ) { }
                        );
        
        }

        private static void MakeCharacter(string name, Color tint, int number, InputMap inputMap)
        {
            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    name,
                    new Placement()
                    {
                        Layer = 3,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = "Man".CRC32Hash(),
                        Tint = tint,
                        Size = new Vector2(1.1f),
                    },
                    new Physics()
                    {
                        BoundingVolumeType = BoundingVolumeType.Circle,
                        IsDynamic = true,
                        Size = 0.95f,
                        CollisionCategories = CollisionCategory.AllPlayers,
                        CollidesWidth = CollisionCategory.Bricks | CollisionCategory.Bombs,
                    },
                    inputMap,
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.None,
                        ShouldSendMessage = true,
                    },
                    new InputHandlers("DropBomb", "MovePlayer")
                    {
                    },
                    // TODO: Update with a better handler that does animations, etc...
                    new MessageHandler(new MessageAndHandler(Messages.InExplosion, "KillPlayer".CRC32Hash()),
                                       new MessageAndHandler(Messages.DirectKill, "KillPlayer".CRC32Hash()))
                    {
                    },
                    new PlayerInfo()
                    {
                        MaxSpeed = GameConstants.PlayerDefaultSpeed,
                        PermittedSimultaneousBombs = 1,
                        BombState = new BombState()
                        {
                            PropagationDirection = PropagationDirection.NESW,
                            Range = 1,
                        },
                        PlayerNumber = number,
                    }
                    )
                );
        }
    }
}
