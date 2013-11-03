using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Bomberman_ECS.Core
{
    [StructLayout(LayoutKind.Explicit)]
    struct Int32SingleUnion
    {
        /// <summary>
        /// Int32 version of the value.
        /// </summary>
        [FieldOffset(0)]
        int i;
        /// <summary>
        /// Single version of the value.
        /// </summary>
        [FieldOffset(0)]
        float f;

        /// <summary>
        /// Creates an instance representing the given integer.
        /// </summary>
        /// <param name="i">The integer value of the new instance./param>
        internal Int32SingleUnion(int i)
        {
            this.f = 0; // Just to keep the compiler happy
            this.i = i;
        }

        /// <summary>
        /// Creates an instance representing the given floating point
        /// number.
        /// </summary>
        /// <param name="f">The floating point value of the new instance
        /// </param>
        internal Int32SingleUnion(float f)
        {
            this.i = 0; // Just to keep the compiler happy
            this.f = f;
        }

        /// <summary>
        /// Returns the value of the instance as an integer.
        /// </summary>
        internal int AsInt32
        {
            get { return i; }
        }

        /// <summary>
        /// Returns the value of the instance as a floating point number.
        /// </summary>
        internal float AsSingle
        {
            get { return f; }
        }
    }
}
