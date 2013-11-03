using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    [Flags]
    public enum SystemFlags
    {
        None = 0x00000000,
        HandleAllMessages = 0x00000001,
    }

    // Base class for systems. Keeps the system updated with the indices of all the entities that apply to it.
    public abstract class EntitySystem
    {
        public static bool EnableTiming = false;

        public EntityManager EntityManager { get; private set; }
        public SystemFlags Flags { get; private set; }

        internal void CallInitialize(EntityManager em)
        {
            this.EntityManager = em;
            Initialize();
        }
        protected abstract void Initialize(); // After this point, EntityManager has been set.

        public int UpdateOrder { get; private set; }
        public int DrawOrder { get; protected set; }

        public EntitySystem(int updateOrder, int[] requiredComponentTypeIds, uint[] supportedMessages = null) : this(updateOrder, requiredComponentTypeIds, null, supportedMessages)
        {
        }

        public EntitySystem(int updateOrder, int[] requiredComponentTypeIds, int[] optionalButOneRequiredComponentTypeIds, uint[] supportedMessages = null, SystemFlags flags = SystemFlags.None)
        {
            Flags = flags;
            UpdateOrder = updateOrder;
            enabled = true;
            foreach (int componentTypeId in requiredComponentTypeIds)
            {
                RequiredComponentTypesBitField |= ComponentTypeIdHelper.GetBit(componentTypeId);
            }
            if (optionalButOneRequiredComponentTypeIds == null)
            {
                RequireAtLeastOneComponentTypesBitField = ComponentTypeIdHelper.AllMask;
            }
            else
            {
                foreach (int componentTypeId in optionalButOneRequiredComponentTypeIds)
                {
                    RequireAtLeastOneComponentTypesBitField |= ComponentTypeIdHelper.GetBit(componentTypeId);
                }
            }

            liveIdToSlot = new int[EntityManager.MaxEntities];
            slotToLiveId = new int[EntityManager.MaxEntities];
            for (int i = 0; i < liveIdToSlot.Length; i++)
            {
                liveIdToSlot[i] = -1;
                slotToLiveId[i] = -1;
            }
            this.SupportedMessages = supportedMessages;

            entityIdCollection = new EntityIdCollection(this);
        }

        internal uint[] SupportedMessages;

        // We are interested only in entities with these components:
        public int RequiredComponentTypesBitField { get; private set; }
        public int RequireAtLeastOneComponentTypesBitField { get; private set; }

        // Using this double index, we can quickly loop through all current entities by looping through
        // slotToLiveId from 0 to partitionIndex exclusive. One important thing is that order must not matter.
        private int[] liveIdToSlot;   // "Fixed". If we store info about a thing, we use liveId
        private int[] slotToLiveId;   // These move around and are good for iterating through
        private int partitionIndex;     // First free slot

        private bool enabled;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        protected virtual void OnEnabledChanged() { }

        // For now, we can use the LiveId to manage things.
        internal void AddEntity(Entity entity)
        {
            Debug.Assert(liveIdToSlot[entity.LiveId] == -1);
            Debug.Assert(slotToLiveId[partitionIndex] == -1);
            liveIdToSlot[entity.LiveId] = partitionIndex;
            slotToLiveId[partitionIndex] = entity.LiveId;

            OnEntityAdded(entity, entity.LiveId);

            partitionIndex++;
        }

        internal void RemoveEntity(Entity entity)
        {
            int slotIndex = liveIdToSlot[entity.LiveId];
            OnEntityRemoved(entity, entity.LiveId);

            // Swap
            slotToLiveId[slotIndex] = slotToLiveId[partitionIndex - 1];
            // Save off the liveId of the slot that got moved
            int movedLiveId = slotToLiveId[partitionIndex - 1];
            slotToLiveId[partitionIndex - 1] = -1;  // Set it to invalid
            // And update liveIdToSlot index for the guy that got moved.
            liveIdToSlot[movedLiveId] = slotIndex;
            liveIdToSlot[entity.LiveId] = -1;   // Set this to invalid too

            partitionIndex--;
        }

        protected virtual void OnEntityAdded(Entity entity, int liveId) { }
        protected virtual void OnEntityRemoved(Entity entity, int liveId) { }

        private EntityIdCollection entityIdCollection;

        class EntityIdCollection : IEnumerable<int>
        {
            public EntityIdCollection(EntitySystem es)
            {
                enumerator = new EntityIdEnumerator(es);

            }
            private EntityIdEnumerator enumerator;

            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                enumerator.PublicReset();
                return enumerator;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                enumerator.PublicReset();
                return enumerator;
            }
        }

        class EntityIdEnumerator : IEnumerator<int>
        {
            public EntityIdEnumerator(EntitySystem es)
            {
                this.es = es;
            }
            private EntitySystem es;
            private int index = -1;

            public void PublicReset() { index = -1; }

            int IEnumerator<int>.Current
            {
                get { return es.slotToLiveId[index]; }
            }

            void IDisposable.Dispose() {}

            object System.Collections.IEnumerator.Current
            {
                get { return es.slotToLiveId[index]; }
            }

            bool System.Collections.IEnumerator.MoveNext()
            {
                index++;
                return index < es.partitionIndex;
            }

            void System.Collections.IEnumerator.Reset()
            {
                index = 0;
            }
        }

        public void ProcessEntities(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (Enabled)
            {
                OnProcessEntities(gameTime, entityIdCollection);
            }
        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (Enabled)
            {
                OnDraw(gameTime, entityIdCollection);
            }
        }

        protected IEnumerable<int> GetEntityLiveIds()
        {
            return entityIdCollection;
        }

        protected virtual void OnProcessEntities(Microsoft.Xna.Framework.GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
        }


        protected virtual void OnDraw(Microsoft.Xna.Framework.GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
        }

        internal int SendMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            int result = 0;
            if (Enabled)
            {
                //if (liveIdToSlot[target.LiveId] != -1)
                {
                    result = OnHandleMessage(target, message, ref data, sender);
                }
            }
            return result;
        }

        // The entity is guaranteed to be part of this system.
        protected virtual int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            return 0;
        }
    }
}
