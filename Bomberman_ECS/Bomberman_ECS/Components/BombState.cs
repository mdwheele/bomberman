using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bomberman_ECS.Components
{
    // Helper for components that have to do with bombs.
    struct BombState
    {
        public bool IsPassThrough;
        public bool IsHardPassThrough;
        public int Range;
        public PropagationDirection PropagationDirection;

        public void Deserialize(BinaryReader reader)
        {
            IsPassThrough = reader.ReadBoolean();
            IsHardPassThrough = reader.ReadBoolean();
            Range = reader.ReadInt32();
            PropagationDirection = (PropagationDirection)reader.ReadInt32();
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(IsPassThrough);
            writer.Write(IsHardPassThrough);
            writer.Write(Range);
            writer.Write((int)PropagationDirection);
        }

        public static bool operator !=(BombState m1, BombState m2)
        {
            return m1.IsPassThrough != m2.IsPassThrough ||
                m1.IsHardPassThrough != m2.IsHardPassThrough ||
                m1.Range != m2.Range ||
                m1.PropagationDirection != m2.PropagationDirection;
        }
        public static bool operator ==(BombState m1, BombState m2)
        {
            return m1.IsPassThrough == m2.IsPassThrough &&
                m1.IsHardPassThrough == m2.IsHardPassThrough &&
                m1.Range == m2.Range &&
                m1.PropagationDirection == m2.PropagationDirection;
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
}
