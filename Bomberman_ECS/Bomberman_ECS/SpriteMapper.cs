using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman_ECS
{
    [Flags]
    enum SpriteFlags
    {
        Default = 0,
        FramesVertical = 0x1,
    }

    class SpriteMapper
    {
        private Dictionary<int, Sprite> spriteCache = new Dictionary<int, Sprite>();

        public Sprite GetSpriteForId(int it) { return spriteCache[it]; }

        private void AddTextureWithCustomVarieties(string name, SpriteSheet spriteSheet, Point[] points)
        {
            Debug.Assert(!spriteCache.ContainsKey(name.CRC32Hash()));
            spriteCache[name.CRC32Hash()] = Sprite.CreateCustomVarieties(spriteSheet, points);
        }

        private void AddTextureWithCustomFrames(string name, SpriteSheet spriteSheet, Point[] points)
        {
            Debug.Assert(!spriteCache.ContainsKey(name.CRC32Hash()));
            spriteCache[name.CRC32Hash()] = Sprite.CreateCustomFrames(spriteSheet, points);
        }

        public void AddTexture(string name, SpriteSheet spriteSheet, int frameStart, int frameCount, int varietyStart, int varietyCount, SpriteFlags flags = SpriteFlags.Default)
        {
            Debug.Assert(!spriteCache.ContainsKey(name.CRC32Hash()));

            if ((flags & SpriteFlags.FramesVertical) == SpriteFlags.FramesVertical)
            {
                spriteCache[name.CRC32Hash()] = Sprite.CreateVerticalFramesHorizontalVarieties(spriteSheet, frameStart, frameCount, varietyStart, varietyCount);
            }
            else
            {
                spriteCache[name.CRC32Hash()] = Sprite.CreateHorizontalFramesVerticalVarieties(spriteSheet, frameStart, frameCount, varietyStart, varietyCount);
            }
        }

        public void AddTexture(string name, Texture2D texture, int numberOfFrames = 1, int numberOfVarieties = 1, SpriteFlags flags = SpriteFlags.Default)
        {
            SpriteSheet ss;
            if ((flags & SpriteFlags.FramesVertical) == SpriteFlags.FramesVertical)
            {
                ss = new SpriteSheet(texture, numberOfVarieties, numberOfFrames);
            }
            else
            {
                ss = new SpriteSheet(texture, numberOfFrames, numberOfVarieties);
            }
            AddTexture(name, ss, 0, numberOfFrames, 0, numberOfVarieties, flags);
        }
    }
}
