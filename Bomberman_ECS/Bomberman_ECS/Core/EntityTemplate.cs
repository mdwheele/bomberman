using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bomberman_ECS.Core
{
    public class EntityTemplate
    {
        // Overriding/inheriting is done solely while parsing the template definition files.
        // The end result will be one of these.
        public EntityTemplate(string name, params Component[] componentTemplates)
        {
            Name = name;
            EntityTemplateId = name.CRC32Hash();

            ComponentTemplates = new List<Component>();
            ComponentTemplates.AddRange(componentTemplates);
            foreach (Component component in componentTemplates)
            {
                ComponentArray[EntityManager.GetComponentIdForComponent(component)] = component;
            }
        }

        public Component AddComponentIfNotPresent(int typeId, Component newOne)
        {
            if (ComponentArray[typeId] == null)
            {
                ComponentArray[typeId] = newOne;
                ComponentTemplates.Add(newOne);
            }
            return ComponentArray[typeId];
        }

        public static EntityTemplate NewInheritFrom(string name, string parent)
        {
            EntityTemplate cloneSource = EntityTemplateManager.GetTemplateByName(parent);
            return CreateFromOther(name, cloneSource);
        }

        public static EntityTemplate CreateFromOther(string name, EntityTemplate cloneSource)
        {
            List<Component> newComponents = new List<Component>();
            foreach (Component component in cloneSource.ComponentTemplates)
            {
                Component newComponent = (Component)Activator.CreateInstance(component.GetType());
                newComponent.ApplyFromTemplate(component);
                newComponents.Add(newComponent);
            }
            EntityTemplate foo = new EntityTemplate(name, newComponents.ToArray());
            foo.ParentName = cloneSource.Name;

            // Children! phil
            if ((cloneSource.ChildTemplates != null) && (cloneSource.ChildTemplates.Count > 0))
            {
                foo.AddChildren(cloneSource.ChildTemplates);
            }

            return foo;
        }
        public string ParentName { get; set; }

        internal void AddChildren(IEnumerable<EntityTemplate> childTemplates)
        {
            ChildTemplates = new List<EntityTemplate>(childTemplates);
        }

        public string Name { get; set; }
        public string Category { get; set; }    // Editor only
        public int EntityTemplateId { get; set; }

        // These hold the same data, but in different forms:
        public List<Component> ComponentTemplates { get; private set; }
        public Component[] ComponentArray = new Component[EntityManager.MaxComponentTypes];

        // This is for templates that have children that go in a TransformChildren template
        public List<EntityTemplate> ChildTemplates { get; private set; }


    }
}
