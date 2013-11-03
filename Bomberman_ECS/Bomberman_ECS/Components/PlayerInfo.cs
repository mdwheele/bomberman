using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Components
{
    // Information about a player (health, speed, powerups, etc)
    class PlayerInfo : Component
    {
        private const byte CurrentVersion = 1;

        public override void Init() { }

        public float MaxSpeed;
        public int PlayerNumber;

        // Information about the kind of bombs the player can make
        public int PermittedSimultaneousBombs;
        public bool FirstBombInfinite;
        public bool FirstBombLandMine;
        public bool RemoteTrigger;
        public BombState BombState;

        public override void Deserialize(BinaryReader reader)
        {
            byte version = reader.ReadByte();

            PermittedSimultaneousBombs = reader.ReadInt32();
            FirstBombInfinite = reader.ReadBoolean();
            FirstBombLandMine = reader.ReadBoolean();
            RemoteTrigger = reader.ReadBoolean();
            BombState.Deserialize(reader);
        }
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);

            writer.Write(PermittedSimultaneousBombs);
            writer.Write(FirstBombInfinite);
            writer.Write(FirstBombLandMine);
            writer.Write(RemoteTrigger);
            BombState.Serialize(writer);
        }

        public override bool IsDifferent(Entity entity, Component template)
        {
            PlayerInfo playerTemplate = template as PlayerInfo;
            return playerTemplate.MaxSpeed != MaxSpeed &&
                playerTemplate.PermittedSimultaneousBombs != PermittedSimultaneousBombs &&
                playerTemplate.BombState != BombState &&
                playerTemplate.FirstBombInfinite != FirstBombInfinite &&
                playerTemplate.FirstBombLandMine != FirstBombLandMine &&
                playerTemplate.RemoteTrigger != RemoteTrigger &&
                playerTemplate.PlayerNumber != PlayerNumber;
        }

        public override void ApplyFromTemplate(Component template)
        {
            PlayerInfo playerTemplate = template as PlayerInfo;
            MaxSpeed = playerTemplate.MaxSpeed;
            PermittedSimultaneousBombs = playerTemplate.PermittedSimultaneousBombs;
            BombState = playerTemplate.BombState;
            FirstBombInfinite = playerTemplate.FirstBombInfinite;
            FirstBombLandMine = playerTemplate.FirstBombLandMine;
            RemoteTrigger = playerTemplate.RemoteTrigger;
            PlayerNumber = playerTemplate.PlayerNumber;
        }
    }
}
