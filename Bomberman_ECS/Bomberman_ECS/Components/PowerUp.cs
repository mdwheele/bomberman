using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    enum PowerUpType
    {
        BombUp,
        FireUp,
        SpeedUp,
        FullFire,
        BombPunch,
        BombKick,
        PowerGlove,
        LineBomb,
        PowerBomb,
        DangerousBomb,
        PassThroughBomb,
        RemoteBomb,
        LandMineBomb,
        BombDown,
        FireDown,
        SpeedDown,
    }

    class PowerUp : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { }

        public PowerUpType Type { get; set; }
        public int SoundId { get; set; }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            Type = (PowerUpType)reader.ReadInt32();
            SoundId = reader.ReadInt32();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write((int)Type);
            writer.Write(SoundId);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            PowerUp powerUpTemplate = template as PowerUp;
            return (powerUpTemplate.Type != Type) &&
                (powerUpTemplate.SoundId != SoundId);
        }

        public override void ApplyFromTemplate(Component template)
        {
            PowerUp powerUpTemplate = template as PowerUp;
            Type = powerUpTemplate.Type;
            SoundId = powerUpTemplate.SoundId;
        }
    }
}
