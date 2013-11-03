using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    interface ComponentManagerBase
    {
        void FreeAtComponentLookupId(int componentLookupId);
        Component GetComponentAt(int componentLookupId);
        Component AllocateComponentForEntity(Entity e, out short componentLookupId);
        void SetEntityManager(EntityManager em);
    }

    public interface IComponentMapper<T>
    {
        T GetComponentFor(Entity e);
        T GetComponentFor(int entityLiveId);
        IEnumerable<T> EnumerateComponents();
    }

    // Component manager for a specific Component. So basically it contains all the active Components
    // of a particular type.
    class ComponentManager<T> : ComponentManagerBase, IComponentMapper<T> where T : Component, new()
    {
        public ComponentManager(int componentTypeId, short defaultPoolSize)
        {
            if (defaultPoolSize > 0)
            {
                GrowPool(defaultPoolSize);
            }
            partitionIndex = 0;
            this.componentTypeId = componentTypeId;
        }

        private EntityManager em;
        public void SetEntityManager(EntityManager em) { this.em = em; }

        private void GrowPool(short newSize)
        {
            Debug.Assert(newSize > poolSize);

            short oldSize = poolSize;
            // Grow to newSize, or double the current size at the very minimum.
            newSize = (short)Math.Max(poolSize * 2, newSize);

            short[] rosterTEMP = new short[newSize];
            short[] inverseRosterTEMP = new short[newSize];
            T[] componentPoolTEMP = new T[newSize];
            // Copy the old ones over
            for (int i = 0; i < oldSize; i++)
            {
                rosterTEMP[i] = roster[i];
                inverseRosterTEMP[i] = inverseRoster[i];
                componentPoolTEMP[i] = componentPool[i];
            }
            // And make new ones
            for (short i = oldSize; i < newSize; i++)
            {
                rosterTEMP[i] = i; // To start off with
                inverseRosterTEMP[i] = i;
                componentPoolTEMP[i] = new T();
            }
            roster = rosterTEMP;
            inverseRoster = inverseRosterTEMP;
            componentPool = componentPoolTEMP;
            poolSize = newSize;
        }

        private int componentTypeId;
        private short[] roster;           // Indexing this results in ComponentLookupId
        private short[] inverseRoster;
        private short partitionIndex;     // First free index of roster
        private T[] componentPool;      // Index this with ComponentLookupId
        private short poolSize;

        public Component AllocateComponentForEntity(Entity e, out short componentLookupId)
        {
            // See if we need to resize our pool.
            if (partitionIndex >= poolSize)
            {
                GrowPool((short)(poolSize + 1)); // Actually, it will double.
            }

            componentLookupId = roster[partitionIndex];
            Component t = componentPool[componentLookupId];
            t.Init();
            partitionIndex++;
            return t;
        }

        public void FreeAtComponentLookupId(int componentLookupId)
        {
            T t = componentPool[componentLookupId];

            short rosterIndex = inverseRoster[componentLookupId];
            Debug.Assert(roster[rosterIndex] == componentLookupId, "ComponentManager is corrupt");

            // Swap with the empty one at partitionIndex - 1
            short temp = roster[rosterIndex];
            short movedComponentLookupId = roster[partitionIndex - 1];
            roster[rosterIndex] = movedComponentLookupId;
            roster[partitionIndex - 1] = temp;

            // Swap the inverse roster.
            inverseRoster[componentLookupId] = inverseRoster[movedComponentLookupId];
            inverseRoster[movedComponentLookupId] = rosterIndex;

            partitionIndex--;

            t.Init();
        }

        public Component GetComponentAt(int componentLookupId)
        {
            return componentPool[componentLookupId];
        }

        public T GetComponentAt<TT>(int componentLookupId) where TT : T
        {
            return componentPool[componentLookupId];
        }

        public T GetComponentFor(Entity e)
        {
            int componentLookupId = em.GetComponentLookupIdForEntity(e, componentTypeId);
            if (componentLookupId != -1)
            {
                return componentPool[componentLookupId];
            }
            return null;
        }

        public T GetComponentFor(int entityLiveId)
        {
            int componentLookupId = em.GetComponentLookupIdForEntity(entityLiveId, componentTypeId);
            if (componentLookupId != -1)
            {
                return componentPool[componentLookupId];
            }
            return null;
        }

        // Allocates garbage, only use in editor
        public IEnumerable<T> EnumerateComponents()
        {
            for (int i = 0; i < partitionIndex; i++)
            {
                yield return componentPool[roster[i]];
            }
        }
    }
}
