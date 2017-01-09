using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Gabriel.Cat
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct PointZ : IComparable<PointZ>
    {
        [FieldOffset(0)]
        int x;
        [FieldOffset(4)]
        int y;
        [FieldOffset(8)]
        int z;

        public PointZ(int x=0, int y=0, int z=0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public PointZ(Point point, int z=0) : this(point.X, point.Y, z)
        {

        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public int Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
            }
        }

        #region IComparable implementation
        public int CompareTo(PointZ other)
        {
            int compareTo = -1;

            compareTo = Z.CompareTo(other.Z);
            if (compareTo == 0)
            {
                if (X < other.X)
                    compareTo = 1;
                else if (X > other.X)
                    compareTo = -1;
                else
                    compareTo = Y.CompareTo(other.Y);
            }
            else
                compareTo = Z.CompareTo(other.Z) * -1;

            return compareTo;
        }


        #endregion
    }
}


