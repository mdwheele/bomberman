using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Bomberman_ECS.Core
{
    // We only need to load this for debug builds or the editor, really.
    // For this reason, we don't use any optimized format for reading in data.
    // We keep it human-readable.
    class CRC32StringIds
    {
        private Dictionary<int, string> idToStrings = new Dictionary<int, string>();

        public void AddString(string text)
        {
            int id = text.CRC32Hash();
            string value;
            if (!idToStrings.TryGetValue(id, out value))
            {
                idToStrings[id] = text;
            }
            else
            {
                Debug.Assert(value == text, "String collision!");
            }
        }

        // Returns null if none
        public string GetString(int id)
        {
            string text = null;
            idToStrings.TryGetValue(id, out text);
            return text;
        }

        // Use a nice readable format for this
        public void Load(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string word = null;
                    do
                    {
                        word = reader.ReadLine();
                        if (word != null)
                        {
                            idToStrings[word.CRC32Hash()] = word;
                        }
                    } while (word != null);
                    reader.Close();
                }
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        public void Save(string filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(stream);
                foreach (KeyValuePair<int, string> pair in idToStrings)
                {
                    writer.WriteLine(pair.Value);
                }
                writer.Close();
            }
        }
    }
}
