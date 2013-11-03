using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // Currently this keeps track of who won.
    // It could also be used to drive a HUD that displays information about a player
    class PlayerSystem : EntitySystem
    {
        public PlayerSystem()
            : base(SystemOrders.Update.DontCare,
                new int[] { ComponentTypeIds.Player },
                new uint[] { Messages.QueryWinningPlayer }
                )
        {
        }

        protected override void Initialize()
        {
            playerComponents = EntityManager.GetComponentManager<PlayerInfo>(ComponentTypeIds.Player);
        }

        private IComponentMapper<PlayerInfo> playerComponents;
        private int uniqueIdOfWinningPlayer;

        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            int uniqueId = EntityManager.InvalidEntityUniqueId;
            int calculatedPlayerCount = 0;
            foreach (int liveId in entityIdCollection)
            {
                uniqueId = EntityManager.GetEntityByLiveId(liveId).UniqueId;
                calculatedPlayerCount++;
            }

            if (calculatedPlayerCount == 1)
            {
                uniqueIdOfWinningPlayer = uniqueId;
            }
            else
            {
                uniqueIdOfWinningPlayer = EntityManager.InvalidEntityUniqueId;
            }
        }

        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            switch (message)
            {
                case Messages.QueryWinningPlayer:
                    data.SetIntResponse(uniqueIdOfWinningPlayer);
                    data.Handled = true;
                    break;
            }
            return 0;
        }
    }
}
