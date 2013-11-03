using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.InputHandlerCallbacks
{
    static partial class Callbacks
    {
        public static void MovePlayer(EntityManager em, int entityLiveId, List<int> actions, List<int> states)
        {
            Physics physics = (Physics)em.GetComponent(entityLiveId, ComponentTypeIds.Physics);
            if (physics != null)
            {
                Vector3 velocity = Vector3.Zero;

                for (int i = 0; i < moveStates.Length; i++)
                {
                    if (states.Contains(moveStates[i]))
                    {
                        states.Remove(moveStates[i]);
                        velocity += moveVelocities[i];
                    }
                }

                PlayerInfo player = (PlayerInfo)em.GetComponent(entityLiveId, ComponentTypeIds.Player);

                if (player != null)
                {
                    // We apply max speed on each axis, instead of for the overall vector.
                    velocity.X *= player.MaxSpeed;
                    velocity.Y *= player.MaxSpeed;
                }
                MessageData data = new MessageData(velocity.XY());
                em.SendMessage(em.GetEntityByLiveId(entityLiveId), Messages.SetVelocity, ref data, null);
            }
        }

        private static int[] moveStates = new int[] { InputStates.MoveDown, InputStates.MoveUp, InputStates.MoveRight, InputStates.MoveLeft };
        private static Vector3[] moveVelocities = new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector3(1, 0, 0), new Vector3(-1, 0, 0) };
    }
}
