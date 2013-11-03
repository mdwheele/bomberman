using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman_ECS
{
    // Defines a sprite within a texture. Could be in any direction!
    // Takes into account frames vs varieties.
    class Sprite
    {
        private Sprite(SpriteSheet spriteSheet)
        {
            this.spriteSheet = spriteSheet;
        }
        private SpriteSheet spriteSheet;

        public static Sprite CreateHorizontalFramesVerticalVarieties(SpriteSheet ss, int frameStart, int frameCount, int varietyStart, int varietyCount)
        {
            Sprite sprite = new Sprite(ss);
            Debug.Assert(ss.YCount >= (varietyStart + varietyCount));
            Debug.Assert(ss.XCount >= (frameStart + frameCount));

            sprite.varieties = new Point[varietyCount];
            for (int i = 0; i < varietyCount; i++)
            {
                sprite.varieties[i] = new Point(0, i + varietyStart);
            }
            sprite.frames = new Point[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                sprite.frames[i] = new Point(i + frameStart, 0);
            }

            return sprite;
        }

        public static Sprite CreateVerticalFramesHorizontalVarieties(SpriteSheet ss, int frameStart, int frameCount, int varietyStart, int varietyCount)
        {
            Sprite sprite = new Sprite(ss);
            Debug.Assert(ss.XCount >= (varietyStart + varietyCount));
            Debug.Assert(ss.YCount >= (frameStart + frameCount));

            sprite.varieties = new Point[varietyCount];
            for (int i = 0; i < varietyCount; i++)
            {
                sprite.varieties[i] = new Point(i + varietyStart, 0);
            }
            sprite.frames = new Point[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                sprite.frames[i] = new Point(0, i + frameStart);
            }

            return sprite;
        }

        public static Sprite CreateCustomVarieties(SpriteSheet ss, Point[] points)
        {
            Sprite sprite = new Sprite(ss);
            sprite.varieties = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                sprite.varieties[i] = points[i];
            }
            sprite.frames = new Point[1];
            sprite.frames[0] = Point.Zero;
            return sprite;
        }

        public static Sprite CreateCustomFrames(SpriteSheet ss, Point[] points)
        {
            Sprite sprite = new Sprite(ss);
            sprite.frames = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                sprite.frames[i] = points[i];
            }
            sprite.varieties = new Point[1];
            sprite.varieties[0] = Point.Zero;
            return sprite;
        }

        public Texture2D Texture { get { return spriteSheet.Texture; } }

        public Point[] varieties;
        public Point[] frames;
        public int VarietyCount
        {
            get { return varieties.Length; }
        }
        public int FrameCount
        {
            get { return frames.Length; }
        }

        public void GetBounds(int variety, int frame, out Rectangle rectangle)
        {
            int x = varieties[variety].X + frames[frame].X;
            int y = varieties[variety].Y + frames[frame].Y;
            spriteSheet.GetBounds(x, y, out rectangle);
        }
    }

}
