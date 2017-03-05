using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gabriel.Cat
{
	/// <summary>
	/// Clase tipus que serveix per millorar String al utilitzar StringBuilder :)
	/// </summary>
	public  class text :ObjectAutoId, IEnumerable<char>,IComparable,IComparable<text>,IClauUnicaPerObjecte
	{
		StringBuilder tString;
		private text()
		{
			tString = new StringBuilder();

		}

		public long LongLenght {
			get{ return tString.ToString().LongCount(); }
		}
		public override bool Equals(object obj)
		{
			text text = obj as text;
            bool equals = text != null;
			if (equals)
				equals= text.tString.Equals(this.tString);
            return equals;
		}
		#region metodes
        public void Append(text tString)
        {
            this.tString.Append(tString.tString);
        }
		public string[] Split(char caracterSplit)
		{
			string text = ToString();
			return text.Split(caracterSplit);
		}
		public text Remove(int startIndex, int length)
		{
            text tRemoved = this;
            tRemoved.tString.Remove(startIndex, length);
            return tRemoved;

        }
		public text Remove(string textToRemove)
		{
            text tRemoved = this;
            tRemoved.Replace(textToRemove, "");
            return tRemoved;
        }
		public text Replace(string oldString, string newString)
		{
            text tReplaced = this;
         tReplaced.tString.Replace(oldString, newString);
            return this;
        }
		public text Replace(string[,] oldNewStrings)
		{
            text tReplaced = this;
            for (int y = 0; y < oldNewStrings.GetLength(DimensionMatriz.Fila); y++)
                tReplaced.Replace(oldNewStrings[0,y], oldNewStrings[1,y]);
            return tReplaced;
			
		}
		public text Replace(char oldChar, char newChar)
		{
            text tReplaced = this;
            tReplaced.tString.Replace(oldChar, newChar);
            return tReplaced;
		}
		public text Replace(string oldString, string newString, int startIndex, int length)
		{
            text tReplaced = this;
            tReplaced.tString.Replace(oldString, newString, startIndex, length);
            return tReplaced;
		}
		public text Replace(char oldChar, char newChar, int startIndex, int length)
		{
            text tReplaced = this;
            tReplaced.tString.Replace(oldChar, newChar, startIndex, length);
            return tReplaced;

		}


		/// <summary>
		/// Si el prefix ja existeix no el posa
		/// </summary>
		/// <param name="caracterInicio"></param>
		/// <param name="prefix"></param>
		public void Prefix(char caracterInicio, string prefix)
		{
			int pos = 0;
			int posInicioPrefix = -1;
			//cuando encuentra el caracter inicio pone la posicion siguiente como la de inicio del prefijo
			//mira si esta el prefijo ya puesto...si esta lo salta...
			while (pos < tString.Length) {
				if (tString[pos] == caracterInicio)
					posInicioPrefix = pos + 1;
				pos++;
				if (posInicioPrefix != -1) {
					if (prefix.Length <= posInicioPrefix - pos && prefix[posInicioPrefix - pos] != tString[pos]) {
						tString.Insert(posInicioPrefix, prefix);
						posInicioPrefix = -1;
					}
				}
				
			}
		}

		public static int CountSubString(text textComplet, text subString)
		{
			int contador = 0;
			int pos = 0;
			text stringT = "";
			for (int i = 0, textCompletCount = textComplet.Count; i < textCompletCount; i++) {
				char caracter = textComplet[i];
				if (pos < subString.Length)
				if (subString[pos] == caracter) {
					stringT += caracter;
					pos++;
				} else {
					pos = 0;
					stringT = "";
				}
				if (stringT.Equals(subString)) {
					contador++;
					stringT = "";
					pos = 0;
				}
			}
			return contador;
		}
		public text SubString(text stringComençament, text stringFi)
		{
			text textS = "";
			text iniciFi = "";
			bool trobatInici = false;
			bool trobatFi = false;
			int pos = 0;
            char caracter;

            for (int i = 0, maxCount = this.Count; i < maxCount; i++) {
				 caracter = this[i];
				if (!trobatInici) {
					if (pos < stringComençament.Length) {
						if (stringComençament[pos] == caracter) {
							iniciFi += caracter;
							pos++;
						} else {
							pos = 0;
							iniciFi = "";
						}
					}
					trobatInici = stringComençament.Equals(iniciFi);
					if (trobatInici) {
						textS += stringComençament;
						iniciFi = "";
						pos = 0;
					}
				} else if (!trobatFi) {
					textS += caracter;
					if (pos < stringFi.Length) {
						if (stringFi[pos] == caracter) {
							iniciFi += caracter;
							pos++;
						} else {
							pos = 0;
							iniciFi = "";
						}
					}
					trobatFi = stringFi.Equals(iniciFi);
				}
			}
			return textS;


		}
		public void TreuAparicionsSubString(text text)
		{
			this.Replace(text, "");
		}

		public int CountSubString(text subString)
		{
			return CountSubString(this, subString);
		}
		public char this[int posicio] {
			get { return tString[posicio]; }
			set { tString[posicio] = value; }
		}
		public int Count {
			get { return tString.Length; }
		}
		public int Length {
			get { return tString.Length; }
		}
		#endregion
		#region Operadors

		public static text operator &(text text1, text text2)
		{
            //((text)"")&variables;//al poner el centinela no se modifica ninguna variable text :) y el resultado de la expresion es una variable text
            //esta hecho para evitar el StringBuilder y su incomodo Append
            if (text2 != null)
                text1.tString.Append(text2.tString);

			return text1;

		}

		public static text operator &(text text1, object text2)
		{
            return text1&(text2 as text);
        }
        public static text operator +(text text1, text text2)
        {
            //se tiene que crear porque sino en una suma con muchos text se modificarian...y luego habrian problemas...
            text tResult = "";
            if (text1 != null)
                tResult.tString.Append(text1.tString);
                if (text2 != null)
                tResult.tString.Append(text2.tString);

            return tResult;

        }

        public static text operator +(text text1, object text2)
        {
            text tRigth =text2!=null? text2.ToString():null;
            return text1+tRigth;
        }
        public static bool operator ==(text text1, text text2)
		{
            return Equals(text1, text2);
		}
		public static bool operator !=(text text1, text text2)
		{
			return !(text1 == text2);
		}

		#endregion
		#region Conversions
		public static implicit operator text(int num)
		{
			text t = new text();
			t.tString.Append(num);
			return t;
		}
		public static implicit operator text(long num)
		{
			text t = new text();
			t.tString.Append(num);
			return t;
		}
		public static implicit operator text(float num)
		{
			text t = new text();
			t.tString.Append(num);
			return t;
		}
		public static implicit operator text(double num)
		{
			text t = new text();
			t.tString.Append(num);
			return t;
		}
		public static implicit operator text(string text)
		{
			text t = new text();
			t.tString.Append(text);
			return t;
		}
		public static implicit operator text(char caracter)
		{
			text t = new text();
			t.tString.Append(caracter);
			return t;
		}
		public static implicit operator text(byte[] bin)
		{
			text t = new text();
            byte b;
            for (int i = 0, binLength = bin.Length; i < binLength; i++) {
				b= bin[i];
				t.tString.Append(b.ToString());
			}

			return t;
		}
		public static implicit operator byte[](text bin)
		{
			
			byte[] bytes = new byte[bin.Length - 1];
			int pos = 0;
            char c;

            for (int i = 0, binCount = bin.Count; i < binCount; i++) {
				c = bin[i];
				bytes[pos++] = Byte.Parse(c + "");
			}
			return bytes;
		}

		public static implicit operator string(text text)
		{
			return text.tString.ToString();
		}
		public override string ToString()
		{
			return tString.ToString();
		}
		public void QuitarPlantilla(string plantilla, string sustituto)
		{

			int fin = 0;
			//cuando encuentra el sustituto es el inicio y cuando acaba lo que queda es el resto...
			int posicion = 0;
			int pos = 0;
			text stringT = "";
			
			for (int i = 0, plantillaCount = plantilla.Length; i < plantillaCount && fin == 0; i++) {
				char caracter = plantilla[i];
				if (pos < sustituto.Length)
				if (sustituto[pos] == caracter) {
					stringT += caracter;
					pos++;
				} else {
					pos = 0;
					stringT = "";
				}
				if (stringT.ToString().Equals(sustituto)) {
					fin = posicion;
				}
				posicion++;
			}
			if (fin != 0) {
				
				
				stringT = ToString().Substring(fin - sustituto.Length + 1, ToString().Length - fin - 2);
				this.tString.Clear();
				this.tString.Append(stringT);
			}

			
		}
		#endregion
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public IEnumerator<char> GetEnumerator()
		{
			return tString.ToString().GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int CompareTo(object obj)
		{
            return CompareTo(obj as text);
		}

        public IComparable Clau
        {
           get
            {
                return this;
            }
        }

        public int CompareTo(text other)
        {
            int compareTo;
            if (other != null)
                compareTo = tString.ToString().CompareTo(other.ToString());
            else
                compareTo = -1;
            return compareTo;
        }
    }
}
