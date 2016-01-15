using System;
using System.Drawing;
using System.Linq;
namespace Gabriel.Cat
{
	public class PointZ : IComparable<PointZ>,IComparable
	{
		int x;

		int y;

		int z;

        public PointZ(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		public PointZ()
			: this(0, 0, 0)
		{
		}

        public PointZ(Point point, int z):this(point.X,point.Y,z)
        {
        }

        public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		public int Z {
			get {
				return z;
			}
			set {
				z = value;
			}
		}

		#region IComparable implementation
		public int CompareTo(PointZ other)
		{
			int compareTo = -1;
			if (other != null) {
				compareTo = Z.CompareTo(other.Z);
				if (compareTo == 0) {
					if (X < other.X)
						compareTo = 1;
					else if (X > other.X)
						compareTo = -1;
					else
						compareTo = Y.CompareTo(other.Y);
				} else
					compareTo = Z.CompareTo(other.Z)*-1;
			}
			return compareTo;
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as PointZ);
		}

		#endregion
	}
}


