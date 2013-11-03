using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman_ECS.Core
{
    static class ComponentTypeIdHelper
    {
        public static int GetBit(int componentTypeId)
        {
            return 0x1 << componentTypeId;
        }

        public const int AllMask = 0x7fffffff;
    }
}
