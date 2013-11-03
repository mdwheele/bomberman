using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bomberman_ECS.Core
{
    public abstract class Component
    {
        // REVIEW: Do we need these?
        public abstract void Init();

        public abstract void Deserialize(BinaryReader reader);
        public abstract void Serialize(BinaryWriter writer);

        public abstract bool IsDifferent(Entity entity, Component template);
        public abstract void ApplyFromTemplate(Component template);
    }
}
