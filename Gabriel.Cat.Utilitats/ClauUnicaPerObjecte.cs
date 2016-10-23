using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public interface IClauUnicaPerObjecte
    {
        IComparable Clau { get; }
    }
    /// <summary>
    /// Sirve para usar clases y structs que son IComparable pero que no son IClauUnicaPerObjecte, hay conversiones implicitas a los tipos basicos
    /// </summary>
    public class ClauUnicaPerObjecte : IClauUnicaPerObjecte
    {
        IComparable clau;
        public ClauUnicaPerObjecte(IComparable clau)
        { this.clau = clau; }
        public IComparable Clau
        {
            get
            {
                return clau;
            }
        }
        #region conversion
        public static implicit operator ClauUnicaPerObjecte(char clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(string clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(double clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(float clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(bool clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(int clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }


        public static implicit operator ClauUnicaPerObjecte(short clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(long clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(ulong clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(uint clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }
        public static implicit operator ClauUnicaPerObjecte(ushort clau)
        {
            return new ClauUnicaPerObjecte(clau);
        }


        #endregion

    }
}
