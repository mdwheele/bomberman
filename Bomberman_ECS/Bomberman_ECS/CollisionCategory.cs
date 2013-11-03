using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS
{
    static class CollisionCategory
    {
        public const uint None = 0x00000000;
        public const uint Player1 = 0x00000001;
        public const uint Player2 = 0x00000002;
        public const uint Player3 = 0x00000004;
        public const uint Player4 = 0x00000008;
        public const uint AllPlayers = 0x0000000f;
        public const uint Bricks = 0x00000010;
        public const uint Bombs = 0x00000020;
    }
}
