using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Bomberman_ECS.Core
{
    // Generic property bag that holds ints, floats and bools.
    class PropertyBag
    {
        private const byte CurrentVersion = 1;

        public PropertyBag()
        {
            IntVariables = new List<IntVariable>(10);
            IntLists = new List<IntList>(2);
        }

        struct IntVariable
        {
            public IntVariable(int id, bool value)
            {
                Debug.Assert(((uint)id & 0x80000000) != 0);
                Id = id & 0x7fffffff; // Indicate it's boolean by stripping off top bit.
                Value = value ? 1 : 0;
            }

            public IntVariable(int id, int value)
            {
                Debug.Assert(((uint)id & 0x80000000) != 0);
                Id = id;
                Value = value;
            }

            // We use the high bit to indicate if this is bool or not.
            public int Id;
            public int Value;
        }

        struct IntList
        {
            public int Id;
            public List<int> Values;
        }

        private List<IntVariable> IntVariables;
        private List<IntList> IntLists;

        public void CopyFrom(PropertyBag other)
        {
            Debug.Assert(IntVariables.Count == 0);
            Debug.Assert(IntLists.Count == 0);

            IntVariables.AddRange(other.IntVariables);
            IntLists.AddRange(other.IntLists);
        }

        public void Clear()
        {
            IntVariables.Clear();
            IntLists.Clear();
        }

        public bool IsEmpty
        {
            get
            {
                return IntVariables.Count == 0 && IntLists.Count == 0;
            }
        }

        public void RemoveValue(int variableNameId)
        {
            bool found = false;
            for (int i = 0; i < IntVariables.Count; i++)
            {
                if (((uint)IntVariables[i].Id | 0x80000000) == (uint)variableNameId)
                {
                    IntVariables.RemoveAt(i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                for (int i = 0; i < IntLists.Count; i++)
                {
                    if (IntLists[i].Id == variableNameId)
                    {
                        IntLists.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
            }
        }

        public bool DoesValueExist(int variableNameId)
        {
            throw new NotImplementedException();
            /*
            for (int i = 0; i < IntVariables.Count; i++)
            {
                if (((uint)IntVariables[i].Id | 0x80000000) == (uint)variableNameId)
                {
                    return true;
                }
            }
            return false;*/
        }

        // The default value for boolean values which are not present is false
        // Yes, this is a bit of a restriction, but otherwise code needs to check 
        // for 3 possible values (true/false/na)
        // For int values it's zero.
        public bool GetBooleanValue(int variableNameId)
        {
            foreach (IntVariable variable in IntVariables)
            {
                if (((uint)variable.Id | 0x80000000) == (uint)variableNameId)
                {
                    Debug.Assert(((uint)variable.Id & 0x80000000) == 0, "int variable stored here");
                    Debug.Assert((variable.Value == 0) || (variable.Value == 1), "Bad value in boolean variable");
                    return variable.Value != 0;
                }
            }
            return false;
        }

        public int GetIntValue(int variableNameId, int defaultValue = 0)
        {
            foreach (IntVariable variable in IntVariables)
            {
                if (((uint)variable.Id | 0x80000000) == (uint)variableNameId)
                {
                    Debug.Assert(((uint)variable.Id & 0x80000000) != 0, "bool variable stored here");
                    return variable.Value;
                }
            }
            return defaultValue;
        }

        public float GetSingleValue(int variableNameId, float defaultValue = 0f)
        {
            foreach (IntVariable variable in IntVariables)
            {
                if (((uint)variable.Id | 0x80000000) == (uint)variableNameId)
                {
                    Debug.Assert(((uint)variable.Id & 0x80000000) != 0, "bool variable stored here");
                    Int32SingleUnion temp = new Int32SingleUnion(variable.Value);
                    return temp.AsSingle;
                }
            }
            return defaultValue;
        }

        public void GetList(int variableNameId, List<int> outList)
        {
            Debug.Assert(outList.Count == 0);
            for (int i = 0; i < IntLists.Count; i++)
            {
                if (IntLists[i].Id == variableNameId)
                {
                    outList.AddRange(IntLists[i].Values);
                    break;
                }
            }
        }

        public void SetValue(int variableNameId, bool value)
        {
            int intValue = value ? 1 : 0;
            bool found = false;
            for (int i = 0; !found && (i < IntVariables.Count); i++)
            {
                IntVariable bv = IntVariables[i];
                found = (((uint)bv.Id | 0x80000000) == (uint)variableNameId);
                if (found)
                {
                    Debug.Assert(((uint)bv.Id & 0x80000000) == 0, "int variable stored here");
                    bv.Value = intValue;
                    IntVariables[i] = bv;
                }
            }
            if (!found)
            {
                IntVariables.Add(new IntVariable(variableNameId, value));
            }
        }

        public void SetValue(int variableNameId, int value)
        {
            bool found = false;
            for (int i = 0; !found && (i < IntVariables.Count); i++)
            {
                IntVariable bv = IntVariables[i];
                found = (((uint)bv.Id | 0x80000000) == (uint)variableNameId);
                if (found)
                {
                    Debug.Assert(((uint)bv.Id & 0x80000000) != 0, "bool variable stored here");
                    bv.Value = value;
                    IntVariables[i] = bv;
                }
            }
            if (!found)
            {
                IntVariables.Add(new IntVariable(variableNameId, value));
            }
        }

        public void SetValue(int variableNameId, float value)
        {
            Int32SingleUnion temp = new Int32SingleUnion(value);
            int intValue = temp.AsInt32;

            bool found = false;
            for (int i = 0; !found && (i < IntVariables.Count); i++)
            {
                IntVariable bv = IntVariables[i];
                found = (((uint)bv.Id | 0x80000000) == (uint)variableNameId);
                if (found)
                {
                    Debug.Assert(((uint)bv.Id & 0x80000000) != 0, "bool variable stored here");
                    bv.Value = intValue;
                    IntVariables[i] = bv;
                }
            }
            if (!found)
            {
                IntVariables.Add(new IntVariable(variableNameId, intValue));
            }
        }

        public void SetList(int variableNameId, IEnumerable<int> inList)
        {
            bool found = false;
            for (int i = 0; !found && (i < IntLists.Count); i++)
            {
                IntList il = IntLists[i];
                found = (il.Id == variableNameId);
                if (found)
                {
                    il.Values.Clear();
                    il.Values.AddRange(inList);
                }
            }
            if (!found)
            {
                List<int> values = new List<int>(inList);
                IntLists.Add(new IntList() { Id = variableNameId, Values = values });
            }
        }

        // Do we ever need to remove them?
        public void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            int countBool = reader.ReadInt16();
            int countInt = reader.ReadInt16();
            int countTotal = countBool + countInt;
            if (IntVariables.Capacity < countTotal)
            {
                IntVariables.Capacity = countTotal;
            }
            IntVariables.Clear();
            for (int i = 0; i < countBool; i++)
            {
                int id = reader.ReadInt32();
                bool value = reader.ReadBoolean();
                IntVariables.Add(new IntVariable(id, value));
            }
            for (int i = 0; i < countInt; i++)
            {
                int id = reader.ReadInt32();
                int value = reader.ReadInt32();
                IntVariables.Add(new IntVariable(id, value));
            }

            throw new NotImplementedException("Need to implement lists");
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            short countBool = 0;
            short countInt = 0;

            foreach (IntVariable variable in IntVariables)
            {
                if (((uint)variable.Id & 0x80000000) != 0)
                {
                    countInt++;
                }
                else
                {
                    countBool++;
                }
            }

            writer.Write(countBool);
            writer.Write(countInt);

            foreach (IntVariable variable in IntVariables)
            {
                if (((uint)variable.Id & 0x80000000) == 0)
                {
                    Debug.Assert((variable.Value == 0) || (variable.Value == 1), "Bad value in boolean variable");
                    // We write a uint and read an int. Should be ok.
                    writer.Write((uint)variable.Id | 0x80000000);
                    writer.Write(variable.Value != 0); // bool
                }
            }

            foreach (IntVariable variable in IntVariables)
            {
                if (((uint)variable.Id & 0x80000000) != 0)
                {
                    writer.Write((uint)variable.Id | 0x80000000);
                    writer.Write(variable.Value); // int
                }
            }

            throw new NotImplementedException("Need to implement lists");
        }
    }
}
