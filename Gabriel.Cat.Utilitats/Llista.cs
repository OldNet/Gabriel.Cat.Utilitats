/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 11/12/2014
 * Hora: 21:00
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gabriel.Cat
{

    /// <summary>
    /// Description of Llista.
    /// </summary>
    public class Llista<TValue> : IList<TValue>, IReadOnlyList<TValue>
    {
        List<TValue> llista;
        Semaphore semafor;
        public bool Whaiting { get; private set; }
        LlistaOrdenada<IComparable, TValue> llistaOrdenada;//sirve para facilitar el encontrar los valores para que sea lo mas ágil posible
        /// <summary>
        /// Ocurre cuando la lista es modificada ya sea en el numero de elementos o en el orden
        /// </summary>
        public event EventHandler Updated;
        public event EventHandler Added;
        public event EventHandler Removed;
        ConfirmationEventHandler<Llista<TValue>,TValue> AddConfirmation;
        ConfirmationEventHandler<Llista<TValue>, IEnumerable<TValue>> AddRangeConfirmation;
        ConfirmationEventHandler<Llista<TValue>,TValue> RemoveConfirmation;
        ConfirmationEventHandler<Llista<TValue>, IEnumerable<TValue>> ReomveRangeConfirmation;
        ConfirmacionEventHandler<Llista<TValue>> ClearConfirmation;
        ConfirmacionEventHandler<Llista<TValue>> UpdateConfirmation;
        public Llista()
        {
            Whaiting = false;
            llista = new List<TValue>();
            if (typeof(TValue).GetInterface("IClauUnicaPerObjecte") != null)
                llistaOrdenada = new LlistaOrdenada<IComparable, TValue>();
            semafor = new Semaphore(1, 1);
        }
        public Llista(ConfirmationEventHandler<Llista<TValue>, TValue> metodoAddConfirmation, ConfirmationEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoAddRangeConfirmation, ConfirmationEventHandler<Llista<TValue>, TValue> metodoRemoveConfirmation, ConfirmationEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoReomveRangeConfirmation, ConfirmacionEventHandler<Llista<TValue>> metodoUpdateConfirmation, ConfirmacionEventHandler<Llista<TValue>> metodoClearConfirmation) : this()
        {
            UpdateConfirmation = metodoUpdateConfirmation;
            AddConfirmation = metodoAddConfirmation;
            AddRangeConfirmation = metodoAddRangeConfirmation;
            RemoveConfirmation = metodoRemoveConfirmation;
            ReomveRangeConfirmation = metodoReomveRangeConfirmation;
            ClearConfirmation = metodoClearConfirmation;
        }
        public Llista(IEnumerable<TValue> valors)
            : this()
        {
            AddRange(valors);
        }
        public Llista(IEnumerable<TValue> valors,ConfirmationEventHandler<Llista<TValue>, TValue> metodoAddConfirmation, ConfirmationEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoAddRangeConfirmation, ConfirmationEventHandler<Llista<TValue>, TValue> metodoRemoveConfirmation, ConfirmationEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoReomveRangeConfirmation, ConfirmacionEventHandler<Llista<TValue>> metodoUpdateConfirmation, ConfirmacionEventHandler<Llista<TValue>> metodoClearConfirmation) :this(metodoAddConfirmation,metodoAddRangeConfirmation,metodoRemoveConfirmation,metodoReomveRangeConfirmation,metodoUpdateConfirmation,metodoClearConfirmation)
        {
            AddRange(valors);
        }
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        public int Count
        {
            get
            {
                int count;
                semafor.WaitOne();
                Whaiting = true;
                count = llista.Count;
                semafor.Release();
                Whaiting = false;
                return count;
            }
        }
        public TValue this[int pos]
        {
            get
            {
                TValue value = default(TValue);
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    value = llista[pos];
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    try
                    {
                        semafor.Release();
                    }
                    finally
                    {
                        Whaiting = false;

                    }
                }
                return value;

            }
            set
            {

                if (UpdateConfirmation == null || UpdateConfirmation(this))
                    try
                    {
                        semafor.WaitOne();
                        Whaiting = true;
                        llista[pos] = value;
                    }
                    finally
                    {
                        try
                        {
                            semafor.Release();
                        }
                        finally
                        {
                            Whaiting = false;
                            if (Updated != null)
                                Updated(this, new EventArgs());
                        }
                    }
            }
        }
        public void Add(TValue value)
        {

            if(AddConfirmation==null||AddConfirmation(this,value))
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.Add(value);
                if (llistaOrdenada != null)
                    llistaOrdenada.AddOrReplace(((IClauUnicaPerObjecte)value).Clau(), value);
            }
            finally
            {
                semafor.Release();
                Whaiting=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Added != null)
                    Added(this, new EventArgs());
            }
        }
        public void AddRange(IEnumerable<TValue> values)
        {
            if(values!=null)
            if (AddRangeConfirmation == null || AddRangeConfirmation(this, values))
                try
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.AddRange(values);
                if (llistaOrdenada != null)
                {
                    foreach (TValue value in values)
                        try
                        {
                            llistaOrdenada.AddOrReplace(((IClauUnicaPerObjecte)value).Clau(), value);
                        }
                        catch { }
                }
            }
            finally
            {
                semafor.Release();
                Whaiting=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Added != null)
                    Added(this, new EventArgs());
            }
        }
        public bool Remove(TValue value)
        {
            bool removeDone=false;
            if (RemoveConfirmation == null || RemoveConfirmation(this, value))
                try
            {
                semafor.WaitOne();
                Whaiting = true;
                if (llistaOrdenada != null)
                    llistaOrdenada.Remove((value as IClauUnicaPerObjecte).Clau());
                    removeDone=llista.Remove(value);
            }
            finally
            {
                semafor.Release();
                Whaiting=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());

            }
            return removeDone;
        }
        public bool[] RemoveRange(IEnumerable<TValue> values)
        {
            List<bool> result = new List<bool>();
            if (values != null)
                if (ReomveRangeConfirmation == null || ReomveRangeConfirmation(this, values))
                    try
                    {
                        semafor.WaitOne();
                        Whaiting = true;
                        if (llistaOrdenada != null)
                        {
                            foreach (TValue value in values)
                            {
                                llistaOrdenada.Remove((value as IClauUnicaPerObjecte).Clau());
                                result.Add(llista.Remove(value));
                            }
                        }
                        else
                        {
                            foreach (TValue value in values)
                                result.Add(llista.Remove(value));
                        }
                    }
                    finally
                    {
                        semafor.Release();
                        Whaiting = false;
                        if (Updated != null)
                            Updated(this, new EventArgs());
                        if (Removed != null)
                            Removed(this, new EventArgs());
                    }
            return result.ToArray();
        }
        public void RemoveAt(int pos)
        {
            if (RemoveConfirmation == null || RemoveConfirmation(this,this[pos]))
                try
            {
                semafor.WaitOne();
                Whaiting = true;
                if (llistaOrdenada != null)
                    llistaOrdenada.Remove((llista[pos] as IClauUnicaPerObjecte).Clau());
                llista.RemoveAt(pos);
            }
            finally
            {
                semafor.Release();
                Whaiting=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());
            }
        }
        public TValue FirstOrDefault(IComparable keyToFindItem) 
        {
            TValue value;
            semafor.WaitOne();
            Whaiting = true;
            if (llistaOrdenada == null)
            {
                value = llista.FirstOrDefault((item) =>
                {
                    return (object)keyToFindItem == (object)item;
                });
            }else
            {
                value = llistaOrdenada[keyToFindItem];
            }
            semafor.Release();
            Whaiting = false;
            return value;
        }
        public int IndexOf(TValue value)
        {
            int index;
            semafor.WaitOne();
            Whaiting = true;
            index = llista.IndexOf(value);
            Whaiting = false;
            semafor.Release();
            return index;
        }
        public bool Contains(TValue value)
        {
            bool existeix = false;

            try
            {
                semafor.WaitOne();
                Whaiting = true;
                if (llistaOrdenada != null)
                    existeix = llistaOrdenada.ContainsKey(((IClauUnicaPerObjecte)value).Clau());
                else
                    existeix = llista.Contains(value);
            }
            finally
            {
                try
                {
                    semafor.Release();
                }
                finally
                {
                    Whaiting = false;
                }
            }
            return existeix;

        }
        public bool Contains(IComparable value)
        {
            bool existeix = false;
   
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                if (llistaOrdenada != null)
                    existeix = llistaOrdenada.ContainsKey(value);
                else
                    existeix = llista.Contains(value);
            }
            finally
            {
                try
                {
                    semafor.Release();
                }
                finally
                {
                    Whaiting = false;
                }
            }
            return existeix;

        }
        public void Sort()
        {
            Sort(Comparer<TValue>.Default);//por mirar si funciona
        }
        public void Sort(IComparer<TValue> comparador)
        {
            if (UpdateConfirmation == null || UpdateConfirmation(this))
                try
            {
                semafor.WaitOne(); 
                Whaiting = true;
                llista.Sort(comparador);
            }
            finally
            {
                try
                {
                    semafor.Release();
                }
                finally
                {
                    Whaiting = false;
                    if (Updated != null)
                        Updated(this, new EventArgs());
                }
            }
        }
        public void Disorder()
        {
            if (UpdateConfirmation == null || UpdateConfirmation(this))
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                IEnumerable<TValue> desorden = llista.Desordena();
                this.llista.Clear();
                this.llista.AddRange(desorden);
            }
            finally
            {
                semafor.Release();
                Whaiting = false;
                if (Updated != null)
                    Updated(this, new EventArgs());
            }
        }

        public void Clear()
        {
            if (ClearConfirmation == null || ClearConfirmation(this))
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.Clear();
                semafor.Release();
                Whaiting = false;
                if (llistaOrdenada != null)
                    llistaOrdenada.Clear();
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());
            }
        }
        
        public void Insert(int posicio, TValue valor)
        {
            if(AddConfirmation==null||AddConfirmation(this,valor))
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.Insert(posicio, valor);
            }
            finally
            {
                try
                {
                    semafor.Release();
                }
                finally
                {
                    Whaiting = false;
                    if (Updated != null)
                        Updated(this, new EventArgs());
                    if (Added != null)
                        Added(this, new EventArgs());
                }
            }
        }
        public void Push(TValue valor)
        {
            Insert(0, valor);
        }

        public TValue Peek()
        {
            TValue peek=default(TValue);
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                peek = llista[0];
            }
            catch { }
            finally
            {
                semafor.Release();
                Whaiting = false;
            }
            return peek;
        }
        public TValue Pop()
        {
            TValue primero = Peek();
            RemoveAt(0);
            return primero;
        }


        #region IEnumerable implementation

        public IEnumerator<TValue> GetEnumerator()
        {
            TValue valor;
            TValue[] taula;
            semafor.WaitOne();
            Whaiting = true;
            taula = llista.ToArray();
            semafor.Release();
            Whaiting=false;
            for (int i = 0, taulaLength = taula.Length; i < taulaLength; i++)
            {
                valor = taula[i];
                yield return valor;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.CopyTo(array, arrayIndex);
            }
            finally
            {
                Whaiting = false;
                semafor.Release();
            }
           
        }
        #endregion
    }
    public interface IClauUnicaPerObjecte
    {
        IComparable Clau();
    }

}
