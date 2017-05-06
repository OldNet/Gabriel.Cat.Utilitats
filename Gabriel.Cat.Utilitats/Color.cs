/*
 * Created by SharpDevelop.
 * User: tetra
 * Date: 06/05/2017
 * Time: 5:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace Gabriel.Cat.V2
{
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	/// <summary>
	/// Description of Color.
	/// </summary>
	public struct Color:IComparable
	{
		[FieldOffset(0)]
		byte a;
		[FieldOffset(1)]
		byte r;
		[FieldOffset(2)]
		byte g;
		[FieldOffset(3)]
		byte b;
		
		public Color(int argb=0)
		{
			byte[] argbBytes=Serializar.GetBytes(argb);
			a=argbBytes[0];
			r=argbBytes[1];
			g=argbBytes[2];
			b=argbBytes[3];
			
		}

		public byte Alfa {
			get {
				return a;
			}
			set {
				a = value;
			}
		}

		public byte Red {
			get {
				return r;
			}
			set {
				r = value;
			}
		}

		public byte Green {
			get {
				return g;
			}
			set {
				g = value;
			}
		}

		public byte Blue {
			get {
				return b;
			}
			set {
				b = value;
			}
		}
		public int ToArgb()
		{
			return Serializar.ToInt(new byte[]{a,r,g,b});
		}

		#region IComparable implementation

		public int CompareTo(object obj)
		{
			Color other;
			int compareTo;
			try{
				other=(Color)obj;
				compareTo=ToArgb().CompareTo(other.ToArgb());
			}catch{
				compareTo=(int)Gabriel.Cat.CompareTo.Inferior;
				
			}
			return compareTo;
		}
		#endregion
		public static implicit operator System.Drawing.Color(Color color)
		{
			return System.Drawing.Color.FromArgb(color.ToArgb());
		}
	}

	
}

