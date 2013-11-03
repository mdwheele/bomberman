using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman_ECS
{
    public static class Extensions
    {
        public static Vector2 XY(this Vector3 v3)
        {
            return new Vector2(v3.X, v3.Y);
        }

        public static Point ToInteger(this Vector3 v3)
        {
            return new Point((int)Math.Round(v3.X), (int)Math.Round(v3.Y));
        }

        public static Point Add(this Point pThis, Point p)
        {
            return new Point(pThis.X + p.X, pThis.Y + p.Y);
        }

        /*
        public static Vector2 XZ(this Vector3 v3)
        {
            return new Vector2(v3.X, v3.Z);
        }*/

        public static Vector2 XY(this Vector4 v4)
        {
            return new Vector2(v4.X, v4.Y);
        }

        public static Vector2 ZW(this Vector4 v4)
        {
            return new Vector2(v4.Z, v4.W);
        }

        public static void Serialize(this List<Rectangle> rectList, BinaryWriter writer)
        {
            writer.Write(rectList.Count);
            for (int i = 0; i < rectList.Count; i++)
            {
                writer.Write(rectList[i].X);
                writer.Write(rectList[i].Y);
                writer.Write(rectList[i].Width);
                writer.Write(rectList[i].Height);
            }
        }

        public static void Deserialize(this List<Rectangle> rectList, BinaryReader reader)
        {
            int rectCount = reader.ReadInt32();
            for (int i = 0; i < rectCount; i++)
            {
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                rectList.Add(new Rectangle(x, y, width, height));
            }
        }

        // The .net hash implementation isn't sufficient for our purposes. Small string differences result
        // in small hash differences.
        // So instead we'll use a CRC32 hash.
        // We add in the top bit always, so we're only using 31 bits of info.
        // This is so we can use this id in a namespace that also includes un-named things.
        public static int CRC32Hash(this string text)
        {
            int id = (int)(Crc32.CalculateHashASCII(text) | 0x80000000);
#if DEBUG
            // So we can reverse lookup things in the debugger.
            UsedIds[id] = text;
#endif
            return id;
        }

#if DEBUG
        public static Dictionary<int, string> UsedIds = new Dictionary<int, string>();
#endif
    }
}
