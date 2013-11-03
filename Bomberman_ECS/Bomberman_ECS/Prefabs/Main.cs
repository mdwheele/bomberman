using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS.Prefabs
{
    static partial class Prefabs
    {
        public static void Initialize()
        {
            MakeBricks();
            MakeCharacters();
            MakeBombs();
            MakeExplosions();
            MakePowerUps();
            MakeGame();
        }
    }
}
