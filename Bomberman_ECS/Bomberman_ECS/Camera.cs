using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Bomberman_ECS
{
    // Orthographic camera
    public class Camera
    {
        public Camera(string name, int width, int height)
        {
            SetPixelDimensions(width, height, width, height);
            this.Name = name;
        }

        public string Name { get; private set; }

        public void SetPixelDimensions(int width, int height, int vpWidth, int vpHeight)
        {
            this.width = width;
            this.height = height;
            this.vpWidth = vpWidth;
            this.vpHeight = vpHeight;
        }
        private int width;
        private int height;
        private int vpWidth;
        private int vpHeight;

        public Vector3 LookAt { get; set; }
        public Vector3 DirectionToViewer { get; set; }

        public Vector3 Position { get; set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public void RestrictObjectPos(bool excludeBoundaries, ref Rectangle grid, ref Vector3 pos)
        {
            if (excludeBoundaries)
            {
                // We exclude the boundaries...
                pos.X = Math.Max(grid.Left + 1, pos.X);
                pos.Y = Math.Max(grid.Top + 1, pos.Y);
                pos.X = Math.Min(grid.Right - 2, pos.X);
                pos.Y = Math.Min(grid.Bottom - 2, pos.Y);
            }
            else
            {
                pos.X = Math.Max(grid.Left, pos.X);
                pos.Y = Math.Max(grid.Top, pos.Y);
                pos.X = Math.Min(grid.Right - 1, pos.X);
                pos.Y = Math.Min(grid.Bottom - 1, pos.Y);
            }
        }

        public const int LongSize = 17;
        public const int ShortSize = 12;

        public float TotalScale { get; private set; }

        public void Update()
        {
            Position = DirectionToViewer + LookAt;

            float cameraRotation = 0;

            // This adjusts ourselves for screensize!
            float scaleInt = (float)width / LongSize;

            float Scale = scaleInt;

            float xOffset = -LookAt.X * Scale;
            float yOffset = -LookAt.Y * Scale;

            TotalScale = Scale;

            float xDiff = -(width - vpWidth) * 0.5f;
            float yDiff = -(height - vpHeight) * 0.5f;

            View = Matrix.CreateScale(TotalScale, TotalScale, 1) * Matrix.CreateRotationZ(cameraRotation) * Matrix.CreateTranslation(xOffset + xDiff, yOffset + yDiff, 0);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, vpWidth, vpHeight, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            Projection = halfPixelOffset * projection;
        }
    }
}
