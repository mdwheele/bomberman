using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS.Components
{
    public static class ComponentTypeIds
    {
        public const int Placement = 0;
        public const int Aspect = 1;
        public const int Movement = 2;
        public const int InputMap = 3;
        public const int Player = 4;
        public const int Bomb = 5;
        public const int Explosion = 6;
        public const int ExplosionImpact = 7;
        public const int InputHandlers = 8;
        public const int MessageHandler = 9;
        public const int PowerUp = 10;
        public const int Physics = 11;
        public const int SoundLoop = 12;
        public const int ScriptContainer = 13;
        public const int FrameAnimation = 14;
        public const int GameState = 15;

        public static int GetBit(int componentTypeId)
        {
            return 0x1 << componentTypeId;
        }
    }
}
