using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    public class Universe
    {
        public Universe()
        {
        }

        // TODO: segregate unique ids by range. e.g. Regions will have unique ids that are different.
        // -ve unique ids are for things that are generated and have no static content.
        private int nextUniqueIdForStaticContent = 0x00000001;      // Nameless static content (present at start of game)
        private int nextUniqueIdForGeneratedContent = 0x20000000;   // Generated content (created during game. Monsters, weapons, etc..)
        public const int TopLevelGroupUniqueIdBase = 0x40000000;    // Top half of the range (for regions)
        // Things which are named take ids of the form 0x8000000 and higher (which are negative)

        // Region indices work as follows:
        //  - They start from TopLevelGroupUniqueIdBase
        //  - There are up to 64 regions per map (limited by the regionmask per chunk)
        //  - Each map supplies an additional offset
        //  - The final id for a region is (TopLevelGroupUniqueIdBase + regionIndex + mapOffset);

        public static bool IsTopLevelGroupId(int uniqueId)
        {
            // This does *not* include the global group.
            return uniqueId >= 0x40000000;
        }

        public static bool HasEntityOwner(int uniqueId)
        {
            return (uniqueId < 0x40000000) && (uniqueId != EntityManager.InvalidEntityUniqueId);
        }

        internal int GetNextUniqueIdForStaticContent()
        {
            return nextUniqueIdForStaticContent++;
        }

        internal int GetNextUniqueIdForGeneratedContent()
        {
            return nextUniqueIdForGeneratedContent++;
        }

        // Used in game for spawning things.
        public int GetNextUniqueObjectIdForGeneratedContent()
        {
            return nextUniqueIdForGeneratedContent++;
        }
    }
}
