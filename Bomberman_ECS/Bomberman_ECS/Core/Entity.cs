using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    public class Entity
    {
        public static byte CurrentVersion = 1;

        public int UniqueId { get; internal set; }          // Unique across all time/objects, and possibly corresponds to entity name
        public int OwnerUniqueId { get; internal set; }     // Owner - typically a region, but could be another entity
        public int TemplateId { get; internal set; }
        internal int ComponentTypesBitField { get; set; }   // bitfield of current component types attached. Might end up being long
        public short LiveId { get; internal set; }          // Basically, a re-usable thing for a live object that allows quick lookups

        public bool MatchesComponents(int allComponentTypes, int anyComponentTypes)
        {
            return ((ComponentTypesBitField & allComponentTypes) == allComponentTypes) &&
                    ((ComponentTypesBitField & anyComponentTypes) != 0);
        }

        public void SetOwnerUniqueId(int uniqueId)
        {
            OwnerUniqueId = uniqueId;
        }
    }
}
