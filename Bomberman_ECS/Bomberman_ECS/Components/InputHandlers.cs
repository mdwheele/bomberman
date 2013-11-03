using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    enum InputType
    {
        Keyboard,
        Mouse,
        GamePad,
    }

    // Specifies the handlers that map actions and states to callback functions.
    // Callback functions are game-defined ids that represent functions.
    class InputHandlers : Component
    {
        public InputHandlers()
        {
            InputHandlerIds = new List<int>();
        }

        public InputHandlers(params string[] names) : this()
        {
            foreach (string name in names)
            {
                InputHandlerIds.Add(name.CRC32Hash());
            }
        }

        private const byte CurrentVersion = 1;

        public override void Init() { InputHandlerIds.Clear(); }

        public List<int> InputHandlerIds { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                InputHandlerIds.Add(reader.ReadInt32());
            }
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(InputHandlerIds.Count);
            foreach (int id in InputHandlerIds)
            {
                writer.Write(id);
            }
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            return true;
        }

        public override void ApplyFromTemplate(Component template)
        {
            InputHandlers inputTemplate = template as InputHandlers;
            InputHandlerIds.Clear();
            InputHandlerIds.AddRange(inputTemplate.InputHandlerIds);
        }
    }
}
