/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel
 * Fecha: 13/07/2015
 * Hora: 17:39
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;

namespace Gabriel.Cat
{
    /// <summary>
    /// Description of Hex.Por acabar...se tienen que hacer las operaciones...son muy chungas...
    /// de momento se limita a convertir y operar...en el futuro no se convierte asi pueden ser numero gigantes
    /// </summary>
    public struct Hex : IEquatable<Hex>
    {
        static LlistaOrdenada<char, char> diccionarioCaracteresValidos;
        public static readonly string[] caracteresHex = new string[]{
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "a",
                "b",
                "c",
                "d",
                "e",
                "f",
                "A",
                "B",
                "C",
                "D",
                "E",
                "F"
            };
        string numberHex;
        static Hex()
        {
            diccionarioCaracteresValidos = new LlistaOrdenada<char, char>();
            for (int i = 0; i < caracteresHex.Length; i++)
                diccionarioCaracteresValidos.Afegir(caracteresHex[i][0], caracteresHex[i][0]);
        }


        public Hex(string numero)
        {
            if (!ValidaString(numero))
                throw new Exception("la string contiene caracteres no HEX");
            this.numberHex = numero;
        }
        // this is just an example member, replace it with your own struct members!
        public string Number
        {
            get
            {
                return numberHex;
            }
            private set
            {
                numberHex = value;
            }
        }
        /// <summary>
        /// Concatena 0x al numero
        /// </summary>
        public string ByteString
        {
            get
            {
                return "0x" + Number;
            }
        }
        #region Equals and GetHashCode implementation
        // The code in this region is useful if you want to use this structure in collections.
        // If you don't need it, you can just remove the region and the ": IEquatable<Hex>" declaration.

        public override bool Equals(object obj)
        {
            if (obj is Hex)
                return Equals((Hex)obj); // use Equals method below
            else
                return false;
        }

        public bool Equals(Hex other)
        {
            // add comparisions for all members here
            return this.numberHex == other.numberHex;
        }
        public override string ToString()
        {
            return numberHex;
        }

        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return numberHex.GetHashCode();
        }

        public static bool operator ==(Hex left, Hex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Hex left, Hex right)
        {
            return !left.Equals(right);
        }
        public static Hex operator +(Hex left, Hex right)
        {
            long result = left;
            result += (long)right;
            return result;
        }
        public static Hex operator -(Hex left, Hex right)
        {
            long result = left;
            result -= (long)right;
            return result;
        }
        public static Hex operator ++(Hex number)
        {
            long result = number + 1;

            return result;
        }
        char SigienteCaracter(char caracter)
        {
            char caracterNext = '0';
            switch (caracter)
            {
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                    caracterNext = (char)(((int)caracter) + 1);
                    break;
                case 'f':
                    caracterNext = '0';
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                    caracterNext = (Convert.ToInt32(caracter) + 1).ToString()[0];
                    break;
                case '9':
                    caracterNext = 'a';
                    break;

            }
            return caracterNext;
        }
        public static Hex operator --(Hex number)
        {
            long result = number - 1;
            return result;
        }
        char AnteriorCaracter(char caracter)
        {
            char caracterNext = '0';
            switch (caracter)
            {

                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                    caracterNext = (char)(((int)caracter) - 1);
                    break;
                case 'a':
                    caracterNext = '9';
                    break;


                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    caracterNext = (Convert.ToInt32(caracter) - 1).ToString()[0];
                    break;
                case '0':
                    caracterNext = 'f';
                    break;

            }
            return caracterNext;
        }
        public static Hex operator *(Hex left, Hex right)
        {
            long result = left;
            result *= (long)right;
            return result;
        }
        public static Hex operator /(Hex left, Hex right)
        {
            long result = left;
            result /= (long)right;
            return result;
        }
        #endregion

        public static implicit operator Hex(string numero)
        {
            return new Hex(numero);
        }
        public static implicit operator Hex(int numero)
        {
            return new Hex(QuitaCerosInutiles(numero.ToString("X2")));
        }
        public static implicit operator Hex(byte numero)
        {
            return (int)numero;
        }
        public static explicit operator Hex(byte[] numero)
        {
            string numHex = "";
            for (int i = 0; i < numero.Length; i++)
                numHex += ((Hex)numero[i]).Number.PadLeft(2,'0');
            return (Hex)numHex;
        }
        public static implicit operator Hex(uint numero)
        {
            return (Hex)Convert.ToInt64(numero);
        }

        public static implicit operator Hex(long numero)
        {
            return new Hex(QuitaCerosInutiles(numero.ToString("X4")));
        }

        static string QuitaCerosInutiles(string numero)
        {
            text num = numero;
            while (num.Count > 0 && num[0] == '0') num.Remove(0, 1);
            if (num.Count == 0)
                num = "0";
            return num;
        }
        public static implicit operator string(Hex numero)
        {
            return numero.numberHex;
        }
        public static implicit operator int(Hex numero)
        {
            return Convert.ToInt32((string)numero.numberHex, 16);
        }
        public static implicit operator uint(Hex numero)
        {
            return Convert.ToUInt32((long)numero);
        }
        public static implicit operator byte(Hex numero)
        {
            return Convert.ToByte((int)numero);
        }
        public static implicit operator long(Hex numero)
        {
            return Convert.ToInt64((string)numero.numberHex, 16);
        }
        public static bool ValidaString(string stringHex)
        {
            bool valida = true;
            stringHex.WhileEach((caracterAComprovar) =>
            {
                valida = diccionarioCaracteresValidos.Existeix(caracterAComprovar);
                return valida;
            });
            return valida;

        }
    }
}
