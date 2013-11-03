using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    enum ExplosionBarrier
    {
        // Ordered in strength from weakest to strongest
        None = 0,       // No barrier
        Soft = 1,       // Explosions can go here, but not through
        Hard = 2,       // Explosions can't even go here
    }

    // Describes how an entity handles a bomb explosion.
    class ExplosionImpact : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { }

        public ExplosionBarrier Barrier { get; set; }
        public bool ShouldSendMessage { get; set; }       // Send a message to this thing indicating it is in an explosion (do this every frame)

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            Barrier = (ExplosionBarrier)reader.ReadInt32();
            ShouldSendMessage = reader.ReadBoolean();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write((int)Barrier);
            writer.Write(ShouldSendMessage);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            ExplosionImpact iTemplate = template as ExplosionImpact;
            return iTemplate.Barrier != Barrier &&
                iTemplate.ShouldSendMessage != ShouldSendMessage;
        }

        public override void ApplyFromTemplate(Component template)
        {
            ExplosionImpact iTemplate = template as ExplosionImpact;
            Barrier = iTemplate.Barrier;
            ShouldSendMessage = iTemplate.ShouldSendMessage;
        }
    }
}
