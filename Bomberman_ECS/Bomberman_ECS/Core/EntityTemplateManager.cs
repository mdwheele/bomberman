using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    public static class EntityTemplateManager
    {
        static EntityTemplateManager()
        {
            EntityTemplatesById = new Dictionary<int, EntityTemplate>();
        }

        public static EntityTemplate GetTemplateByName(string name)
        {
            return GetTemplateById(name.CRC32Hash());
        }

        public static void AddTemplate(EntityTemplate template, bool overwriteOk = false)
        {
            int id = template.Name.CRC32Hash();
            Debug.Assert(overwriteOk || !EntityTemplatesById.ContainsKey(id), "Template " + template.Name + " already exists.");
            EntityTemplatesById[id] = template;
        }

        private static Dictionary<int, EntityTemplate> EntityTemplatesById { get; set; }
        public static EntityTemplate GetTemplateById(int id)
        {
            return EntityTemplatesById[id];
        }

        public static void PopulateListWithCategorizedTemplates(List<EntityTemplate> templates)
        {
            foreach (KeyValuePair<int, EntityTemplate> type in EntityTemplatesById)
            {
                if ((type.Value.Category != null) && (type.Value.Category != string.Empty))
                {
                    templates.Add(type.Value);
                }
            }
        }
    }
}
