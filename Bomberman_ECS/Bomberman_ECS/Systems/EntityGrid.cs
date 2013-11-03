using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman_ECS.Systems
{
    // Keeps track of which entities are at which positions.
    class EntityGrid
    {
        private void SetSize(int width, int height)
        {
            int size = width * height;
            if ((uniqueIds == null) || (uniqueIds.Length != size))
            {
                uniqueIds = new List<int>[size];

                for (int i = 0; i < uniqueIds.Length; i++)
                {
                    uniqueIds[i] = new List<int>(DefaultListSize);
                }
            }
        }

        private const int DefaultListSize = 4; // Max number of entities at one location to avoid reallocs
        private Rectangle grid;
        public Rectangle Bounds
        {
            get { return grid; }
            set
            {
                if (grid != value)
                {
                    grid = value;
                    SetSize(grid.Width, grid.Height);
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < uniqueIds.Length; i++)
            {
                uniqueIds[i].Clear();
            }
        }

        public void Assign(int x, int y, int uniqueId)
        {
            if (grid.Contains(x, y))
            {
                uniqueIds[x + grid.Width * y].Add(uniqueId);
            }
        }

        public void QueryUniqueIdsAt(Point point, List<int> results)
        {
            QueryUniqueIdsAt(point.X, point.Y, results);
        }

        public void QueryUniqueIdsAt(int x, int y, List<int> results)
        {
            results.Clear();
            if (grid.Contains(x, y))
            {
                results.AddRange(uniqueIds[x + grid.Width * y]);
            }
        }

        // This allows for multiple things at each square.
        // There exist one startIndex and endIndex for each grid square. They are indices into uniqueIds.
        private List<int>[] uniqueIds;


    }
}
