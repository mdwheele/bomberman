using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // Responsible for assigning an entity to a grid.
    // This is essential for other systems to access
    class GridSystem : EntitySystem
    {
        public GridSystem()
            : base(SystemOrders.Update.Grid, 
                new int[] { ComponentTypeIds.Placement },
                new uint[] { Messages.QueryComponentAtGrid }
                )
        {
            entityGrid = new EntityGrid();
        }

        private EntityGrid entityGrid;
        public EntityGrid EntityGrid { get { return entityGrid; } }

        protected override void Initialize()
        {
            placementComponents = EntityManager.GetComponentManager<Placement>(ComponentTypeIds.Placement);
        }

        private IComponentMapper<Placement> placementComponents;

        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            entityGrid.Clear();
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (int liveId in entityIdCollection)
            {
                Placement placement = placementComponents.GetComponentFor(liveId);

                int x = (int)Math.Round(placement.Position.X);
                int y = (int)Math.Round(placement.Position.Y);

                entityGrid.Assign(x, y, EntityManager.GetEntityByLiveId(liveId).UniqueId);
            }
        }

        private List<int> uniqueIdWorker = new List<int>();
        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            switch (message)
            {
                    // This returns the unique id of the first entity at a particular spot that contains a particular component.
                case Messages.QueryComponentAtGrid:
                    int uniqueIdFound = EntityManager.InvalidEntityUniqueId;
                    entityGrid.QueryUniqueIdsAt(data.Int32, data.Int32Alt, uniqueIdWorker);
                    foreach (int uniqueIdConsider in uniqueIdWorker)
                    {
                        if (EntityManager.GetComponent(EntityManager.GetEntityByUniqueId(uniqueIdConsider), data.Int32AltAlt) != null)
                        {
                            uniqueIdFound = uniqueIdConsider;
                            break;
                        }
                    }
                    data.SetIntResponse(uniqueIdFound);
                    data.Handled = true;
                    break;
            }
            return 0;
        }
    }
}
