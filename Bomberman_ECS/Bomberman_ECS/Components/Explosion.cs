using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    [Flags]
    enum PropagationDirection
    {
        None        = 0x00000000,
        Left        = 0x00000001,
        UpperLeft   = 0x00000002,
        Up          = 0x00000004,
        UpperRight  = 0x00000008,
        Right       = 0x00000010,
        BottomRight = 0x00000020,
        Bottom      = 0x00000040,
        BottomLeft  = 0x00000080,
        NESW        = Left | Up | Right | Bottom,
        All         = 0x000000ff,
    }

    class Explosion : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { }

        public float Countdown;                // How long this lasts
        public float PropagationCountDown;     // How long before it propagates
        public bool MadeInitialBlast;
        public BombState State;

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            Countdown = reader.ReadSingle();
            PropagationCountDown = reader.ReadSingle();
            State.Deserialize(reader);
            MadeInitialBlast = reader.ReadBoolean();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(Countdown);
            writer.Write(PropagationCountDown);
            State.Serialize(writer);
            writer.Write(MadeInitialBlast);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            Explosion explosionTemplate = template as Explosion;
            return explosionTemplate.Countdown != Countdown &&
                explosionTemplate.PropagationCountDown != PropagationCountDown &&
                explosionTemplate.State != State &&
                explosionTemplate.MadeInitialBlast != MadeInitialBlast;
        }

        public override void ApplyFromTemplate(Component template)
        {
            Explosion explosionTemplate = template as Explosion;
            Countdown = explosionTemplate.Countdown;
            State = explosionTemplate.State;
            PropagationCountDown = explosionTemplate.PropagationCountDown;
            MadeInitialBlast = explosionTemplate.MadeInitialBlast;
        }
    }
}
