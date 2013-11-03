using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS
{
    static class Messages
    {
        // We need to document which parameters these messages take.
        public const uint LoadLevel = 0;
        public const uint SetVelocity = 1;
        public const uint TriggerBomb = 2;
        public const uint InExplosion = 3;
        public const uint RemoteTriggerAllPlayerBombs = 4;
        public const uint PlaySound = 5;
        public const uint AddScript = 6;
        public const uint RemoveScript = 7;
        public const uint HitByInitialExplosion = 8;
        public const uint QueryComponentAtGrid = 9; // x, y, componentid. Answer is a uniqueid.
        public const uint QueryWinningPlayer = 10;
        public const uint DirectKill = 11;
    }
}
