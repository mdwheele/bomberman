using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.InputHandlerCallbacks
{
    delegate void InputCallback(EntityManager em, int entityLiveId, List<int> actions, List<int> states);

    static partial class Callbacks
    {
        // We want to be able to serialize callbacks with an int id. This is a hacky way that lets us do that
        // with reflection without having to explicitly register each one.
        public static void Initialize()
        {
            Type type = typeof(Callbacks);
            foreach (MethodInfo mi in type.GetMethods())
            {
                // Identify ones that match the callbacks
                InputCallback temp = null;
                try
                {
                    temp = (InputCallback)Delegate.CreateDelegate(typeof(InputCallback), mi);
                }
                catch (Exception)
                {
                }
                if (temp != null)
                {
                    idToCallbacks[mi.Name.CRC32Hash()] = temp;
                }
            }
        }

        private static Dictionary<int, InputCallback> idToCallbacks = new Dictionary<int, InputCallback>();

        public static InputCallback GetCallback(int id) { return idToCallbacks[id]; }
    }
}
