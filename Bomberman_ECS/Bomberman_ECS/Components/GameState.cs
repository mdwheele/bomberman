using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    // This represents the current game state, such as the time remaining, and any unique things about this level.
    class GameState : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { }

        public float TimeRemaining;
        public Rectangle Bounds;
        public bool IsGameOver;
        public int WinningPlayerUniqueId;

        // Death blocks
        public int DeathBlockCount;

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            TimeRemaining = reader.ReadSingle();
            Bounds.X = reader.ReadInt32();
            Bounds.Y = reader.ReadInt32();
            Bounds.Width = reader.ReadInt32();
            Bounds.Height = reader.ReadInt32();
            IsGameOver = reader.ReadBoolean();
            WinningPlayerUniqueId = reader.ReadInt32();
            DeathBlockCount = reader.ReadInt32();
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(TimeRemaining);
            writer.Write(Bounds.X);
            writer.Write(Bounds.Y);
            writer.Write(Bounds.Width);
            writer.Write(Bounds.Height);
            writer.Write(IsGameOver);
            writer.Write(WinningPlayerUniqueId);
            writer.Write(DeathBlockCount);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            return true;
        }

        public override void ApplyFromTemplate(Component template)
        {
            GameState gameTemplate = template as GameState;
            TimeRemaining = gameTemplate.TimeRemaining;
            Bounds = gameTemplate.Bounds;
            DeathBlockCount = gameTemplate.DeathBlockCount;
            WinningPlayerUniqueId = gameTemplate.WinningPlayerUniqueId;
            IsGameOver = gameTemplate.IsGameOver;
        }
    }
}
