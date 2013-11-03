using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    public class SystemManager
    {
        public SystemManager(EntityManager entityManager)
        {
            entityManager.SystemManager = this;
            Systems = new List<EntitySystem>();
            SystemsForDraw = new List<EntitySystem>();
            this.entityManager = entityManager;
        }

        private EntityManager entityManager;

        internal List<EntitySystem> Systems { get; private set; }
        internal List<EntitySystem> SystemsForDraw { get; private set; }

        public List<EntitySystem> EnumerateSystems() { return Systems; }    // List to avoid garbage

        private static int SortByUpdateOrder(EntitySystem s1, EntitySystem s2)
        {
            return s1.UpdateOrder - s2.UpdateOrder;
        }

        private static int SortByDrawOrder(EntitySystem s1, EntitySystem s2)
        {
            return s1.DrawOrder - s2.DrawOrder;
        }

        public void AddSystem(EntitySystem system)
        {
            system.CallInitialize(entityManager);
            Systems.Add(system);
            Systems.Sort(SortByUpdateOrder);
            SystemsForDraw.Add(system);
            SystemsForDraw.Sort(SortByDrawOrder);

            if (system.SupportedMessages != null)
            {
                foreach (uint message in system.SupportedMessages)
                {
                    Debug.Assert(!messageSystemDispatch.ContainsKey(message), "A system already registed for " + message);
                    messageSystemDispatch[message] = system;
                }
            }

            if ((system.Flags & SystemFlags.HandleAllMessages) == SystemFlags.HandleAllMessages)
            {
                globalMessageHandlers.Add(system);
            }
        }

        private Dictionary<uint, EntitySystem> messageSystemDispatch = new Dictionary<uint, EntitySystem>();
        private List<EntitySystem> globalMessageHandlers = new List<EntitySystem>();

        internal int SendMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            int result = 0;
            EntitySystem system;
            if (messageSystemDispatch.TryGetValue(message, out system))
            {
                result = system.SendMessage(target, message, ref data, sender);
            }

            // We now allow for global message handlers
            if (!data.Handled)
            {
                foreach (EntitySystem system2 in globalMessageHandlers)
                {
                    system2.SendMessage(target, message, ref data, sender);
                    if (data.Handled)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        internal void UpdateSystems(Microsoft.Xna.Framework.GameTime gameTime)
        {
            foreach (EntitySystem system in Systems)
            {
                system.ProcessEntities(gameTime);
            }
        }

        internal void DrawSystems(Microsoft.Xna.Framework.GameTime gameTime)
        {
            foreach (EntitySystem system in SystemsForDraw)
            {
                system.Draw(gameTime);
            }
        }

    }
}
