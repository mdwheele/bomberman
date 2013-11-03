using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman_ECS
{
    // Basic info about the texture
    class SpriteSheet
    {
        public SpriteSheet(Texture2D texture, int xCount, int yCount)
        {
            this.xCount = xCount;
            this.yCount = yCount;
            this.Texture = texture;
            this.xFrame = texture.Width / xCount;
            this.yFrame = texture.Height / yCount;
        }

        public Texture2D Texture;
        private int xCount, yCount;
        private int xFrame, yFrame;

        public int YCount { get { return yCount; } }
        public int XCount { get { return xCount; } }

        public void GetBounds(int x, int y, out Rectangle rectangle)
        {
            rectangle.X = x * xFrame;
            rectangle.Y = y * yFrame;
            rectangle.Width = xFrame;
            rectangle.Height = yFrame;
        }
    }
}
