using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.MessageHandlers
{
    static partial class Handlers
    {
        public static void RemoteDetonate(EntityManager em, int entityLiveId, uint message, ref MessageData data)
        {
            // Trigger all the bombs that the player dropped.
            Debug.Assert(message == Messages.InExplosion);
            em.DelayFreeByLiveId(entityLiveId, EntityFreeOptions.Deep);
            data.Handled = true;
        }
    }
}
