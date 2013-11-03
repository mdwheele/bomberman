using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;

namespace Bomberman_ECS.Util
{
    static class Helpers
    {
        public static void TransferPlacement(EntityManager em, int liveIdFrom, int liveIdTo)
        {
            Placement placementTo = (Placement)em.GetComponent(liveIdTo, ComponentTypeIds.Placement);
            Placement placementFrom = (Placement)em.GetComponent(liveIdFrom, ComponentTypeIds.Placement);
            placementTo.Position = placementFrom.Position;
            placementTo.OrientationAngle = placementFrom.OrientationAngle;
        }

        public static void CalculateDeathSpiralPoints(Rectangle bounds, List<Point> points)
        {
            points.Clear();
            bounds.Inflate(-1, -1);
            bool[] filled = new bool[bounds.Width * bounds.Height];
            Point point = new Point(bounds.Left, bounds.Top);
            points.Add(point);
            filled[0] = true;
            int dx = 1;
            int dy = 0;
            while (points.Count < filled.Length)
            {
                Point test = new Point(point.X + dx, point.Y + dy);
                int index = (test.X - bounds.Left) + (test.Y - bounds.Top) * bounds.Width;
                if (bounds.Contains(test) && !filled[index])
                {
                    filled[index] = true;
                    points.Add(test);
                    point = test;
                }
                else
                {
                    // Turn clockwise
                    int temp = dx;
                    dx = -dy;
                    dy = temp;
                }
            }
        }
    }
}
