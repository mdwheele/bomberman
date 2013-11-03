using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Levels
{
    class LevelLoader
    {
        public LevelLoader(EntityManager em, int ownerId)
        {
            this.em = em;
            this.ownerId = ownerId;

            CharToType[] charToTypesTemp = new CharToType[]
            {
                new CharToType('1', "HardBlock"),
            };
            foreach (CharToType ctt in charToTypesTemp)
            {
                charToTypes[ctt.Char] = ctt.Type;
            }
        }

        private Random random = new Random();
        private const double SoftBlockChance = 0.7f;
        private EntityManager em;
        private int ownerId;

        struct CharToType
        {
            public CharToType(char theChar, string type) { Char = theChar; Type = type; }
            public char Char;
            public string Type;
        }
        private Dictionary<char, string> charToTypes = new Dictionary<char, string>();

        public void Load(int number, out Rectangle bounds)
        {
            string name = string.Format(@"Levels\Level{0:d2}.txt", number);

            int width = 0;
            int height = 0;
            int x = 0;
            int y = 0;
            using (Stream file = File.OpenRead(name))
            {
                StreamReader reader = new StreamReader(file);

                string foo = reader.ReadLine();
                while ((foo != null) && (foo.Length > 0))
                {
                    x = 0;
                    width = Math.Max(width, foo.Length);

                    foreach (char c in foo)
                    {
                        string objectType;
                        if (charToTypes.TryGetValue(c, out objectType))
                        {
                            AddObject(x, y, objectType);
                        }
                        else if (c == ' ')
                        {
                            if (random.NextDouble() < SoftBlockChance)
                            {
                                AddObject(x, y, "SoftBlock");
                            }
                        }

                        x++;
                    }

                    foo = reader.ReadLine();

                    height++;
                    y++;
                }
            }

            bounds = new Rectangle(0, 0, width, height);
        }

        private void AddObject(int x, int y, string type)
        {
            Entity entity = em.AllocateForGeneratedContent(type, ownerId);

            Placement placement = (Placement)em.GetComponent(entity, ComponentTypeIds.Placement);
            placement.Position = new Vector3(x, y, 0);
        }
    }
}
