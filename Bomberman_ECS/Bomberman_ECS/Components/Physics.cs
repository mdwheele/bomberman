using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    enum BoundingVolumeType : byte
    {
        Circle,
        Box,
    }

    class Physics : Component
    {
        private const byte CurrentVersion = 1;

        public float Size { get; set; }
        public BoundingVolumeType BoundingVolumeType { get; set; }
        public bool IsDynamic { get; set; }   // Does it move?
        public uint CollidesWidth { get; set; }
        public uint CollisionCategories { get; set; }
        public bool IsSensor { get; set; }

        public override void Init() { }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            Size = reader.ReadSingle();
            BoundingVolumeType = (BoundingVolumeType)reader.ReadInt32();
            IsDynamic = reader.ReadBoolean();
            CollisionCategories = reader.ReadUInt32();
            CollidesWidth = reader.ReadUInt32();
            IsSensor = reader.ReadBoolean();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(Size);
            writer.Write((int)BoundingVolumeType);
            writer.Write(IsDynamic);
            writer.Write(CollisionCategories);
            writer.Write(CollidesWidth);
            writer.Write(IsSensor);
        }
        public override bool IsDifferent(Entity entity, Component template)
        {
            Physics collision = template as Physics;
            bool different = 
                (BoundingVolumeType != collision.BoundingVolumeType) &&
                (IsDynamic != collision.IsDynamic) &&
                (CollisionCategories != collision.CollisionCategories) &&
                (CollidesWidth != collision.CollidesWidth) &&
                (Size != collision.Size) &&
                (IsSensor != collision.IsSensor);
            return different;
        }

        public override void ApplyFromTemplate(Component template)
        {
            Physics collision = template as Physics;
            this.BoundingVolumeType = collision.BoundingVolumeType;
            this.IsDynamic = collision.IsDynamic;
            this.Size = collision.Size;
            this.CollisionCategories = collision.CollisionCategories;
            this.CollidesWidth = collision.CollidesWidth;
            this.IsSensor = collision.IsSensor;
        }
    }
}
