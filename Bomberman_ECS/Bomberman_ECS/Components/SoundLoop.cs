using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    public class SoundLoop : Component
    {
        private const byte CurrentVersion = 1;

        public float Range { get; set; }
        public float FallOff { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }

        // If true, all instances of these combine to one instance (the loudest)
        // XACT can do this for us automatically, but we want more control... specifically sound loudness based on distance
        // from a line.
        public bool Collective { get; set; }

        public int CueNameId { get; set; }

        public override void Init() { }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            Range = reader.ReadSingle();
            FallOff = reader.ReadSingle();
            CueNameId = reader.ReadInt32();
            Volume = reader.ReadSingle();
            Collective = reader.ReadBoolean();
            Pitch = reader.ReadSingle();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Range);
            writer.Write(FallOff);
            writer.Write(CueNameId);
            writer.Write(Volume);
            writer.Write(Collective);
            writer.Write(Pitch);
        }
        public override bool IsDifferent(Entity entity, Component template)
        {
            SoundLoop sound = template as SoundLoop;
            bool different = (Range != sound.Range) &&
                (FallOff != sound.FallOff) &&
                (CueNameId != sound.CueNameId) &&
                (Volume != sound.Volume) &&
                (Collective != sound.Collective) &&
                (Pitch != sound.Pitch);
            return different;
        }

        public override void ApplyFromTemplate(Component template)
        {
            SoundLoop sound = template as SoundLoop;
            this.Range = sound.Range;
            this.FallOff = sound.FallOff;
            this.CueNameId = sound.CueNameId;
            this.Volume = sound.Volume;
            this.Collective = sound.Collective;
            this.Pitch = sound.Pitch;
        }
    }
}
