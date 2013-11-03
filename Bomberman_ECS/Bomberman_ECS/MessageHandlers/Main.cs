using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.MessageHandlers
{
    delegate void MessageHandlerCallback(EntityManager em, int entityLiveId, uint message, ref MessageData data);

    static partial class Handlers
    {
        // We want to be able to serialize callbacks with an int id. This is a hacky way that lets us do that
        // with reflection without having to explicitly register each one.
        public static void Initialize()
        {
            Type type = typeof(Handlers);
            foreach (MethodInfo mi in type.GetMethods())
            {
                // Identify ones that match the callbacks
                MessageHandlerCallback temp = null;
                try
                {
                    temp = (MessageHandlerCallback)Delegate.CreateDelegate(typeof(MessageHandlerCallback), mi);
                }
                catch (Exception)
                {
                }
                if (temp != null)
                {
                    idToHandler[mi.Name.CRC32Hash()] = temp;
                }
            }
        }

        private static Dictionary<int, MessageHandlerCallback> idToHandler = new Dictionary<int, MessageHandlerCallback>();
        public static MessageHandlerCallback GetHandler(int id) { return idToHandler[id]; }
    }
}
