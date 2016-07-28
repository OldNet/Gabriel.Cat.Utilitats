/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 11/12/2014
 * Hora: 20:42
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Gabriel.Cat
{
    public delegate bool ConfirmacionEventHandler<TSender, TKey, TValue>(TSender sender, TKey key, TValue value);
    public delegate bool ConfirmacionEventHandler<TSender, TKey>(TSender sender, TKey arg);
    public delegate bool ConfirmacionCambioClaveEventHandler<TSender, TKey>(TSender sender, TKey keyAnt,TKey keyNew);
    public delegate bool ConfirmacionEventHandler<TSender>(TSender sender);
    /// <summary>
    /// Description of LlistaOrdenada.
    /// </summary>
    public class LlistaOrdenada<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        SortedList<TKey, TValue> llista;
        public event EventHandler Updated;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> AntesDeAñadir;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey> AntesDeBorrar;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> AntesDeVaciar;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> AntesDeActualizar;
        ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> AntesDeCambiarClave;
        public event EventHandler Added;
        public event EventHandler Removed;
        public LlistaOrdenada()
        {
            llista = new SortedList<TKey, TValue>();

        }
        public LlistaOrdenada(ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> metodoAntesDeAñadir, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoAntesDeBorrar, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoAntesDeVaciar, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoAntesDeActualizar, ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoAntesDeCambiarClave) : this()
        {
            AntesDeAñadir = metodoAntesDeAñadir;
            AntesDeBorrar = metodoAntesDeBorrar;
            AntesDeVaciar = metodoAntesDeVaciar;
            AntesDeActualizar = metodoAntesDeActualizar;
        }
        public LlistaOrdenada(IEnumerable<KeyValuePair<TKey, TValue>> llistaOrdenada) : this()
        {
            AfegirMolts(llistaOrdenada);
        }
        public LlistaOrdenada(IEnumerable<KeyValuePair<TKey, TValue>> llistaOrdenada, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> metodoAntesDeAñadir, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoAntesDeBorrar, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoAntesDeVaciar, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoAntesDeActualizar, ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoAntesDeCambiarClave) : this(metodoAntesDeAñadir, metodoAntesDeBorrar, metodoAntesDeVaciar,metodoAntesDeActualizar,metodoAntesDeCambiarClave)
        {
            AfegirMolts(llistaOrdenada);
        }
        public void Afegir(TKey key, TValue value)
        {
            bool añadir = AntesDeAñadir == null || AntesDeAñadir(this, key, value);
            if (añadir)
                try
                {
                    Monitor.Enter(llista);

                    llista.Add(key, value);

                }
                finally
                {
                    Monitor.Exit(llista);

                    if (Updated != null)
                        Updated(this, new EventArgs());
                    if (Added != null)
                        Added(this, new EventArgs());

                }
        }
        public void AfegirMolts(IEnumerable<KeyValuePair<TKey, TValue>> llista)
        {
            foreach (KeyValuePair<TKey, TValue> un in llista)
                AfegirORemplaçar(un.Key, un.Value);
        }
        public void CanviClau(TKey keyAnt, TKey keyNew)
        {
            if (!Existeix(keyAnt))
                throw new Exception("no existeix la clau a remplaçar");
            if (Existeix(keyNew))
                throw new Exception("ja s'utilitza la clau");
            if (AntesDeCambiarClave == null || AntesDeCambiarClave(this,keyAnt,keyNew))
            {
                Afegir(keyNew, this[keyAnt]);
                if (Existeix(keyNew))//si se ha cancelado no se tien que reemplazar 
                    Elimina(keyAnt);
            }
        }
        public bool Elimina(TKey key)
        {
            bool fer = AntesDeBorrar == null || AntesDeBorrar(this, key);
            if (fer)
                try
                {
                    Monitor.Enter(llista);
                    fer = llista.Remove(key);
                }
                catch { fer = false; }
                finally
                {
                    Monitor.Exit(llista);
                    if (fer)
                    {
                        if (Updated != null)
                            Updated(this, new EventArgs());
                        if (Removed != null)
                            Removed(this, new EventArgs());
                    }
                }
            return fer;
        }
        public bool[] Elimina(IEnumerable<TKey> keysToRemove)
        {
            List<bool> seHaPodidoHacer = new List<bool>();
            if (keysToRemove != null)
                foreach (TKey key in keysToRemove)
                    seHaPodidoHacer.Add(Elimina(key));
            return seHaPodidoHacer.ToTaula();
        }

        public void Buida()
        {
            if (Removed == null || AntesDeVaciar(this))
                try
                {
                    Monitor.Enter(llista);

                    {

                        llista.Clear();

                    }
                }
                finally
                {
                    Monitor.Exit(llista);

                    if (Updated != null)
                        Updated(this, new EventArgs());
                    if (Removed != null)
                        Removed(this, new EventArgs());

                }
        }
        public void AfegirORemplaçar(TKey clau, TValue nouValor)
        {
            bool hecho = false;
            try
            {
                Monitor.Enter(llista);
                if (llista.ContainsKey(clau))
                {
                    Monitor.Exit(llista);//para poder usarlo en durante el método de validación
                    if (AntesDeActualizar == null || AntesDeActualizar(this))
                    {
                        Monitor.Enter(llista);
                        llista[clau] = nouValor;
                        hecho = true;

                    }
                }
                else
                {
                    Monitor.Exit(llista);//para poder usarlo en durante el método de validación
                    if (AntesDeAñadir == null || AntesDeAñadir(this, clau, nouValor))
                    {
                        Monitor.Enter(llista);
                        llista.Add(clau, nouValor);
                        if (Added != null)
                            Added(this, new EventArgs());
                        hecho = true;
                    }

                }
            }
            finally
            {
                try
                {
                    Monitor.Exit(llista);
                }
                finally
                {
                    if (hecho)
                    {
                        if (Updated != null)
                            Updated(this, new EventArgs());
                    }
                }

            }
        }
        public int Count
        {
            get
            {
                int count;
                Monitor.Enter(llista);
                count = llista.Count;
                Monitor.Exit(llista);
                return count;
            }
        }



        public bool Existeix(TKey key)
        {
            bool existeix;
            Monitor.Enter(llista);
            existeix = llista.ContainsKey(key);
            Monitor.Exit(llista);
            return existeix;
        }
        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                try
                {
                    Monitor.Enter(llista);
                    if (llista.ContainsKey(key))
                        value = llista[key];
                     else value = default(TValue);
                }
                finally
                {
                    Monitor.Exit(llista);
                }
                return value;
            }
            set
            {
                bool actualizar = AntesDeActualizar==null||AntesDeActualizar(this);
                if(actualizar)
                try
                {
                    Monitor.Enter(llista);
                    if (llista.ContainsKey(key))
                        llista[key] = value;
                }
                finally
                {
                    Monitor.Exit(llista);
                    if (Updated != null)
                        Updated(this, new EventArgs());
                }

            }
        }

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Llista<KeyValuePair<TKey, TValue>> parelles = new Llista<KeyValuePair<TKey, TValue>>();
            Monitor.Enter(llista);
            foreach (KeyValuePair<TKey, TValue> par in llista)
                parelles.Afegir(par);
            Monitor.Exit(llista);
            return parelles.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        #endregion
    }

}
