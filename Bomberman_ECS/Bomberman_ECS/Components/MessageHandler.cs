using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    struct MessageAndHandler
    {
        public MessageAndHandler(uint message, int handler)
        {
            Message = message;
            Handler = handler;
        }
        public uint Message;
        public int Handler;

        public static bool operator!= (MessageAndHandler m1, MessageAndHandler m2)
        {
            return m1.Handler != m2.Handler || m1.Message != m2.Message;
        }
        public static bool operator==(MessageAndHandler m1, MessageAndHandler m2)
        {
            return m1.Handler == m2.Handler && m1.Message == m2.Message;
        }
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class MessageHandler : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { messageAndFunctions.Clear(); }

        private List<MessageAndHandler> messageAndFunctions = new List<MessageAndHandler>();

        public MessageHandler() { }
        public MessageHandler(params MessageAndHandler[] mfs) : this()
        {
            messageAndFunctions.AddRange(mfs);
        }

        public void Add(MessageAndHandler mf) { messageAndFunctions.Add(mf); }
        public bool TryGetHandler(uint message, out int function)
        {
            function = -1;
            for (int i = 0; i < messageAndFunctions.Count; i++)
            {
                if (messageAndFunctions[i].Message == message)
                {
                    function = messageAndFunctions[i].Handler;
                    return true;
                }
            }
            return false;
        }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                messageAndFunctions.Add(new MessageAndHandler(reader.ReadUInt32(), reader.ReadInt32()));
            }

        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(messageAndFunctions.Count);
            foreach (MessageAndHandler mah in messageAndFunctions)
            {
                writer.Write(mah.Message);
                writer.Write(mah.Handler);
            }
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            MessageHandler mhTemplate = template as MessageHandler;
            bool different = true;
            if (mhTemplate.messageAndFunctions.Count == messageAndFunctions.Count)
            {
                different = false;
                for (int i = 0; !different && (i < messageAndFunctions.Count); i++)
                {
                    different = (messageAndFunctions[i] != mhTemplate.messageAndFunctions[i]);
                }
            }
            return different;
        }

        public override void ApplyFromTemplate(Component template)
        {
            MessageHandler mhTemplate = template as MessageHandler;
            messageAndFunctions.AddRange(mhTemplate.messageAndFunctions);
        }
    }
}
