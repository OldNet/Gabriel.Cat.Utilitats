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
using System.Collections;

namespace Gabriel.Cat
{
    /// <summary>
    /// Description of TwoKeysList.
    /// </summary>
    public class TwoKeysList<Tkey1, Tkey2, Tvalue> : IDictionary<TwoKeys<Tkey1, Tkey2>, Tvalue>, IEnumerable<KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue>>
                                                        where Tkey1 : IComparable<Tkey1>
                                                        where Tkey2 : IComparable<Tkey2>
    {

        LlistaOrdenada<Tkey1, Tvalue> llista1;
        LlistaOrdenada<Tkey2, Tvalue> llista2;
        LlistaOrdenada<Tkey2, Tkey1> llistaClau2;
        LlistaOrdenada<Tkey1, Tkey2> llistaClau1;
        public TwoKeysList()
        {
            llista1 = new LlistaOrdenada<Tkey1, Tvalue>();
            llista2 = new LlistaOrdenada<Tkey2, Tvalue>();
            llistaClau1 = new LlistaOrdenada<Tkey1, Tkey2>();
            llistaClau2 = new LlistaOrdenada<Tkey2, Tkey1>();

        }
        public int Count
        { get { return llistaClau1.Count; } }

        public ICollection<TwoKeys<Tkey1, Tkey2>> Keys
        {
            get
            {
                return KeysToArray();
            }
        }

        public ICollection<Tvalue> Values
        {
            get
            {
                return llista1.Values;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public Tvalue this[TwoKeys<Tkey1, Tkey2> key]
        {
            get
            {
                return llista1[key.Key1];
            }

            set
            {
                llista1[key.Key1] = value;
                llista2[llistaClau1[key.Key1]] = value;
            }
        }
        public Tvalue this[Tkey1 key1]
        {
            get { return llista1[key1]; }
            set { llista1[key1] = value; }
        }
        public Tvalue this[Tkey2 key2]
        {
            get { return llista2[key2]; }
            set { llista2[key2] = value; }
        }
        public void Add(Tkey1 key1, Tkey2 key2, Tvalue value)
        {
            if (llista1.ContainsKey(key1))
                throw new Exception("Esta duplicada la clave1 para el valor");
            if (llista2.ContainsKey(key2))
                throw new Exception("Esta duplicada la clave2 para el valor");
            llista1.Add(key1, value);
            llista2.Add(key2, value);
            llistaClau1.Add(key1, key2);
            llistaClau2.Add(key2, key1);
        }
        public bool Remove1(Tkey1 key1)
        {
            Tkey2 key2 = llistaClau1[key1];
            bool removed;
            llistaClau1.Remove(key1);
            llistaClau2.Remove(key2);
            removed = llista1.Remove(key1);
            llista2.Remove(key2);
            return removed;
        }
        public bool Remove2(Tkey2 key2)
        {
            Tkey1 key1 = llistaClau2[key2];
            bool removed;
            llistaClau1.Remove(key1);
            llistaClau2.Remove(key2);
            removed = llista1.Remove(key1);
            llista2.Remove(key2);
            return removed;
        }
        public Tvalue GetValueWithKey1(Tkey1 key)
        {
            return llista1[key];
        }
        public Tvalue GetValueWithKey2(Tkey2 key)
        {
            return llista2[key];
        }
        public Tkey1 GetTkey1WhithTkey2(Tkey2 key2)
        {
            return llistaClau2[key2];
        }
        public Tkey2 GetTkey2WhithTkey1(Tkey1 key1)
        {
            return llistaClau1[key1];
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
        
        public void Clear()
        {
            llistaClau1.Clear();
            llista2.Clear();
            llistaClau2.Clear();
            llista1.Clear();
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
        public KeyValuePair<Tkey1, Tvalue>[] Key1ValuePair()
        {
            return llista1.ToArray();
        }
        public KeyValuePair<Tkey2, Tvalue>[] Key2ValuePair()
        {
            return llista2.ToArray();
        }
        public TwoKeys<Tkey1, Tkey2>[] KeysToArray()
        {
            KeyValuePair<Tkey1, Tkey2>[] keys = llistaClau1.ToArray();
            TwoKeys<Tkey1, Tkey2>[] twoKeys = new TwoKeys<Tkey1, Tkey2>[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                twoKeys[i] = new TwoKeys<Tkey1, Tkey2>(keys[i].Key, keys[i].Value);
            return twoKeys;
        }
        public bool ContainsKey1(Tkey1 key1)
        {
            return llistaClau1.ContainsKey(key1);
        }
        public bool ContainsKey2(Tkey2 key2)
        {
            return llistaClau2.ContainsKey(key2);
        }
        public IEnumerator<KeyValuePair<TwoKeys<Tkey1,Tkey2>,Tvalue>> GetEnumerator()
        {
            foreach (KeyValuePair<Tkey1, Tkey2> keys in this.llistaClau1)
                yield return new KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue>(new TwoKeys<Tkey1, Tkey2>(keys.Key, keys.Value), llista1[keys.Key]);
        }
      

        public bool ContainsKey(TwoKeys<Tkey1, Tkey2> key)
        {
            return llistaClau1.ContainsKey(key.Key1) && llistaClau2.ContainsKey(key.Key2);
        }

        public void Add(TwoKeys<Tkey1, Tkey2> key, Tvalue value)
        {
            Add(key.Key1, key.Key2, value);
        }

        public bool Remove(TwoKeys<Tkey1, Tkey2> key)
        {
            return Remove1(key.Key1);
        }

        public bool TryGetValue(TwoKeys<Tkey1, Tkey2> key, out Tvalue value)
        {
            return llista1.TryGetValue(key.Key1, out value);
        }

        public void Add(KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue> item)
        {
            return llista1.ContainsKey(item.Key.Key1);
        }

        public void CopyTo(KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue>[] array, int arrayIndex)
        {
           
            this.WhileEach((item) => {
                array[arrayIndex++] = item;
                return array.Length == arrayIndex;
            });
        }

        public bool Remove(KeyValuePair<TwoKeys<Tkey1, Tkey2>, Tvalue> item)
        {
           return Remove1(item.Key.Key1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }




      
    }
    public struct TwoKeys<Tkey1, Tkey2>
    {
        Tkey1 key1;
        Tkey2 key2;
        public TwoKeys(Tkey1 key1, Tkey2 key2)
        {
            this.key1 = key1;
            this.key2 = key2;
        }
        public Tkey2 Key2
        {
            get
            {
                return key2;
            }
            set
            {
                key2 = value;
            }
        }
        public Tkey1 Key1
        {
            get
            {
                return key1;
            }
        }
    }
}
