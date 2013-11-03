using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    // Maps raw input to actions and states.
    // TODO: Support for mouse
    class InputMap : Component
    {
        private const byte CurrentVersion = 1;

        public InputMap()
        {
            KeyToAction = new Dictionary<Keys, int>();
            KeyToState = new Dictionary<Keys, int>();
            ButtonToAction = new Dictionary<Buttons, int>();
            ButtonToState = new Dictionary<Buttons, int>();
        }

        public InputMap(IEnumerable<KeyValuePair<Keys, int>> actions, IEnumerable<KeyValuePair<Keys, int>> states = null)
            : this()
        {
            foreach (KeyValuePair<Keys, int> pair in actions)
            {
                KeyToAction[pair.Key] = pair.Value;
            }
            if (states != null)
            {
                foreach (KeyValuePair<Keys, int> pair in states)
                {
                    KeyToState[pair.Key] = pair.Value;
                }
            }
        }

        public InputMap(IEnumerable<KeyValuePair<Buttons, int>> actions, IEnumerable<KeyValuePair<Buttons, int>> states = null)
            : this()
        {
            foreach (KeyValuePair<Buttons, int> pair in actions)
            {
                ButtonToAction[pair.Key] = pair.Value;
            }
            if (states != null)
            {
                foreach (KeyValuePair<Buttons, int> pair in states)
                {
                    ButtonToState[pair.Key] = pair.Value;
                }
            }
        }

        public override void Init() { KeyToAction.Clear(); KeyToState.Clear(); ButtonToAction.Clear(); ButtonToState.Clear(); }

        public Dictionary<Keys, int> KeyToAction { get; private set; }
        public Dictionary<Keys, int> KeyToState { get; private set; }
        public Dictionary<Buttons, int> ButtonToAction { get; private set; }
        public Dictionary<Buttons, int> ButtonToState { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                // Use Add so we can leverage order of param evaluation rules for reading data in the right order.
                KeyToAction.Add((Keys)reader.ReadInt32(), reader.ReadInt32());
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                KeyToState.Add((Keys)reader.ReadInt32(), reader.ReadInt32());
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ButtonToAction.Add((Buttons)reader.ReadInt32(), reader.ReadInt32());
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ButtonToState.Add((Buttons)reader.ReadInt32(), reader.ReadInt32());
            }
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(KeyToAction.Count);
            foreach (KeyValuePair<Keys, int> pair in KeyToAction)
            {
                writer.Write((int)pair.Key);
                writer.Write(pair.Value);
            }

            writer.Write(KeyToState.Count);
            foreach (KeyValuePair<Keys, int> pair in KeyToState)
            {
                writer.Write((int)pair.Key);
                writer.Write(pair.Value);
            }

            writer.Write(ButtonToAction.Count);
            foreach (KeyValuePair<Buttons, int> pair in ButtonToAction)
            {
                writer.Write((int)pair.Key);
                writer.Write(pair.Value);
            }

            writer.Write(ButtonToState.Count);
            foreach (KeyValuePair<Buttons, int> pair in ButtonToState)
            {
                writer.Write((int)pair.Key);
                writer.Write(pair.Value);
            }
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            return true;
        }

        public override void ApplyFromTemplate(Component template)
        {
            InputMap inputTemplate = template as InputMap;
            foreach (KeyValuePair<Keys, int> pair in inputTemplate.KeyToAction)
            {
                KeyToAction[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<Keys, int> pair in inputTemplate.KeyToState)
            {
                KeyToState[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<Buttons, int> pair in inputTemplate.ButtonToAction)
            {
                ButtonToAction[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<Buttons, int> pair in inputTemplate.ButtonToState)
            {
                ButtonToState[pair.Key] = pair.Value;
            }
        }
    }
}
