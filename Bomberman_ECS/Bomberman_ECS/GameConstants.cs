using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS
{
    static class GameConstants
    {
        public static float GameLength = 120f;  // 2 minutes
        public static float TimeRunningOutWarningTime = 5f;

        public static float PlayerDefaultSpeed = 4.0f;
        public static float PlayerMinSpeed = 2.0f;
        public static float PlayerSpeedUp = 1.2f;
        public static float BombSecondsToExplode = 2.5f;
        public static float ExplosionDuration = 0.4f;
        public static float ExplosionFrameRate = 19f;   // This is closely tied to ExplosionDuration
        public static float PropagationTime = 0.05f;
        public static double PowerUpChance = 0.4f;
        public static float InfiniteTime = 1000f;
        public static float LandMineFuseTime = 0.33f;
        public static float LandMineAnimationFrameRate = 6f;

        public static float PulsationPeriod = 2.1f;
        public static float PulsationSizeMin = 0.97f;
        public static float PulsationSizeMax = 1.05f;
        
        public static float PowerUpWigglePeriod = 0.25f;
        public static float PowerUpWiggleExtent = 0.06f;

        public static float DeathBlocksArrivePeriod = 0.2f;

        public static float ExplosionVolume = 0.4f;
        public static float ThudVolume = 0.3f;

        public static int InfiniteRange = 1000;
        public static int MaxDangerousBombRange = 4;
    }
}
