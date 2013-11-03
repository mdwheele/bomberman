using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    // 7 * 4 bytes: 28 bytes.
    public class Aspect : Component
    {
        private const byte CurrentVersion = 1;

        public Aspect()
        {
            Size = Vector2.One;
            Tint = Color.White;
        }

        public int ModelNameId { get; set; }
        public int FrameIndex { get; set; }     // Horizontal
        public int VarietyIndex { get; set; }   // Vertical

        // Scaling here (instead of content pipeline) lets us re-use animation data for different size models.
        public Vector2 Size { get; set; }
        public Color Tint { get; set; }

        public SpriteEffects SpriteEffects { get; set; }

        public override void Init()
        {
        }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            ModelNameId = reader.ReadInt32();
            FrameIndex = reader.ReadInt32();
            VarietyIndex = reader.ReadInt32();
            SpriteEffects = (SpriteEffects)reader.ReadInt32();
            Vector2 size;
            size.X = reader.ReadSingle();
            size.Y = reader.ReadSingle();
            Size = size;
            Color color = Color.Black;
            color.PackedValue = reader.ReadUInt32();
            Tint = color;
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);
            writer.Write(ModelNameId);
            writer.Write(FrameIndex);
            writer.Write(VarietyIndex);
            writer.Write((int)SpriteEffects);
            writer.Write(Size.X);
            writer.Write(Size.Y);
            writer.Write(Tint.PackedValue);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            Aspect aspect = template as Aspect;
            bool different = (ModelNameId != aspect.ModelNameId) &&
                (Size != aspect.Size) &&
                (Tint != aspect.Tint) &&
                (FrameIndex != aspect.FrameIndex) &&
                (SpriteEffects != aspect.SpriteEffects) &&
                (VarietyIndex != aspect.VarietyIndex);
            return different;
        }

        public override void ApplyFromTemplate(Component template)
        {
            Aspect aspect = template as Aspect;
            this.ModelNameId = aspect.ModelNameId;
            this.Size = aspect.Size;
            this.Tint = aspect.Tint;
            this.FrameIndex = aspect.FrameIndex;
            this.SpriteEffects = aspect.SpriteEffects;
            this.VarietyIndex = aspect.VarietyIndex;
        }
    }
}
