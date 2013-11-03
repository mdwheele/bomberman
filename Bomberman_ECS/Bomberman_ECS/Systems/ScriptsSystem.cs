using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    class ScriptsSystem : EntitySystem
    {
        public ScriptsSystem()
            : base(
            SystemOrders.Update.Scripts,
            new int[] { ComponentTypeIds.ScriptContainer },
            new uint[] { Messages.AddScript, Messages.RemoveScript }
            )
        {
        }

        protected override void Initialize()
        {
            scriptsComponents = EntityManager.GetComponentManager<ScriptContainer>(ComponentTypeIds.ScriptContainer);
        }

        IComponentMapper<ScriptContainer> scriptsComponents;

        private List<int> scriptIdsWorker = new List<int>();
        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            foreach (int liveId in entityIdCollection)
            {
                ScriptContainer scriptContainer = scriptsComponents.GetComponentFor(liveId);
                // Make a temp copy of things incase stuff is removed.
                scriptIdsWorker.Clear();
                scriptIdsWorker.AddRange(scriptContainer.GetScriptIds());
                foreach (int scriptId in scriptIdsWorker)
                {
                    // Call each script
                    Scripts.Scripts.GetScript(scriptId)(EntityManager, scriptContainer, liveId, gameTime);
                }
            }
        }
    }
}
