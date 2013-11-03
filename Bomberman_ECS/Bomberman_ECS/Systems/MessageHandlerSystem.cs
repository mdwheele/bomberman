using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // This is a bit of a bastardation of a system, since all it does is forward messages to callbacks.
    class MessageHandlerSystem : EntitySystem
    {
        public MessageHandlerSystem() : base(SystemOrders.Update.DontCare,
            new int[] {},
            new int[] { },
            new uint[] {},
            SystemFlags.HandleAllMessages
            )
        {
        }

        protected override void Initialize()
        {
        }

        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            if (target != null)
            {
                MessageHandler messageHandler = (MessageHandler)EntityManager.GetComponent(target, ComponentTypeIds.MessageHandler);
                if (messageHandler != null)
                {
                    int functionId;
                    if (messageHandler.TryGetHandler(message, out functionId))
                    {
                        MessageHandlers.Handlers.GetHandler(functionId)(EntityManager, target.LiveId, message, ref data);
                    }
                }
            }
            return 0;
        }
    }
}
