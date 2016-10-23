using Gabriel.Cat.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public class Coleccion<T> : IColeccion<T>
    {

        IList<T> lst;
        T[] array; 
        public Coleccion(IList<T> lst)
        {
            if (lst == null) throw new ArgumentNullException();
            this.lst = lst;
        }
        public Coleccion(T[] array)
        {
            if (array == null) throw new ArgumentNullException();
            this.array = array;
        }
        public T this[int pos]
        {
            get
            {
                T value;
                if (array != null)
                    value = array[pos];
                else value = lst[pos];
                return value;
            }
            set
            {
                if (array != null)
                    array[pos]=value;
                else  lst[pos]=value;
            }
        }

        public int Count
        {
            get
            {
                int count;
                if (array != null)
                    count = array.Length;
                else count = lst.Count;
                return count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                bool isReadOnly;
                if (array != null)
                    isReadOnly = array.IsReadOnly;
                else isReadOnly = lst.IsReadOnly;
                return isReadOnly;
            }
        }

        void ICollection<T>.Add(T item)
        {
            if (array != null)
                throw new Exception("collection is readOnly");
            else lst.Add(item);
        }

        void ICollection<T>.Clear()
        {
            if (array != null)
                throw new Exception("collection is readOnly");
            else lst.Clear();
        }

        public bool Contains(T item)
        {
            bool contains;
            if (array != null)
                contains = array.Contains(item);
            else contains= lst.Contains(item);
            return contains;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array != null)
                array.CopyTo(array,arrayIndex);
            else lst.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> enumerator;
            if (array != null)
                enumerator = array.ObtieneEnumerador();
            else enumerator = lst.GetEnumerator();
            return enumerator;
        }

        bool ICollection<T>.Remove(T item)
        {
            bool removed;
            if (array != null)
                throw new Exception("collection is readOnly");
            else removed= lst.Remove(item);
            return removed;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator Coleccion<T>(List<T> lst)
        {
            return new Coleccion<T>(lst);
        }
        public static implicit operator Coleccion<T>(T[] array)
        {
            return new Coleccion<T>(array);
        }
    }

    public interface IColeccion<T>:ICollection<T>
    {
       T this[int pos]
        {
            get;
            set;
        }
    }
}
