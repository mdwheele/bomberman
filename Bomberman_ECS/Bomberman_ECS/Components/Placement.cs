using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    public class Placement : Component
    {
        private const byte CurrentVersion = 1;

        public Placement()
        {
            OrientationAngle = 0;
            Visible = true;
        }

        // Systems may need to know when position has changed. They could store their own
        // cache of positions for each Placement component over which they operate, but a more
        // efficient solution is to store the dirtybits in Placement itself. This may seem like
        // storing per-system information in a Component, but we generalize it by letting
        // systems provide their own dirty bits flags.
        private uint dirtyBits = 0xffffffff;

        public bool Visible { get; set; }
        public int Layer { get; set; }

        private Vector3 position;
        public Vector3 Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    dirtyBits = 0xffffffff;
                }
            }
        }

        public void ClearDirtyBit(uint bit)
        {
            dirtyBits &= ~bit;
        }
        public bool IsDirty(uint bit)
        {
            return (dirtyBits & bit) != 0; 
        }
        public void SetPositionNoDirty(Vector3 position, uint noDirtyBit)
        {
            Position = position;
            ClearDirtyBit(noDirtyBit);
        }

        public void SetPositionWholeNumber(Vector3 position)
        {
            position.X = (float)Math.Round(position.X);
            position.Y = (float)Math.Round(position.Y);
            position.Z = (float)Math.Round(position.Z);
            Position = position;
        }

        private float orientationAngle;
        public float OrientationAngle
        {
            get { return orientationAngle; }
            set
            {
                if (orientationAngle != value)
                {
                    orientationAngle = value;
                    if (orientationAngle < 0f)
                    {
                        orientationAngle += 360f;
                    }
                    orientationAngle %= 360f;
                }
            }
        }

        // Extra transforms.
        public float AdditionalVisualOrientation;
        public Vector3 AdditionalVisualPosition;

        public override void Init() { dirtyBits = 0xffffffff; }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            Visible = reader.ReadBoolean();
            Vector3 position = Vector3.Zero;
            position.X = reader.ReadSingle();
            position.Y = reader.ReadSingle();
            position.Z = reader.ReadSingle();
            Position = position;
            OrientationAngle = reader.ReadSingle(); // REVIEW: We could convert to ushort

            Layer = reader.ReadInt32();
            AdditionalVisualPosition.X = reader.ReadSingle();
            AdditionalVisualPosition.Y = reader.ReadSingle();
            AdditionalVisualPosition.Z = reader.ReadSingle();
            AdditionalVisualOrientation = reader.ReadSingle();
            dirtyBits = 0xffffffff;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);
            writer.Write(Visible);
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);
            writer.Write(OrientationAngle); // REVIEW: We could convert to ushort

            writer.Write(Layer);
            writer.Write(AdditionalVisualPosition.X);
            writer.Write(AdditionalVisualPosition.Y);
            writer.Write(AdditionalVisualPosition.Z);
            writer.Write(AdditionalVisualOrientation);
            //writer.Write(dirtyBits);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            // Always different - except in the case when this is owned by another entity.
            // Because in that case position will be determine by the parent. So we can just 
            // say "it's the same", since it will be instantly ignored. This is what lets us
            // make torches tiny in terms of persisted size.
            return !Universe.HasEntityOwner(entity.OwnerUniqueId);
        }

        public override void ApplyFromTemplate(Component template)
        {
            Placement placementTemplate = template as Placement;
            Position = placementTemplate.Position;
            OrientationAngle = placementTemplate.OrientationAngle;
            Layer = placementTemplate.Layer;
            Visible = placementTemplate.Visible;
            AdditionalVisualOrientation = placementTemplate.AdditionalVisualOrientation;
            AdditionalVisualPosition = placementTemplate.AdditionalVisualPosition;
            dirtyBits = 0xffffffff;
        }
    }
}
