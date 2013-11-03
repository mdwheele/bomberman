using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS.Systems
{
    static class SystemOrders
    {
        public static class Update
        {
            public const int DontCare = 0;
            public const int GameState = 10;
            public const int Input = 50;
            public const int Physics = 70;
            public const int Scripts = 100;     // Scripts may need to have finer control over this?
            public const int Grid = 120;
            public const int Explosion = 122;
            public const int PowerUp = 125;
            public const int Render = 130;
            public const int Sound = 140;
            public const int FrameAnimation = 150;
        }

        public static class Draw
        {
            public const int None = 0;
            public const int FrameAnimation = 20;
            public const int Render = 50;
            public const int GameState = 60;
        }
    }
}
