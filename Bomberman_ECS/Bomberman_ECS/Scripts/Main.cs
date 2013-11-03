using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Scripts
{
    delegate void ScriptCallback(EntityManager em, ScriptContainer scriptContainer, int entityLiveId, GameTime gameTime);

    static partial class Scripts
    {
        // We want to be able to serialize callbacks with an int id. This is a hacky way that lets us do that
        // with reflection without having to explicitly register each one.
        public static void Initialize()
        {
            Type type = typeof(Scripts);
            foreach (MethodInfo mi in type.GetMethods())
            {
                // Identify ones that match the callbacks
                ScriptCallback temp = null;
                try
                {
                    temp = (ScriptCallback)Delegate.CreateDelegate(typeof(ScriptCallback), mi);
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

        private static Dictionary<int, ScriptCallback> idToHandler = new Dictionary<int, ScriptCallback>();
        public static ScriptCallback GetScript(int id) { return idToHandler[id]; }

        private static Random random = new Random();
    }
}
