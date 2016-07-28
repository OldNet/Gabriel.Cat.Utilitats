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
    public class Llista<TValue> : IEnumerable<TValue>
    {
        List<TValue> llista;
        Semaphore semafor;
        bool esperando = false;
        LlistaOrdenada<IComparable, TValue> llistaOrdenada;//sirve para facilitar el encontrar los valores para que sea lo mas ágil posible
        /// <summary>
        /// Ocurre cuando la lista es modificada ya sea en el numero de elementos o en el orden
        /// </summary>
        public event EventHandler Updated;
        public event EventHandler Added;
        public event EventHandler Removed;
        ConfirmacionEventHandler<Llista<TValue>,TValue> AntesDeAñadir;
        ConfirmacionEventHandler<Llista<TValue>, IEnumerable<TValue>> AntesDeAñadirMuchos;
        ConfirmacionEventHandler<Llista<TValue>,TValue> AntesDeBorrar;
        ConfirmacionEventHandler<Llista<TValue>, IEnumerable<TValue>> AntesDeBorrarMuchos;
        ConfirmacionEventHandler<Llista<TValue>> AntesDeVaciar;
        ConfirmacionEventHandler<Llista<TValue>> AntesDeActualizar;
        public Llista()
        {
            llista = new List<TValue>();
            if (typeof(TValue).GetInterface("IClauUnicaPerObjecte") != null)
                llistaOrdenada = new LlistaOrdenada<IComparable, TValue>();
            semafor = new Semaphore(1, 1);
        }
        public Llista(ConfirmacionEventHandler<Llista<TValue>, TValue> metodoAntedDeAñadir, ConfirmacionEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoAntesDeAñadirMuchos, ConfirmacionEventHandler<Llista<TValue>, TValue> metodoAntesDeBorrar, ConfirmacionEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoAntesDeBorrarMuchos, ConfirmacionEventHandler<Llista<TValue>> metodoAntesDeActualizar, ConfirmacionEventHandler<Llista<TValue>> metodoAntesDeVaciar) : this()
        {
            AntesDeActualizar = metodoAntesDeActualizar;
            AntesDeAñadir = metodoAntedDeAñadir;
            AntesDeAñadirMuchos = metodoAntesDeAñadirMuchos;
            AntesDeBorrar = metodoAntesDeBorrar;
            AntesDeBorrarMuchos = metodoAntesDeBorrarMuchos;
            AntesDeVaciar = metodoAntesDeVaciar;
        }
        public Llista(IEnumerable<TValue> valors)
            : this()
        {
            AfegirMolts(valors);
        }
        public Llista(IEnumerable<TValue> valors,ConfirmacionEventHandler<Llista<TValue>, TValue> metodoAntedDeAñadir, ConfirmacionEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoAntesDeAñadirMuchos, ConfirmacionEventHandler<Llista<TValue>, TValue> metodoAntesDeBorrar, ConfirmacionEventHandler<Llista<TValue>, IEnumerable<TValue>> metodoAntesDeBorrarMuchos, ConfirmacionEventHandler<Llista<TValue>> metodoAntesDeActualizar, ConfirmacionEventHandler<Llista<TValue>> metodoAntesDeVaciar):this(metodoAntedDeAñadir,metodoAntesDeAñadirMuchos,metodoAntesDeBorrar,metodoAntesDeBorrarMuchos,metodoAntesDeActualizar,metodoAntesDeVaciar)
        {
            AfegirMolts(valors);
        }
        public void Afegir(TValue value)
        {

            if(AntesDeAñadir==null||AntesDeAñadir(this,value))
            try
            {
                semafor.WaitOne();
                esperando = true;
                llista.Add(value);
                if (llistaOrdenada != null)
                    llistaOrdenada.AfegirORemplaçar(((IClauUnicaPerObjecte)value).Clau(), value);
            }
            finally
            {
                semafor.Release();
                esperando=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Added != null)
                    Added(this, new EventArgs());
            }
        }
        public void AfegirMolts(IEnumerable<TValue> values)
        {
            if (AntesDeAñadirMuchos == null || AntesDeAñadirMuchos(this, values))
                try
            {
                semafor.WaitOne();
                esperando = true;
                llista.AddRange(values);
                if (llistaOrdenada != null)
                {
                    foreach (TValue value in values)
                        try
                        {
                            llistaOrdenada.AfegirORemplaçar(((IClauUnicaPerObjecte)value).Clau(), value);
                        }
                        catch { }
                }
            }
            finally
            {
                semafor.Release();
                esperando=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Added != null)
                    Added(this, new EventArgs());
            }
        }
        public void Elimina(TValue value)
        {
            if (AntesDeBorrar == null || AntesDeBorrar(this, value))
                try
            {
                semafor.WaitOne();
                esperando = true;
                if (llistaOrdenada != null)
                    llistaOrdenada.Elimina((value as IClauUnicaPerObjecte).Clau());
                llista.Remove(value);
            }
            finally
            {
                semafor.Release();
                esperando=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());

            }
        }
        public void Elimina(IEnumerable<TValue> values)
        {
            if (AntesDeBorrarMuchos == null || AntesDeBorrarMuchos(this, values))
            try
            {
                semafor.WaitOne();
                esperando = true;
                if (llistaOrdenada != null)
                {
                    foreach (TValue value in values)
                    {
                        llistaOrdenada.Elimina((value as IClauUnicaPerObjecte).Clau());
                        llista.Remove(value);
                    }
                    }
                else
                {
                    foreach (TValue value in values)
                        llista.Remove(value);
                }
            }
            finally
            {
                semafor.Release();
                esperando = false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());
            }
        }
        public void Elimina(int pos)
        {
            if (AntesDeBorrar == null || AntesDeBorrar(this,this[pos]))
                try
            {
                semafor.WaitOne();
                esperando = true;
                if (llistaOrdenada != null)
                    llistaOrdenada.Elimina((llista[pos] as IClauUnicaPerObjecte).Clau());
                llista.RemoveAt(pos);
            }
            finally
            {
                semafor.Release();
                esperando=false;
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());
            }
        }
        public bool Existeix(TValue value)
        {
            bool existeix = false;

            try
            {
                semafor.WaitOne();
                esperando = true;
                if (llistaOrdenada != null)
                    existeix = llistaOrdenada.Existeix(((IClauUnicaPerObjecte)value).Clau());
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
                    esperando = false;
                }
            }
            return existeix;

        }
        public bool Existeix(IComparable value)
        {
            bool existeix = false;
   
            try
            {
                semafor.WaitOne();
                esperando = true;
                if (llistaOrdenada != null)
                    existeix = llistaOrdenada.Existeix(value);
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
                    esperando = false;
                }
            }
            return existeix;

        }
        public void Ordena()
        {
            Ordena(Comparer<TValue>.Default);//por mirar si funciona
        }
        public void Ordena(IComparer<TValue> comparador)
        {
            if (AntesDeActualizar == null || AntesDeActualizar(this))
                try
            {
                semafor.WaitOne(); 
                esperando = true;
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
                    esperando = false;
                    if (Updated != null)
                        Updated(this, new EventArgs());
                }
            }
        }
        public void Desordena()
        {
            if (AntesDeActualizar == null || AntesDeActualizar(this))
            try
            {
                semafor.WaitOne();
                esperando = true;
                IEnumerable<TValue> desorden = llista.Desordena();
                this.llista.Clear();
                this.llista.AddRange(desorden);
            }
            finally
            {
                semafor.Release();
                esperando = false;
                if (Updated != null)
                    Updated(this, new EventArgs());
            }
        }

        public int Count
        {
            get
            {
                int count;
                semafor.WaitOne();
                esperando = true;
                count = llista.Count;
                semafor.Release();
                esperando=false;
                return count;
            }
        }
        public void Buida()
        {
            if (AntesDeVaciar == null || AntesDeVaciar(this))
            {
                semafor.WaitOne();
                esperando = true;
                llista.Clear();
                semafor.Release();
                esperando = false;
                if (llistaOrdenada != null)
                    llistaOrdenada.Buida();
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());
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
                    esperando = true;
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
                        esperando = false;

                    }
                }
                return value;

            }
            set
            {

                if(AntesDeActualizar==null||AntesDeActualizar(this))
                try
                {
                    semafor.WaitOne();
                    esperando = true;
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
                        esperando = false;
                        if (Updated != null)
                            Updated(this, new EventArgs());
                    }
                }
            }
        }
        public void InserirEn(int posicio, TValue valor)
        {
            if(AntesDeAñadir==null||AntesDeAñadir(this,valor))
            try
            {
                semafor.WaitOne();
                esperando = true;
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
                    esperando = false;
                    if (Updated != null)
                        Updated(this, new EventArgs());
                    if (Added != null)
                        Added(this, new EventArgs());
                }
            }
        }
        public void Push(TValue valor)
        {
            InserirEn(0, valor);
        }

        public TValue Peek()
        {
            TValue peek=default(TValue);
            try
            {
                semafor.WaitOne();
                esperando = true;
                peek = llista[0];
            }
            catch { }
            finally
            {
                semafor.Release();
                esperando = false;
            }
            return peek;
        }
        public TValue Pop()
        {
            TValue primero = Peek();
            Elimina(0);
            return primero;
        }


        #region IEnumerable implementation

        public IEnumerator<TValue> GetEnumerator()
        {
            TValue valor;
            TValue[] taula;
            semafor.WaitOne();
            esperando = true;
            taula = llista.ToArray();
            semafor.Release();
            esperando=false;
            for (int i = 0, taulaLength = taula.Length; i < taulaLength; i++)
            {
                valor = taula[i];
                yield return valor;
            }
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
    public interface IClauUnicaPerObjecte
    {
        IComparable Clau();
    }

}
