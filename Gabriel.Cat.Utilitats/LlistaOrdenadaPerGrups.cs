using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{
	public class LlistaOrdenadaPerGrups<TKey,TValue>:ObjectAutoId,IEnumerable<KeyValuePair<TKey,TValue[]>>
        where TKey :IComparable
        where TValue:IComparable
    {
        LlistaOrdenada<TKey, LlistaOrdenada<IComparable,TValue>> diccionari;
        public LlistaOrdenadaPerGrups()
        {
            diccionari = new LlistaOrdenada<TKey, LlistaOrdenada<IComparable,TValue>>();
        }
        public TValue[] this[TKey key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException();
                return (TValue[])diccionari[key].Values;
            }
        }
        public int Count()
        {
        	int total=0;
        	foreach(var item in diccionari)
        		total+=item.Value.Count;
        	return total;
        }
        public int Count(TKey key)
        {
            return diccionari[key].Count;
        }
        public void Add(TKey key, IList<TValue> values)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (values == null)
                throw new ArgumentNullException("values");
            for (int i = 0; i < values.Count; i++)
            {
                try
                {
                    Add(key, values[i]);
                }
                catch { }//si ya esta añadido no pasa nada :D
                }
        }
        public void Add(TKey key,TValue value)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (value == null)
                throw new ArgumentNullException();
            if (!diccionari.ContainsKey(key))
                diccionari.Add(key, new LlistaOrdenada<IComparable, TValue>());
            diccionari[key].Add(value, value);
        }
        public void Remove(TKey key,IList<TValue>values)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (values == null)
                throw new ArgumentNullException("values");
            for (int i = 0; i < values.Count; i++)
                if(values[i]!=null)
                Remove(key, values[i]);
        }
        public void Remove(TKey key,TValue value)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (value == null)
                throw new ArgumentNullException();

            if (diccionari.ContainsKey(key))
            {
                if(diccionari[key].ContainsKey(value))
                {
                    diccionari[key].Remove(value);
                    if (diccionari[key].Count == 0)
                        diccionari.Remove(key);
                }
            }
        }
        public void Remove(IList<TKey> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");
            for (int i = 0; i < keys.Count; i++)
                if(keys[i]!=null)
                   Remove(keys[i]);
        }
        public void Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException();
            diccionari.Remove(key);
        }
        public void Clear()
        {
            diccionari.Clear();
        }
        public void ClearValues(TKey key)
        {
            if (ContainsKey(key))
                diccionari[key].Clear();
        }
        public bool ContainsKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException();
            return diccionari.ContainsKey(key);
        }
        public bool ContainValues(TKey key)
        {
            return !ContainsKey(key) ? false : diccionari[key].Count != 0;
        }
        public bool ContainsValue(TKey key,TValue value)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (value == null)
                throw new ArgumentNullException();
            return ContainsKey(key) ? diccionari[key].ContainsKey(value) : false;
        }
        public bool ContainsValue(TValue value)
        {
            if (value == null)
                throw new ArgumentNullException();
            bool encontrado = false;
            for (int i = 0; i < diccionari.Count && !encontrado; i++)
                encontrado = diccionari.GetValueAt(i).ContainsKey(value);
            return encontrado;
        }
        public TValue GetValueAt(TKey key,int index)
        {
            return diccionari[key].GetValueAt(index);
        }

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<TKey,TValue[]>> GetEnumerator()
		{
			foreach(var item in diccionari)
				yield return new KeyValuePair<TKey,TValue[]>(item.Key,item.Value.ValuesToArray());
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
    }
}
