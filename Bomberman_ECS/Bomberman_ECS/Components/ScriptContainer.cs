using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    class ScriptContainer : Component
    {
        // Scripts are almost like Components themselves, except they are things that
        // tend to change more, and be more specific to a particular object.
        // In addition, Scripts have logic, unlike Components.
        public ScriptContainer()
        {
            ScriptIds = new List<int>(DefaultScriptNum);
            propertyBag = new PropertyBag();
        }

        public ScriptContainer(params int[] ids)
        {
            ScriptIds = new List<int>(DefaultScriptNum);
            ScriptIds.AddRange(ids);
            propertyBag = new PropertyBag();
        }

        private const int DefaultScriptNum = 6;
        private List<int> ScriptIds;
        private PropertyBag propertyBag;

        public PropertyBag PropertyBag { get { return propertyBag; } }

        public override void Init()
        {
            ScriptIds.Clear();
            PropertyBag.Clear();
        }

        // Use List to avoid allocations during foreach
        public List<int> GetScriptIds()
        {
            return ScriptIds;
        }

        public void AddScriptId(int id)
        {
            Debug.Assert(!ScriptIds.Contains(id));
            ScriptIds.Add(id);
        }

        public void RemoveItem()
        {

        }

        public override void Deserialize(BinaryReader reader)
        {
            int count = reader.ReadInt16();
            if (ScriptIds.Capacity < count)
            {
                ScriptIds.Capacity = count;
            }
            ScriptIds.Clear();
            for (int i = 0; i < count; i++)
            {
                ScriptIds.Add(reader.ReadInt32());
            }

            PropertyBag.Deserialize(reader);
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((short)ScriptIds.Count);
            foreach (int scriptId in ScriptIds)
            {
                writer.Write(scriptId);
            }

            PropertyBag.Serialize(writer);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            ScriptContainer scripts = template as ScriptContainer;
            bool different = scripts.GetScriptIds().Count != ScriptIds.Count;
            if (!different)
            {
                for (int i = 0; !different && (i < ScriptIds.Count); i++)
                {
                    different = (ScriptIds[i] != scripts.GetScriptIds()[i]);
                }
            }
            return different;
        }

        public override void ApplyFromTemplate(Component template)
        {
            ScriptContainer scripts = template as ScriptContainer;
            ScriptIds.Clear();
            ScriptIds.AddRange(scripts.GetScriptIds());
            this.PropertyBag.Clear();
            this.PropertyBag.CopyFrom(scripts.PropertyBag);
        }
    }
}
