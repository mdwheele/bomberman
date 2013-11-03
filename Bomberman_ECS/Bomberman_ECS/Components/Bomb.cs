using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    // Describes the "bomb" part of a bomb.
    class Bomb : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { OwnerUniqueId = EntityManager.InvalidEntityUniqueId; }

        public float Countdown;
        public int OwnerUniqueId;   // Who owns this bomb?
        public bool Triggered;
        public BombState State;

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            Countdown = reader.ReadSingle();
            State.Deserialize(reader);
            OwnerUniqueId = reader.ReadInt32();
            Triggered = reader.ReadBoolean();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);
            writer.Write(Countdown);
            State.Serialize(writer);
            writer.Write(OwnerUniqueId);
            writer.Write(Triggered);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            return true; // Since it has OwnerUniqueId;
        }

        public override void ApplyFromTemplate(Component template)
        {
            Bomb bombTemplate = template as Bomb;
            State = bombTemplate.State;
            OwnerUniqueId = bombTemplate.OwnerUniqueId;
            Countdown = bombTemplate.Countdown;
            Triggered = bombTemplate.Triggered;
        }
    }
}
