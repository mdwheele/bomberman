#define ALTERNATE_LAYOUT
// Alternate layout means we organize the componentLookupIds so that they are in a good order for enumerating through a type of component.
// It does mean a poor order for enumerating through all components in an entity (like when you add/remove entities)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace Bomberman_ECS.Core
{
    public enum EntityEnumeration
    {
        Shallow,
        Deep
    }

    public class EntityManager
    {
        public const int MaxEntities = 1000;
        public const int MaxComponentTypes = 32;
        public delegate void EnumEntityHandler(Entity e);

        public const int InvalidEntityUniqueId = -1;
        public const int InvalidEntityLiveId = -1;

        private SystemManager systemManager;
        public SystemManager SystemManager
        {
            get { return systemManager; }
            set
            {
                systemManager = value;
            }
        }

        private Universe universe;
        private static Dictionary<Type, byte> typeToComponentId = new Dictionary<Type, byte>();
        public static int GetComponentIdForComponent(Component component)
        {
            return typeToComponentId[component.GetType()];
        }

        public void AddComponentType<T>(int componentTypeId, short defaultSize) where T : Component, new()
        {
            Debug.Assert(componentManagers[componentTypeId] == null);
            componentManagers[componentTypeId] = new ComponentManager<T>(componentTypeId, defaultSize);

            Type[] genericTypes = componentManagers[componentTypeId].GetType().GetGenericArguments();
            typeToComponentId[genericTypes[0]] = (byte)componentTypeId;
            componentManagers[componentTypeId].SetEntityManager(this);
        }

        public EntityManager(Universe universe)
        {
            this.universe = universe;
            //Scripts = new ScriptIdManager(this);
            liveIdRoster = new short[MaxEntities];
            inverseRoster = new short[MaxEntities];
            entityPool = new Entity[MaxEntities];
            componentLookupIdsPerEntity = new short[MaxEntities * EntityManager.MaxComponentTypes];
            for (short i = 0; i < (short)liveIdRoster.Length; i++)
            {
                liveIdRoster[i] = i; // To start off with
                inverseRoster[i] = i;
                entityPool[i] = new Entity();
            }

            partitionIndex = 0;

            componentManagers = new ComponentManagerBase[EntityManager.MaxComponentTypes];

            // REVIEW: Where should this go?
            /*
            componentManagers[ComponentTypeIds.ColorBlind] = new ComponentManager<ColorBlind>(ComponentTypeIds.ColorBlind, profile.GetDefaultPoolSize(ComponentTypeIds.ColorBlind, 10));
            componentManagers[ComponentTypeIds.Placement] = new ComponentManager<Placement>(ComponentTypeIds.Placement, profile.GetDefaultPoolSize(ComponentTypeIds.Placement, 10));
            componentManagers[ComponentTypeIds.Inventory] = new ComponentManager<Inventory>(ComponentTypeIds.Inventory, profile.GetDefaultPoolSize(ComponentTypeIds.Inventory, 10)); // we don't have these in Tangled.
            componentManagers[ComponentTypeIds.Aspect] = new ComponentManager<Aspect>(ComponentTypeIds.Aspect, profile.GetDefaultPoolSize(ComponentTypeIds.Aspect, 10));
            componentManagers[ComponentTypeIds.Selection] = new ComponentManager<Selection>(ComponentTypeIds.Selection, profile.GetDefaultPoolSize(ComponentTypeIds.Selection, 10));
            componentManagers[ComponentTypeIds.Button] = new ComponentManager<Button>(ComponentTypeIds.Button, profile.GetDefaultPoolSize(ComponentTypeIds.Button, 10));
            //componentManagers[ComponentTypeIds.PointLightSource] = new ComponentManager<PointLightSource>(ComponentTypeIds.PointLightSource, 100);
            componentManagers[ComponentTypeIds.TransformChildren] = new ComponentManager<TransformChildren>(ComponentTypeIds.TransformChildren, profile.GetDefaultPoolSize(ComponentTypeIds.TransformChildren, 10));
            //componentManagers[ComponentTypeIds.AttachmentPoint] = new ComponentManager<AttachmentPoint>(ComponentTypeIds.AttachmentPoint, 50);
            //componentManagers[ComponentTypeIds.Input] = new ComponentManager<Input>(ComponentTypeIds.Input, 10);
            //componentManagers[ComponentTypeIds.ViewPoint] = new ComponentManager<ViewPoint>(ComponentTypeIds.ViewPoint, 10);
            componentManagers[ComponentTypeIds.Constrained] = new ComponentManager<Constrained>(ComponentTypeIds.Constrained, profile.GetDefaultPoolSize(ComponentTypeIds.Constrained, 10));
            //componentManagers[ComponentTypeIds.Common] = new ComponentManager<Common>(ComponentTypeIds.Common, 1000);
            componentManagers[ComponentTypeIds.Animation] = new ComponentManager<Animation>(ComponentTypeIds.Animation, profile.GetDefaultPoolSize(ComponentTypeIds.Animation, 10));
            componentManagers[ComponentTypeIds.Scripts] = new ComponentManager<ScriptComponent>(ComponentTypeIds.Scripts, profile.GetDefaultPoolSize(ComponentTypeIds.Scripts, 10));
            //scriptsComponents = (IComponentMapper<ScriptComponent>)componentManagers[ComponentTypeIds.Scripts]; // Hold onto this
            componentManagers[ComponentTypeIds.LaserInteractive] = new ComponentManager<LaserInteractive>(ComponentTypeIds.LaserInteractive, profile.GetDefaultPoolSize(ComponentTypeIds.LaserInteractive, 10));
            componentManagers[ComponentTypeIds.SoundLoop] = new ComponentManager<SoundLoop>(ComponentTypeIds.SoundLoop, profile.GetDefaultPoolSize(ComponentTypeIds.SoundLoop, 10));
            componentManagers[ComponentTypeIds.Text] = new ComponentManager<TextComponent>(ComponentTypeIds.Text, profile.GetDefaultPoolSize(ComponentTypeIds.Text, 10));
            componentManagers[ComponentTypeIds.PropertyBag] = new ComponentManager<PropertyBag>(ComponentTypeIds.PropertyBag, profile.GetDefaultPoolSize(ComponentTypeIds.PropertyBag, 10));     // Because of leaves.
            componentManagers[ComponentTypeIds.Entanglement] = new ComponentManager<Entanglement>(ComponentTypeIds.Entanglement, profile.GetDefaultPoolSize(ComponentTypeIds.Entanglement, 10));
            componentManagers[ComponentTypeIds.TitleLocation] = new ComponentManager<TitleLocation>(ComponentTypeIds.TitleLocation, profile.GetDefaultPoolSize(ComponentTypeIds.TitleLocation, 10));
            componentManagers[ComponentTypeIds.MouseInput] = new ComponentManager<MouseInput>(ComponentTypeIds.MouseInput, profile.GetDefaultPoolSize(ComponentTypeIds.MouseInput, 10));
            componentManagers[ComponentTypeIds.FrameAnimation] = new ComponentManager<FrameAnimation>(ComponentTypeIds.FrameAnimation, profile.GetDefaultPoolSize(ComponentTypeIds.FrameAnimation, 10));
            componentManagers[ComponentTypeIds.ChildTransform] = new ComponentManager<ChildTransform>(ComponentTypeIds.ChildTransform, profile.GetDefaultPoolSize(ComponentTypeIds.ChildTransform, 10));
            */
            /*
            Placement p1 = new Placement();
            Placement p2 = new Placement();
            TypedReference td = __makeref(p1);
            IntPtr ptr1 = **(IntPtr**)(&td);
            td = __makeref(p2);
            IntPtr ptr2 = **(IntPtr**)(&td);
            */
            uniqueIdToEntity = new Dictionary<int, Entity>(MaxEntities);
        }
        
        public EnumEntityHandler RemovingEntityEvent;
        public EnumEntityHandler AddedEntityEvent;

        private short[] liveIdRoster;     // de-reference these gives liveIds.
        private short[] inverseRoster;    // liveid into here gives roster index
        private short partitionIndex;     // First free index
        private Entity[] entityPool;

        private short[] componentLookupIdsPerEntity;          // list of componentLookupIds for each entity, organized by liveid

        private ComponentManagerBase[] componentManagers;   // indexed by componentTypeId
        //private IComponentMapper<ScriptComponent> scriptsComponents;// Handy

        private Dictionary<int, Entity> uniqueIdToEntity;

        public void UpdateSystems(Microsoft.Xna.Framework.GameTime gameTime)
        {
            DelayFreeEntitiesWorker();

            systemManager.UpdateSystems(gameTime);
        }

        public void DrawSystems(Microsoft.Xna.Framework.GameTime gameTime)
        {
            systemManager.DrawSystems(gameTime);
        }

        // When creating completely new entities (e.g. in editor, or in monster loot drop)
        // Note: this must be re-entrant, for when we create from templates that have children.
        public Entity AllocateForStaticContent(EntityTemplate template, int ownerId)
        {
            Entity entity = AllocateForStaticContentHelper();
            entity.OwnerUniqueId = ownerId;
            ApplyFromTemplate(entity, template.Name.CRC32Hash());
            // The component bits should be set now.
            AddNewEntityToSystems(entity);
            return entity;
        }

        public Entity AllocateForGeneratedContent(string templateName, int ownerId)
        {
            return AllocateForGeneratedContent(EntityTemplateManager.GetTemplateByName(templateName), ownerId);
        }

        public Entity AllocateForGeneratedContent(EntityTemplate template, int ownerId)
        {
            Entity entity = AllocateForGeneratedContentHelper();
            entity.OwnerUniqueId = ownerId;
            ApplyFromTemplate(entity, template.Name.CRC32Hash());
            // The component bits should be set now.
            AddNewEntityToSystems(entity);

            return entity;
        }

        // For use by the editor. REVIEW: Very similar to EntityTemplateManager.ApplyFromTemplate
        public Entity CloneAndAdd(Entity cloneSource)
        {
            Entity e = AllocateForStaticContentHelper();
            e.OwnerUniqueId = cloneSource.OwnerUniqueId;
            e.TemplateId = cloneSource.TemplateId;
            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                Component sourceComponent = this.GetComponent(cloneSource, i);
                if (sourceComponent != null)
                {
                    bool allocatedNew;
                    Component component = GetOrAllocateComponentAndUpdateBitField(e, typeToComponentId[sourceComponent.GetType()], out allocatedNew);
                    Debug.Assert(allocatedNew);
                    component.ApplyFromTemplate(sourceComponent);
                }
            }

            // Handle adding any children.
            /*
            Component componentTransformChildrenSource = GetComponent(cloneSource, ComponentTypeIds.TransformChildren);
            if (componentTransformChildrenSource != null)
            {
                TransformChildren transformChildrenSource = (TransformChildren)componentTransformChildrenSource;
                TransformChildren transformChildrenClone = (TransformChildren)GetComponent(e, ComponentTypeIds.TransformChildren);
                foreach (int uniqueIdChild in transformChildrenSource.GetContainedItemUniqueIds())
                {
                    Entity newChild = CloneAndAdd(GetEntityByUniqueId(uniqueIdChild));
                    // But re-assign the owner: REVIEW: This is a little sketchy
                    newChild.OwnerUniqueId = e.UniqueId;
                    transformChildrenClone.AddItem(this, e.LiveId, newChild);
                }
            }*/

            // The component bits should be set now.
            AddNewEntityToSystems(e);

            return e;
        }

        public IComponentMapper<T> GetComponentManager<T>(int componentTypedId)
        {
            return (IComponentMapper<T>)componentManagers[componentTypedId];
        }

        private Entity AllocateForStaticContentHelper()
        {
            Entity e = Allocate(universe.GetNextUniqueIdForStaticContent());
            return e;
        }

        private Entity AllocateForGeneratedContentHelper()
        {
            Entity e = Allocate(universe.GetNextUniqueIdForGeneratedContent());
            return e;
        }

        // When deserializing:
        private Entity Allocate(int uniqueId)
        {
            Entity t = entityPool[liveIdRoster[partitionIndex]];
            t.LiveId = liveIdRoster[partitionIndex];
            inverseRoster[t.LiveId] = partitionIndex;
            //t.Init();
            partitionIndex++;

#if ALTERNATE_LAYOUT
            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                componentLookupIdsPerEntity[t.LiveId + i * MaxEntities] = -1;
            }
#else
            int iStart = t.LiveId * EntityManager.MaxComponentTypes;
            int iEnd = iStart + EntityManager.MaxComponentTypes;
            // Blank out the component lookup ids
            for (int i = iStart; i < iEnd; i++)
            {
                componentLookupIdsPerEntity[i] = -1;
            }
#endif

            Debug.Assert(!uniqueIdToEntity.ContainsKey(uniqueId));
            uniqueIdToEntity[uniqueId] = t;
            t.UniqueId = uniqueId;

            // REVIEW: This is prior to adding any components. That may be a problem
            if (AddedEntityEvent != null)
            {
                AddedEntityEvent(t);
            }

            return t;
        }

        public void EnumerateEntitiesOwnedBy(int ownerUniqueId, ICollection<int> liveIds, EntityEnumeration enumerationType)
        {
            //ComponentManager<Inventory> inventoryComponentManager = (ComponentManager<Inventory>)componentManagers[ComponentTypeIds.Inventory];
            //ComponentManager<TransformChildren> transformChildrenComponentManager = (ComponentManager<TransformChildren>)componentManagers[ComponentTypeIds.TransformChildren];

            Debug.Assert(liveIds.Count == 0);
            for (int i = 0; i < partitionIndex; i++)
            {
                int liveId = liveIdRoster[i];
                Entity e = entityPool[liveId];
                Debug.Assert(e.LiveId == liveId);
                if (e.OwnerUniqueId == ownerUniqueId)
                {
                    liveIds.Add(liveId);

                    if (enumerationType == EntityEnumeration.Deep)
                    {
                        throw new NotImplementedException();
                        /*
                        // Check to see if it has children. This is a little strange, because the entity
                        // manager now has knowledge of the Inventory component. We could move this out somewhere.
                        // For TransformChildren, on the other hand, it makes sense for EntityManager to know about.
                        int componentLookupId = GetComponentLookupIdForEntity(e, ComponentTypeIds.Inventory);
                        if (componentLookupId != -1)
                        {
                            Inventory inventory = inventoryComponentManager.GetComponentAt<Inventory>(componentLookupId);
                            foreach (int uniqueId in inventory.GetContainedItemUniqueIds())
                            {
                                liveIds.Add(GetEntityByUniqueId(uniqueId).LiveId);
                            }
                        }

                        componentLookupId = GetComponentLookupIdForEntity(e, ComponentTypeIds.TransformChildren);
                        if (componentLookupId != -1)
                        {
                            TransformChildren transformChildren = transformChildrenComponentManager.GetComponentAt<TransformChildren>(componentLookupId);
                            foreach (int uniqueId in transformChildren.GetContainedItemUniqueIds())
                            {
                                liveIds.Add(GetEntityByUniqueId(uniqueId).LiveId);
                            }
                        }*/
                    }
                }
            }
        }

        public void Free(List<int> liveIds, EntityFreeOptions freeOptions)
        {
            foreach (int liveId in liveIds)
            {
                // These could be invalid now (because removing one removed another). Handle that.
                Entity toRemove = entityPool[liveId];
                if (toRemove.LiveId != InvalidEntityLiveId)
                {
                    Free(entityPool[liveId], freeOptions);
                }
            }
        }

        private List<int> liveIdsFreeWorker = new List<int>(600);
        public void FreeAllEntities()
        {
            liveIdsFreeWorker.Clear();
            for (int i = 0; i < partitionIndex; i++)
            {
                int liveId = liveIdRoster[i];
                liveIdsFreeWorker.Add(liveId);
            }
            Free(liveIdsFreeWorker, EntityFreeOptions.Shallow); // Shallow, since we included all.
        }

        public void Free(int liveId, EntityFreeOptions freeOptions)
        {
            Free(entityPool[liveId], freeOptions);
        }

        public void Free(Entity t, EntityFreeOptions freeOptions)
        {
            /*
            if (freeOptions == EntityFreeOptions.Deep)
            {
                Component transformChildrenComponent = GetComponent(t, ComponentTypeIds.TransformChildren);
                if (transformChildrenComponent != null)
                {
                    FreeEntityChildren(t, (TransformChildren)transformChildrenComponent);
                }
            }*/

            // Call this before removing its components, since event handlers may need
            // to inspect its components (for instance, seeing what objects it owns)
            if (RemovingEntityEvent != null)
            {
                RemovingEntityEvent(t);
            }

            RemoveFreedEntityFromSystems(t);

            RemoveComponentsForEntity(t);

            int uniqueId = t.UniqueId;
            // Swap with the empty one at partitionIndex
            short liveId = t.LiveId;
            short rosterIndex = inverseRoster[liveId];

            short tempLiveId = liveIdRoster[rosterIndex];
            Debug.Assert(tempLiveId == liveId);
            short movedLiveId = liveIdRoster[partitionIndex - 1];
            liveIdRoster[rosterIndex] = movedLiveId;
            liveIdRoster[partitionIndex - 1] = tempLiveId;

            // Update the inverse roster.
            inverseRoster[liveId] = -1; // Now points to nowhere
            inverseRoster[movedLiveId] = rosterIndex;

            // Set in valid ids:
            t.LiveId = -1;
            t.OwnerUniqueId = -1;
            t.UniqueId = -1;
            t.ComponentTypesBitField = 0;

            partitionIndex--; // This essentially does the "de-alloc"

            uniqueIdToEntity.Remove(uniqueId);

            //t.Reset();
        }

        /*
        private void FreeEntityChildren(Entity entity, TransformChildren children)
        {
            foreach (int uniqueId in children.GetContainedItemUniqueIds())
            {
                // REVIEW: We're only allowing one level of Deep free.
                Free(GetEntityByUniqueId(uniqueId), EntityFreeOptions.Shallow);
            }
        }*/

        private void RemoveFreedEntityFromSystems(Entity entity)
        {
            foreach (EntitySystem system in systemManager.Systems)
            {
                bool wasInSystem = ((system.RequiredComponentTypesBitField & entity.ComponentTypesBitField) == system.RequiredComponentTypesBitField);
                wasInSystem = wasInSystem && ((system.RequireAtLeastOneComponentTypesBitField & entity.ComponentTypesBitField) != 0);
                if (wasInSystem)
                {
                    system.RemoveEntity(entity);
                }
            }
        }

        private void AddNewEntityToSystems(Entity entity)
        {
            foreach (EntitySystem system in systemManager.Systems)
            {
                bool shouldBeInSystem = ((system.RequiredComponentTypesBitField & entity.ComponentTypesBitField) == system.RequiredComponentTypesBitField);
                shouldBeInSystem = shouldBeInSystem && ((system.RequireAtLeastOneComponentTypesBitField & entity.ComponentTypesBitField) != 0);
                if (shouldBeInSystem)
                {
                    system.AddEntity(entity);
                }
            }
        }

        // After a change in components.
        private void UpdateEntityInSystems(Entity entity, int oldComponentTypesBitField)
        {
            Debug.Assert(oldComponentTypesBitField != entity.ComponentTypesBitField);
            foreach (EntitySystem system in systemManager.Systems)
            {
                int systemRequiredComponentTypesBitField = system.RequiredComponentTypesBitField;
                int systemRequiresOneComponentTypesBitField = system.RequireAtLeastOneComponentTypesBitField;
                bool before = ((systemRequiredComponentTypesBitField & oldComponentTypesBitField) == systemRequiredComponentTypesBitField);
                before = before && ((systemRequiresOneComponentTypesBitField & oldComponentTypesBitField) != 0);
                bool after = ((systemRequiredComponentTypesBitField & entity.ComponentTypesBitField) == systemRequiredComponentTypesBitField);
                after = after && ((systemRequiresOneComponentTypesBitField & entity.ComponentTypesBitField) != 0);
                if (!before && after)
                {
                    system.AddEntity(entity);
                }
                else if (before && !after)
                {
                    system.RemoveEntity(entity);
                }
            }
        }

        private void RemoveComponentsForEntity(Entity t)
        {
#if ALTERNATE_LAYOUT
            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                int componentLookupId = componentLookupIdsPerEntity[t.LiveId + i * MaxEntities];
                if (componentLookupId != -1)
                {
                    componentManagers[i].FreeAtComponentLookupId(componentLookupId);
                }
            }
#else
            int iStart = t.LiveId * EntityManager.MaxComponentTypes;
            int iEnd = iStart + EntityManager.MaxComponentTypes;
            int i2 = 0;
            for (int i = iStart; i < iEnd; i++, i2++)
            {
                int componentLookupId = componentLookupIdsPerEntity[i];
                if (componentLookupId != -1)
                {
                    componentManagers[i2].FreeAtComponentLookupId(componentLookupId);
                }
            }
#endif
        }

        public Component GetComponent(int entityLiveId, int componentTypeId)
        {
            ComponentManagerBase componentManager = componentManagers[componentTypeId];
#if ALTERNATE_LAYOUT
            int componentTypeLookupId = componentLookupIdsPerEntity[entityLiveId + componentTypeId * MaxEntities];
#else
            int componentTypeLookupId = componentLookupIdsPerEntity[liveIndex * EntityManager.MaxComponentTypes + componentTypeId];
#endif
            return (componentTypeLookupId != -1) ? componentManager.GetComponentAt(componentTypeLookupId) : null;
        }

        // Returns null if no such component.
        public Component GetComponent(Entity e, int componentTypeId)
        {
            int liveIndex = e.LiveId;
            Debug.Assert(e == entityPool[liveIndex], "Entity has wrong id");

            ComponentManagerBase componentManager = componentManagers[componentTypeId];
#if ALTERNATE_LAYOUT
            int componentTypeLookupId = componentLookupIdsPerEntity[liveIndex + componentTypeId * MaxEntities];
#else
            int componentTypeLookupId = componentLookupIdsPerEntity[liveIndex * EntityManager.MaxComponentTypes + componentTypeId];
#endif
            return (componentTypeLookupId != -1) ? componentManager.GetComponentAt(componentTypeLookupId) : null;
        }

        internal int GetComponentLookupIdForEntity(Entity e, int componentTypeId)
        {
            int liveIndex = e.LiveId;
#if ALTERNATE_LAYOUT
            int componentTypeLookupId = componentLookupIdsPerEntity[liveIndex + componentTypeId * MaxEntities];
#else
            int componentTypeLookupId = componentLookupIdsPerEntity[liveIndex * EntityManager.MaxComponentTypes + componentTypeId];
#endif
            return componentTypeLookupId;
        }

        internal int GetComponentLookupIdForEntity(int entityLiveId, int componentTypeId)
        {
#if ALTERNATE_LAYOUT
            int componentTypeLookupId = componentLookupIdsPerEntity[entityLiveId + componentTypeId * MaxEntities];
#else
            int componentTypeLookupId = componentLookupIdsPerEntity[entityLiveId * EntityManager.MaxComponentTypes + componentTypeId];
#endif
            return componentTypeLookupId;
        }

        // This shouldn't be public, since it doesn't do proper add/remove from systems.
        private Component GetOrAllocateComponentAndUpdateBitField(Entity e, int componentTypeId, out bool allocatedNew)
        {
            //int liveIndex = liveIdRoster[e.LiveId];
            int liveIndex = e.LiveId;
            Debug.Assert(e == entityPool[liveIndex], "Entity has wrong id");
            ComponentManagerBase componentManager = componentManagers[componentTypeId];
#if ALTERNATE_LAYOUT
            int index = liveIndex + componentTypeId * MaxEntities;
#else
            int index = liveIndex * EntityManager.MaxComponentTypes + componentTypeId;
#endif
            int componentTypeLookupId = componentLookupIdsPerEntity[index];
            if (componentTypeLookupId == -1)
            {
                // Allocate a new component for this entity
                short componentLookupId;
                Component component = componentManager.AllocateComponentForEntity(e, out componentLookupId);
                componentLookupIdsPerEntity[index] = componentLookupId;
                allocatedNew = true;

                // Make sure the entity is tagged as having this component
                e.ComponentTypesBitField |= ComponentTypeIdHelper.GetBit(componentTypeId);

                return component;
            }
            else
            {
                // Retrieve
                allocatedNew = false;
                return componentManager.GetComponentAt(componentTypeLookupId);
            }
        }

        public Entity GetEntityByUniqueId(int uniqueId)
        {
            return uniqueIdToEntity[uniqueId];
        }

        public Entity TryGetEntityByUniqueId(int uniqueId)
        {
            Entity returnValue = null;
            uniqueIdToEntity.TryGetValue(uniqueId, out returnValue);
            return returnValue;
        }

        public Entity GetEntityByLiveId(int liveId)
        {
            //Debug.Assert(liveId < partitionIndex);
            return entityPool[liveId];
        }

        public Component AddComponentToEntity(Entity e, int componentTypeId, out bool allocatedNew)
        {
            int oldComponentTypesBitField = e.ComponentTypesBitField;
            Component component = GetOrAllocateComponentAndUpdateBitField(e, componentTypeId, out allocatedNew);
            if (allocatedNew)
            {
                UpdateEntityInSystems(e, oldComponentTypesBitField);
            }
            return component;
        }

        public void RemoveComponentFromEntity(Entity e, int componentTypeId)
        {
            int oldComponentTypesBitField = e.ComponentTypesBitField;
            // (1) update componentlookupids, and return component to pool
#if ALTERNATE_LAYOUT
            int iIndex = e.LiveId + componentTypeId * MaxEntities;
#else
            int iIndex = e.LiveId * EntityManager.MaxComponentTypes + componentTypeId;
#endif
            int componentLookupId = componentLookupIdsPerEntity[iIndex];
            if (componentLookupId != -1)
            {
                componentManagers[componentTypeId].FreeAtComponentLookupId(componentLookupId);
                componentLookupIdsPerEntity[iIndex] = -1;
            }
            else
            {
                Debug.Assert(false, "removing non-existent component. Is this ok?");
            }
            // (2) update e.ComponentTypesBitField by removing the bit for the removed component type.
            e.ComponentTypesBitField &= ~ComponentTypeIdHelper.GetBit(componentTypeId);
            // (3) Ensure its removed from the appropriate systems
            UpdateEntityInSystems(e, oldComponentTypesBitField);
        }

        // TODO: This has code in common with SerializeEntity a bit.
        private bool IsEntityExactMatchForTemplate(Entity childEntity, EntityTemplate childEntityTemplate)
        {
            bool exactMatch = (childEntity.TemplateId == childEntityTemplate.EntityTemplateId);
            if (exactMatch)
            {
                for (int i = 0; exactMatch && (i < EntityManager.MaxComponentTypes); i++)
                {
                    Component componentCurrent = GetComponent(childEntity, i);
                    Component templateComponentCurrent = childEntityTemplate.ComponentArray[i];
                    if ((componentCurrent != null) && (templateComponentCurrent != null))
                    {
                        exactMatch = !componentCurrent.IsDifferent(childEntity, templateComponentCurrent);
                    }
                    else
                    {
                        exactMatch = (componentCurrent == null) && (templateComponentCurrent == null);
                    }
                }
            }
            return exactMatch;
        }

        // Persisted format:
        //  UniqueId: int32
        //  Template: int16
        //  //Size: int16 // -> We don't need this if we never need to lookup serialized items
        //  componentcount: byte
        //  (n components)
        private Component[] serializationComponentWorker1 = new Component[EntityManager.MaxComponentTypes];
        public void SerializeEntity(BinaryWriter writer, Entity entity, List<int> extraIdsToSerialize, EntityEnumeration enumerationType, List<int> liveIdsWeSkippedSerializing)
        {
            Debug.Assert(((enumerationType == EntityEnumeration.Deep) && (extraIdsToSerialize != null)) ||
                ((enumerationType == EntityEnumeration.Shallow) && (extraIdsToSerialize == null)));

            writer.Write(entity.UniqueId);
            writer.Write(entity.OwnerUniqueId);
            writer.Write(entity.TemplateId);

            EntityTemplate entityTemplate = EntityTemplateManager.GetTemplateById(entity.TemplateId);

            //bool addTransformChildrenToExtraIdsList = false;

            // Now we write the number of components. To know how many we're going to write out,
            // we need to figure out how many are different than the template ones.
            int saveCount = 0;
            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                Component componentCurrent = GetComponent(entity, i);
                serializationComponentWorker1[i] = null;
                if (componentCurrent != null)
                {
                    bool exactMatch = false;
                    //if ((i == ComponentTypeIds.TransformChildren) && (enumerationType == EntityEnumeration.Deep))
                    if (enumerationType == EntityEnumeration.Deep)
                    {
                        throw new NotImplementedException();

                        /*
                        // Big optimization: For TransformChildren, if the set of transform children for this entity matches
                        // *exactly* what's in the template's children, then we can skip adding this component altogether,
                        // saving a lot of space. We''ll assume the same order too.
                        TransformChildren transformChildren = componentCurrent as TransformChildren;
                        List<int> containedItemUniqueIds = transformChildren.GetContainedItemUniqueIds();
                        exactMatch = (containedItemUniqueIds.Count == entityTemplate.ChildTemplates.Count);
                        if (exactMatch)
                        {
                            for (int matchIndex = 0; exactMatch && (matchIndex < containedItemUniqueIds.Count); matchIndex++)
                            {
                                Entity childEntity = GetEntityByUniqueId(containedItemUniqueIds[matchIndex]);
                                EntityTemplate childEntityTemplate = entityTemplate.ChildTemplates[matchIndex];
                                exactMatch = IsEntityExactMatchForTemplate(childEntity, childEntityTemplate);
                            }
                        }

                        // We'll need to serialize the extra ids if this wasn't an exact match.
                        addTransformChildrenToExtraIdsList = !exactMatch;

                        // If we're not adding them to the "please serialize" list, we need to add them to the
                        // list of things we skipped serializing (in case they need to be removed)
                        if (!addTransformChildrenToExtraIdsList)
                        {
                            foreach (int uniqueId in containedItemUniqueIds)
                            {
                                liveIdsWeSkippedSerializing.Add(GetEntityByUniqueId(uniqueId).LiveId);
                            }
                        }*/
                    }
                    else
                    {
                        Component templateComponent = entityTemplate.ComponentArray[i];
                        exactMatch = (templateComponent != null) && !componentCurrent.IsDifferent(entity, templateComponent);
                    }

                    if (!exactMatch)
                    {
                        // Need to serialize it, because it doesn't match the template.
                        serializationComponentWorker1[i] = componentCurrent;
                        saveCount++;
                    }
                }
            }

            writer.Write((byte)saveCount);

            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                Component componentCurrent = serializationComponentWorker1[i];
                if (componentCurrent != null)
                {
                    writer.Write(typeToComponentId[componentCurrent.GetType()]);
                    componentCurrent.Serialize(writer);

                    // Inventory is straightforward, because these are never part of a template, so we don't need to optimize them out.
                    // So always add them to the extra ids to serialize.
                    if (enumerationType == EntityEnumeration.Deep)
                    {
                        throw new NotImplementedException();
                    }
                    /*
                    if ((i == ComponentTypeIds.Inventory) && (enumerationType == EntityEnumeration.Deep))
                    {
                        Inventory inventory = componentCurrent as Inventory;
                        foreach (int uniqueId in inventory.GetContainedItemUniqueIds())
                        {
                            extraIdsToSerialize.Add(GetEntityByUniqueId(uniqueId).LiveId);
                        }
                    }
                    else if ((i == ComponentTypeIds.TransformChildren) && addTransformChildrenToExtraIdsList)
                    {
                        TransformChildren transformChildren = componentCurrent as TransformChildren;
                        foreach (int uniqueId in transformChildren.GetContainedItemUniqueIds())
                        {
                            extraIdsToSerialize.Add(GetEntityByUniqueId(uniqueId).LiveId);
                        }
                    }*/
                }
            }
        }

        private bool[] deserializationComponentWorker1 = new bool[EntityManager.MaxComponentTypes];
        public void DeserializeEntity(BinaryReader reader)
        {
            int uniqueId = reader.ReadInt32();
            int ownerUniqueId = reader.ReadInt32();
            int templateId = reader.ReadInt32();

            EntityTemplate entityTemplate = EntityTemplateManager.GetTemplateById(templateId);

            Entity entity = Allocate(uniqueId);
            entity.TemplateId = templateId;
            entity.OwnerUniqueId = ownerUniqueId;

            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                deserializationComponentWorker1[i] = false;
            }

            int componentCount = reader.ReadByte();
            Debug.Assert((componentCount > 0) && (componentCount < componentManagers.Length), "Corrupt file");
            for (int i = 0; i < componentCount; i++)
            {
                int componentTypeId = reader.ReadByte();
                bool allocatedNew;
                Component component = GetOrAllocateComponentAndUpdateBitField(entity, componentTypeId, out allocatedNew);
                component.Deserialize(reader);
                deserializationComponentWorker1[componentTypeId] = true;
            }

            // Fill in the missing ones from the template. Mark off which ones we did and go do the missing ones.
            for (int i = 0; i < EntityManager.MaxComponentTypes; i++)
            {
                Component componentTemplate = entityTemplate.ComponentArray[i];
                if (!deserializationComponentWorker1[i] && (componentTemplate != null))
                {
                    //Debug.Assert(i != ComponentTypeIds.TransformChildren, "TransformChildren should not be directly in template"); // If change this, beware of if statement below
                    bool allocatedNew;
                    Component component = GetOrAllocateComponentAndUpdateBitField(entity, i, out allocatedNew);
                    component.ApplyFromTemplate(componentTemplate);
                }
            }

            // Handle adding any children if we didn't encounter a serialized set of them.
            /*
            if (!deserializationComponentWorker1[ComponentTypeIds.TransformChildren])
            {
                AddChildrenBasedOnTemplate(entity, entityTemplate);
            }*/

            // The component bits should be set now.
            AddNewEntityToSystems(entity);
        }

        /*
        private void AddChildrenBasedOnTemplate(Entity entity, EntityTemplate entityTemplate)
        {
            if ((entityTemplate.ChildTemplates != null) && (entityTemplate.ChildTemplates.Count > 0))
            {
                if ((entity.OwnerUniqueId != InvalidEntityUniqueId) && !Universe.IsTopLevelGroupId(entity.OwnerUniqueId))
                {
                    throw new InvalidOperationException("Haven't yet implementing cloning owned entities");
                }

                bool allocatedNew;
                TransformChildren transformChildren = (TransformChildren)this.GetOrAllocateComponentAndUpdateBitField(entity, ComponentTypeIds.TransformChildren, out allocatedNew);
                Debug.Assert(allocatedNew);
                foreach (EntityTemplate childTemplate in entityTemplate.ChildTemplates)
                {
                    // REVIEW: Re-entrancy, but it should be ok.
                    Entity childEntity = this.AllocateForStaticContent(childTemplate, entity.OwnerUniqueId);
                    transformChildren.AddItem(this, entity.LiveId, childEntity);
                }
            }
        }*/

        private void ApplyFromTemplate(Entity entity, int templateId)
        {
            EntityTemplate entityTemplate = EntityTemplateManager.GetTemplateById(templateId);
            foreach (Component componentTemplate in entityTemplate.ComponentTemplates)
            {
                bool allocatedNew;
                Component component = this.GetOrAllocateComponentAndUpdateBitField(entity, typeToComponentId[componentTemplate.GetType()], out allocatedNew);
                if (allocatedNew)
                {
                    // That means it didn't exist before. So use the template version.
                    component.ApplyFromTemplate(componentTemplate);
                }
            }

            // Handle adding any children.
            //AddChildrenBasedOnTemplate(entity, entityTemplate);

            entity.TemplateId = templateId;
        }

        // For the editor only
        public void ReassignEntityNameAndId(Entity entity, string newName)
        {
            Entity test = uniqueIdToEntity[entity.UniqueId];
            Debug.Assert(test == entity);
            uniqueIdToEntity.Remove(entity.UniqueId);
            entity.UniqueId = newName.CRC32Hash();
            uniqueIdToEntity[entity.UniqueId] = entity;
            // REVIEW: What other things need to change?
            // Anyone that holds onto this uniqueId in:
            //  - persisted Messages (we can probably ignore this, since this is for editor only)
            //  - if this is in anyone's Inventory
            //  - if this is in anyone's Childtransform (unlikely)
        }

        // Allocates garbage, only use in editor
        public IEnumerable<Entity> EnumerateComponents()
        {
            for (int i = 0; i < partitionIndex; i++)
            {
                yield return entityPool[i];
            }
        }

        public int EntityCount { get { return partitionIndex; } }

        public int SendMessage(uint message, ref MessageData data, object sender)
        {
            return SendMessage(InvalidEntityUniqueId, message, ref data, sender);
        }

        public int SendMessage(Entity entity, uint message, ref MessageData data, object sender)
        {
            return SendMessage(entity.UniqueId, message, ref data, sender);
        }

        public int SendMessage(int uniqueId, uint message, ref MessageData data, object sender)
        {
            int result = 0;
            Entity target;
            if (uniqueId == InvalidEntityUniqueId)
            {
                result = DeliverMessage(null, message, ref data, sender);
            }
            else
            {
                if (uniqueIdToEntity.TryGetValue(uniqueId, out target))
                {
                    result = DeliverMessage(target, message, ref data, sender);
                }
                // Else the entity doesn't exist. Not necessarily an error, but might be useful to log this?
            }
            return result;
        }

        // Convenience helpers
        public int SendMessage(Entity e, uint message, object sender)
        {
            MessageData data = new MessageData();
            return SendMessage(e, message, ref data, sender);
        }
        public int SendMessage(Entity e, uint message, int i, object sender)
        {
            MessageData data = new MessageData(i);
            return SendMessage(e, message, ref data, sender);
        }
        public int SendMessage(Entity e, uint message, bool value, object sender)
        {
            MessageData data = new MessageData(value);
            return SendMessage(e, message, ref data, sender);
        }
        public int SendMessage(Entity e, uint message, int i, bool value, object sender)
        {
            MessageData data = new MessageData(i, value);
            return SendMessage(e, message, ref data, sender);
        }
        public int SendMessage(Entity e, uint message, int i, float single, object sender)
        {
            MessageData data = new MessageData(i, single);
            return SendMessage(e, message, ref data, sender);
        }
        public int SendMessage(Entity e, uint message, int i, float single, float single2, object sender)
        {
            MessageData data = new MessageData(i, single, single2);
            return SendMessage(e, message, ref data, sender);
        }

        // Requires a live entity.
        private int DeliverMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            int result = systemManager.SendMessage(target, message, ref data, sender);
            if (!data.Handled)
            {
                if (target != null)
                {
                    /*
                    ScriptComponent scripts = scriptsComponents.GetComponentFor(target);
                    if (scripts != null)
                    {
                        foreach (int scriptId in scripts.GetScriptIds())
                        {
                            IEntityScript entityScript = Scripts.GetScriptById(scriptId);
                            if (entityScript == null)
                            {
                                throw new ArgumentException("Bug, or need to guard against bad data");
                            }
                            if ((entityScript.ScriptFlags & ScriptFlags.HandlesMessages) == ScriptFlags.HandlesMessages)
                            {
                                result = entityScript.SendMessage(target, message, ref data, sender);
                            }
                            if (data.Handled)
                            {
                                break;
                            }
                        }
                    }*/
                }
            }
            return result;
        }

        // Helper
        /*
        public static void SetProperty(EntityManager em, Entity e, int id, int value)
        {
            bool allocatedNew;
            PropertyBag pb = (PropertyBag)em.AddComponentToEntity(e, ComponentTypeIds.PropertyBag, out allocatedNew);
            pb.SetValue(id, value);
        }
        public static int GetIntProperty(EntityManager em, Entity e, int id, int defaultValue = 0)
        {
            bool allocatedNew;
            PropertyBag pb = (PropertyBag)em.AddComponentToEntity(e, ComponentTypeIds.PropertyBag, out allocatedNew);
            return pb.GetIntValue(id, defaultValue);
        }*/

        public int GetTopLevelOwner(Entity e)
        {
            int owner = e.OwnerUniqueId;
            if (!Universe.IsTopLevelGroupId(owner))
            {
                return GetTopLevelOwner(GetEntityByUniqueId(owner));
            }
            return owner;
        }

        // Support for delay unloading.
        private List<int> uniqueIdsToUnload = new List<int>(200);
        public void DelayFreeByLiveId(int liveId, EntityFreeOptions freeOptions)
        {
            uniqueIdsToUnload.Add(GetEntityByLiveId(liveId).UniqueId);
        }
        public void DelayFreeByLiveId(List<int> liveIds, EntityFreeOptions freeOptions)
        {
            foreach (int liveId in liveIds)
            {
                uniqueIdsToUnload.Add(GetEntityByLiveId(liveId).UniqueId);
            }
        }

        private List<int> removeIdsWorker = new List<int>(200);
        private List<int> ownerIdsToUnload = new List<int>();
        public void DelayFreeEntitiesOwnedBy(int ownerId)
        {
            ownerIdsToUnload.Add(ownerId);
        }
        private void DelayFreeEntitiesWorker()
        {
            foreach (int ownerIdToUnload in ownerIdsToUnload)
            {
                removeIdsWorker.Clear();
                EnumerateEntitiesOwnedBy(ownerIdToUnload, removeIdsWorker, EntityEnumeration.Deep);
                Free(removeIdsWorker, EntityFreeOptions.Shallow);
            }
            ownerIdsToUnload.Clear();

            // And also individual ones that were specified
            foreach (int uniqueId in uniqueIdsToUnload)
            {
                Entity entity = TryGetEntityByUniqueId(uniqueId);
                if (entity != null)
                {
                    Free(entity.LiveId, EntityFreeOptions.Deep);
                }
            }
            uniqueIdsToUnload.Clear();
        }

    }

    public enum EntityFreeOptions
    {
        Shallow,
        Deep,
    }
}
