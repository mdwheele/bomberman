using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.MessageHandlers
{
    static partial class Handlers
    {
        // This is a lot of boiler plate for stuff. Is it worth it?
        /*
         public static void SpeedUp(EntityManager em, int entityLiveId, uint message, ref MessageData data)
         {
             int uniqueIdCollider = data.Int32;
             Entity entityPlayer = em.TryGetEntityByUniqueId(uniqueIdCollider);
             Player player = (Player)em.GetComponent(entityPlayer, ComponentTypeIds.Player);
             if (player != null)
             {
                 player.MaxSpeed *= GameConstants.PlayerSpeedUp;

                 // REVIEW: Might need to delay free:
                 em.Free(entityLiveId, EntityFreeOptions.Deep);
                 data.Handled = true;
             }
         }*/
    }
}
