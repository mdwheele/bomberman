using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    public class FrameAnimation : Component
    {
        private const byte CurrentVersion = 1;

        // Requires placement
        public FrameAnimation()
        {
            FrameRate = 30;
            Direction = 1;
        }

        // Current this is restricted to rotation
        public float FrameRate { get; set; }
        public float EllapsedTime { get; set; }
        public bool RandomFrame { get; set; }
        public bool Loop { get; set; }
        public int Direction { get; set; }

        public override void Init()
        {
            FrameRate = 30;
            Direction = 1;
        }

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            FrameRate = reader.ReadSingle();
            EllapsedTime = reader.ReadSingle();
            RandomFrame = reader.ReadBoolean();
            Loop = reader.ReadBoolean();
            Direction = reader.ReadInt32();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(FrameRate);
            writer.Write(EllapsedTime);
            writer.Write(RandomFrame);
            writer.Write(Loop);
            writer.Write(Direction);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            return true;
            /*
            FrameAnimation animation = template as FrameAnimation;
            bool different = this.FrameRate != animation.FrameRate;
            if (!different)
            {
                different = this.EllapsedTime != animation.EllapsedTime;
            }
            if (!different)
            {
                different = this.RandomFrame != animation.RandomFrame;
            }
            throw new NotImplementedException();
            return different;*/
        }

        public override void ApplyFromTemplate(Component template)
        {
            FrameAnimation animation = template as FrameAnimation;
            this.EllapsedTime = animation.EllapsedTime;
            this.FrameRate = animation.FrameRate;
            this.RandomFrame = animation.RandomFrame;
            this.Loop = animation.Loop;
            this.Direction = animation.Direction;
        }
    }

}
