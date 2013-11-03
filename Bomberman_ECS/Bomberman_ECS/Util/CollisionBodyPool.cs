using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;

namespace Bomberman_ECS.Util
{
    // Note: A unique CollisionBodyPool should be used for every shape type.
    class CollisionBodyPool
    {
        public CollisionBodyPool(World world)
        {
            this.world = world;
        }

        private World world;
        private List<Body> bodyPoolPoly = new List<Body>();
        private List<Body> bodyPoolCircle = new List<Body>();
        private int inactiveCountPoly = 0;
        private int inactiveCountCircle = 0;

        public Body ActivateBodyPolygon(Vertices vertices, BodyType bodyType)
        {
            Body body = null;
            if (inactiveCountPoly == 0)
            {
                body = new Body(world);
                body.BodyType = bodyType;
                PolygonShape shape = new PolygonShape(vertices, 1f);
                body.CreateFixture(shape);
                bodyPoolPoly.Add(body);
            }
            else
            {
                for (int i = 0; i < bodyPoolPoly.Count; i++)
                {
                    if (!bodyPoolPoly[i].Enabled)
                    {
                        Body bodyTemp = bodyPoolPoly[i];
                        if (bodyTemp.FixtureList[0].ShapeType == ShapeType.Polygon)
                        {
                            body = bodyTemp;
                            ((PolygonShape)body.FixtureList[0].Shape).Set(vertices);
                            body.BodyType = bodyType;
                            // Must enable this *after* setting vertices
                            body.Enabled = true;
                            inactiveCountPoly--;
                            break;
                        }
                    }
                }
            }
            return body;
        }

        public Body ActivateBodyCircle(float radius, BodyType bodyType, Vector2 position)
        {
            Body body = null;
            if (inactiveCountCircle == 0)
            {
                body = new Body(world);
                body.Position = position;
                body.BodyType = bodyType;
                CircleShape shape = new CircleShape(radius, 1f);
                body.CreateFixture(shape);
                bodyPoolCircle.Add(body);
            }
            else
            {
                for (int i = 0; i < bodyPoolCircle.Count; i++)
                {
                    if (!bodyPoolCircle[i].Enabled)
                    {
                        Body bodyTemp = bodyPoolCircle[i];
                        if (bodyTemp.FixtureList[0].ShapeType == ShapeType.Circle)
                        {
                            body = bodyTemp;
                            body.Position = position;
                            ((CircleShape)body.FixtureList[0].Shape).Radius = radius;
                            body.BodyType = bodyType;
                            // Must enable this *after* setting vertices
                            body.Enabled = true;
                            inactiveCountCircle--;
                            break;
                        }
                    }
                }
            }
            return body;
        }

        public void DeactivateBody(Body body)
        {
            Debug.Assert(body.Enabled);
            body.Enabled = false;
            if (body.FixtureList[0].ShapeType == ShapeType.Circle)
            {
                inactiveCountCircle++;
            }
            else
            {
                inactiveCountPoly++;
            }
        }
    }
}
