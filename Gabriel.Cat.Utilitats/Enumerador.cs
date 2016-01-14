/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel.Cat
 * Fecha: 06/03/2015
 * Hora: 17:11
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gabriel.Cat
{
	/// <summary>
	/// Clase para heredar y hacer enumeraciones con tipos genericos
	/// </summary>
	public abstract class Enumerador<Tkey,Tvalue> where Tkey:IComparable
	{
		
		private readonly Tvalue _value;
		private readonly Tkey _displayName;
		protected Enumerador()
		{
		}
		protected Enumerador(Tkey displayName, Tvalue value)
		{
			_value = value;
			_displayName = displayName;

			
		}

		public Tvalue Value {
			get { return _value; }
		}

		public Tkey DisplayName {
			get { return _displayName; }
		}

		public override string ToString()
		{
			return DisplayName.ToString();
		}


		public override bool Equals(object obj)
		{
			Enumerador<Tkey,Tvalue> otherValue = obj as Enumerador<Tkey,Tvalue>;

			if (otherValue == null) {
				return false;
			}

			var typeMatches = GetType().Equals(obj.GetType());
			var valueMatches = _value.Equals(otherValue.Value);

			return typeMatches && valueMatches;
		}
		public static implicit operator Tkey(Enumerador<Tkey,Tvalue> enumeracion)
		{
			return enumeracion.DisplayName;
		}
		public static implicit operator Tvalue(Enumerador<Tkey,Tvalue> enumeracion)
		{
			return enumeracion.Value;
		}
		public static IEnumerable<T> GetAll<T>() where T :Enumerador<Tkey,Tvalue>, new()
		{
			var type = typeof(T);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

			foreach (var info in fields) {
				var instance = new T();
				var locatedValue = info.GetValue(instance) as T;

				if (locatedValue != null) {
					yield return locatedValue;
				}
			}
		}
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		//    public static int AbsoluteDifference(Enumerador<Tkey,Tvalue> firstValue, Enumerador<Tkey,Tvalue> secondValue)
		//    {
		//        var absoluteDifference = Math.Abs(firstValue.Value - secondValue.Value);
		//        return absoluteDifference;
		//    }

		public static T FromValue<T>(int value) where T : Enumerador<Tkey,Tvalue>, new()
		{
			var matchingItem = parse<T, int>(value, "value", item => item.Value.Equals(value));
			return matchingItem;
		}

		public static T FromDisplayName<T>(string displayName) where T : Enumerador<Tkey,Tvalue>, new()
		{
			var matchingItem = parse<T, string>(displayName, "display name", item => item.DisplayName.Equals(displayName));
			return matchingItem;
		}

		private static T parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumerador<Tkey,Tvalue>, new()
		{
			var matchingItem = Enumerador<Tkey,Tvalue>.GetAll<T>().FirstOrDefault(predicate);

			if (matchingItem == null) {
				var message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(T));
				throw new ApplicationException(message);
			}

			return matchingItem;
		}

		public int CompareTo(object other)
		{
			return DisplayName.CompareTo(((Enumerador<Tkey,Tvalue>)other).DisplayName);
		}

	}
	
	/// <summary>
	/// Clase para heredar y hacer enumeraciones string para key y int para value
	/// </summary>
	public abstract class Enumerador:Enumerador<string,int>
	{
		protected Enumerador(string key,int value):base(key,value){}
		protected Enumerador():base(){}
		public new  static  IEnumerable<T>  GetAll<T>() where T :Enumerador<string,int>, new()
		{
			return Enumerador<string,int>.GetAll<T>();
		}
	}
	/// <summary>
	/// Clase para heredar y hacer enumeraciones string para key y TValue el tipo que queramos
	/// </summary>
	public  abstract class Enumerador<TValue>:Enumerador<string,TValue>
	{
		protected Enumerador(string key,TValue value):base(key,value){}
		protected Enumerador():base(){}
		public new static IEnumerable<T> GetAll<T>() where T :Enumerador<string,TValue>, new()
		{
			return Enumerador<string,TValue>.GetAll<T>();
		}
	}
}
