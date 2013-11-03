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
        public static void RemoteTrigger(EntityManager em, int entityLiveId, List<int> actions, List<int> states)
        {
            if (actions.Contains(InputActions.RemoteTrigger))
            {
                actions.Remove(InputActions.RemoteTrigger);
                // The logic needs to be in the system, since it requires access to the entire bomb entity.
                em.SendMessage(em.GetEntityByLiveId(entityLiveId), Messages.RemoteTriggerAllPlayerBombs, null);
            }
        }
    }
}
