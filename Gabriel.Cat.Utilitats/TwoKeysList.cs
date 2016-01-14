/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 07/04/2015
 * Hora: 16:48
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Gabriel.Cat
{
	/// <summary>
	/// Description of TwoKeysList.
	/// </summary>
	public class TwoKeysList<Tkey1,Tkey2,Tvalue>:IEnumerable<KeyValuePair<TwoKeys<Tkey1,Tkey2>,Tvalue>>
                                                        where Tkey1 : IComparable<Tkey1>
                                                        where Tkey2 : IComparable<Tkey2>
    {

		LlistaOrdenada<Tkey1,Tvalue> llista1;
		LlistaOrdenada<Tkey2,Tvalue> llista2;
		LlistaOrdenada<Tkey2,Tkey1> llistaClau2;
		LlistaOrdenada<Tkey1,Tkey2> llistaClau1;
		public TwoKeysList()
		{
			llista1 = new LlistaOrdenada<Tkey1, Tvalue>();
			llista2 = new LlistaOrdenada<Tkey2, Tvalue>();
			llistaClau1 = new LlistaOrdenada<Tkey1, Tkey2>();
			llistaClau2 = new LlistaOrdenada<Tkey2, Tkey1>();
		}
		public void Add(Tkey1 key1, Tkey2 key2, Tvalue value)
		{
			llista1.Afegir(key1, value);
			llista2.Afegir(key2, value);
			llistaClau1.Afegir(key1, key2);
			llistaClau2.Afegir(key2, key1);
		}
		public void Remove1(Tkey1 key1)
		{
			Tkey2 key2 = llistaClau1[key1];

				llistaClau1.Elimina(key1);
				llistaClau2.Elimina(key2);
				llista1.Elimina(key1);
				llista2.Elimina(key2);
			
		}
		public void Remove2(Tkey2 key2)
		{
			Tkey1 key1 = llistaClau2[key2];

				llistaClau1.Elimina(key1);
				llistaClau2.Elimina(key2);
				llista1.Elimina(key1);
				llista2.Elimina(key2);
			
		}
		public Tvalue ObtainValueWithKey1(Tkey1 key)
		{
			return llista1[key];
		}
		public Tvalue ObtainValueWithKey2(Tkey2 key)
		{
			return llista2[key];
		}
		public Tkey1 ObtainTkey1WhithTkey2(Tkey2 key2)
		{
			return llistaClau2[key2];
		}

        public void ChangeKey2(Tkey2 key2Old, Tkey2 key2New)
        {
            if (key2Old.CompareTo(key2New) != 0)
            {
                if (ContainsKey2(key2New))
                    throw new Exception("new key2 is already in use");
                Tkey1 key1 = llistaClau2[key2Old];
                Tvalue value = llista2[key2Old];
                Remove2(key2Old);
                Add(key1, key2New, value);
            }
        }
        public void ChangeKey1(Tkey1 key1Old, Tkey1 key1New)
        {
            if (key1Old.CompareTo(key1New) != 0)
            {
                if (ContainsKey1(key1New))
                    throw new Exception("new key1 is already in use");
                Tkey2 key2 = llistaClau1[key1Old];
                Tvalue value = llista1[key1Old];
                Remove1(key1Old);
                Add(key1New, key2, value);
            }
        }
        public Tkey2 ObtainTkey2WhithTkey1(Tkey1 key1)
		{
			return llistaClau1[key1]; 
		}
		public void Clear()
		{
			llistaClau1.Buida();
			llista2.Buida();
			llistaClau2.Buida();
			llista1.Buida();
		}
		public Tvalue this[Tkey1 key1] {
			get{ return llista1[key1]; }
			set{ llista1[key1] = value; }
		}
		public Tvalue this[Tkey2 key2] {
			get{ return llista2[key2]; }
			set{ llista2[key2] = value; }
		}
		public Tvalue[] ValueToArray()
		{
			return llista1.GetValues().ToTaula();
			;
		}
		public Tkey1[] Key1ToArray()
		{
			return llistaClau1.GetKeys().ToTaula();
		}
		public Tkey2[] Key2ToArray()
		{
			return llistaClau2.GetKeys().ToTaula();
		}
		public KeyValuePair<Tkey1,Tvalue>[] Key1ValuePair()
		{
			return llista1.ToArray();
		}
		public KeyValuePair<Tkey2,Tvalue>[] Key2ValuePair()
		{
			return llista2.ToArray();
		}
		public KeyValuePair<Tkey1,Tkey2>[] KeysToArray()
		{
			return llistaClau1.ToArray();
		}
		public bool ContainsKey1(Tkey1 key1)
		{
			return llistaClau1.Existeix(key1);
		}
		public bool ContainsKey2(Tkey2 key2)
		{
			return llistaClau2.Existeix(key2);
		}
		public int Count
		{ get { return llistaClau1.Count; } }

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue>> GetEnumerator()
		{
			var keys = llistaClau1.ToArray();
			Tvalue[] values = ValueToArray();
			for (int i = 0; i < values.Length; i++)
				yield return new KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue>(new TwoKeys<Tkey1,Tkey2>(keys[i].Key, keys[i].Value), values[i]);
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
            return GetEnumerator();
		}

		#endregion
	}
	public struct TwoKeys<Tkey1,Tkey2>
	{
		Tkey1 key1;
		Tkey2 key2;
		public TwoKeys(Tkey1 key1, Tkey2 key2)
		{
			this.key1 = key1;
			this.key2 = key2;
		}
		public Tkey2 Key2 {
			get {
				return key2;
			}
			set {
				key2 = value;
			}
		}
		public Tkey1 Key1 {
			get {
				return key1;
			}
		}
	}
}
