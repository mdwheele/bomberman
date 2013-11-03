using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman_ECS.Core
{
    // Data for a message, which can be serialized.
    // NOTE: When we implement serialization, we need to ensure we keep the order
    // of serialized objects. This is important for when we, for instance, set variables.
    public struct MessageData
    {
        public MessageData(Vector3 position)
        {
            xF = position.X;
            yF = position.Y;
            zF = position.Z;
            i = 0;
            this.i2 = 0;
            this.i3 = 0;
            b = false;
            handled = false;
        }

        public MessageData(Vector2 position)
        {
            xF = position.X;
            yF = position.Y;
            zF = 0;
            i = 0;
            this.i2 = 0;
            this.i3 = 0;
            b = false;
            handled = false;
        }

        public MessageData(int i)
        {
            xF = 0f;
            yF = 0f;
            zF = 0f;
            this.i = i;
            this.i2 = 0;
            this.i3 = 0;
            b = false;
            handled = false;
        }

        public MessageData(int i, float single)
        {
            xF = single;
            yF = 0f;
            zF = 0f;
            this.i = i;
            this.i2 = 0;
            this.i3 = 0;
            b = false;
            handled = false;
        }

        public MessageData(int i, float single, float single2)
        {
            zF = 0f;
            b = false;

            xF = single;
            yF = single2;
            this.i = i;
            this.i2 = 0;
            this.i3 = 0;
            handled = false;
        }

        public MessageData(bool b)
        {
            this.xF = 0f;
            this.yF = 0f;
            this.zF = 0f;
            this.i = 0;
            this.i2 = 0;
            this.i3 = 0;
            this.b = b;
            handled = false;
        }

        public MessageData(int i, bool b)
        {
            this.xF = 0f;
            this.yF = 0f;
            this.zF = 0f;

            this.i = i;
            this.i2 = 0;
            this.i3 = 0;
            this.b = b;
            handled = false;
        }

        public MessageData(int i, int i2)
        {
            this.xF = 0f;
            this.yF = 0f;
            this.zF = 0f;

            this.i = i;
            this.i2 = i2;
            this.i3 = 0;
            this.b = false;
            handled = false;
        }

        public MessageData(int i, int i2, int i3)
        {
            this.xF = 0f;
            this.yF = 0f;
            this.zF = 0f;

            this.i = i;
            this.i2 = i2;
            this.i3 = i3;
            this.b = false;
            handled = false;
        }


        // The data we hold (currently 17 bytes)
        private float xF;
        private float yF;
        private float zF;
        private int i;
        private int i2;
        private int i3;
        private bool b;

        public Vector3 Vector3 { get { return new Vector3(xF, yF, zF); } }

        public Vector2 Vector2 { get { return new Vector2(xF, yF); } }

        public float Single { get { return xF; } }

        public float Single2 { get { return yF; } }

        public int Int32 { get { return i; } }

        public int Int32Alt { get { return i2; } }

        public int Int32AltAlt { get { return i3; } }

        public bool Boolean { get { return b; } }

        private bool handled;
        public bool Handled
        {
            get { return handled; }
            set
            {
                if (value) // You can only set it to true
                {
                    handled = value;
                }
            }
        }

        // For messages that return a value.
        public void SetBooleanResponse(bool b)
        {
            this.b = b;
        }
        public void SetSingleResponse(float v)
        {
            this.xF = v;
        }

        public void SetIntResponse(int i)
        {
            this.i = i;
        }

        // Serialization. Right now we do it without compression.
        public void Deserialize(BinaryReader reader)
        {
            this.xF = reader.ReadSingle();
            this.yF = reader.ReadSingle();
            this.zF = reader.ReadSingle();
            this.b = reader.ReadBoolean();
            this.i = reader.ReadInt32();
            this.handled = false; // Since it was never processer
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(this.xF);
            writer.Write(this.yF);
            writer.Write(this.zF);
            writer.Write(this.b);
            writer.Write(this.i);
        }
    }
}
