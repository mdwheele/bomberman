using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.InputHandlerCallbacks
{
    static partial class Callbacks
    {
        public static void RestartGame(EntityManager em, int entityLiveId, List<int> actions, List<int> states)
        {
            if (actions.Contains(InputActions.RestartGame))
            {
                actions.Remove(InputActions.RestartGame);
                MessageData data = new MessageData(0);
                em.SendMessage(Messages.LoadLevel, ref data, null);
            }
        }
    }
}
