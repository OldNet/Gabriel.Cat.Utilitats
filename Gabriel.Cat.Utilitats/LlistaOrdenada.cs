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
	/// <summary>
	/// Description of LlistaOrdenada.
	/// </summary>
	public class LlistaOrdenada<Tkey,TValue>:IEnumerable<KeyValuePair<Tkey,TValue>>
	{
		SortedList<Tkey,TValue> llista;
        public event EventHandler Updated;
        public event EventHandler Added;
        public event EventHandler Removed;
        public LlistaOrdenada()
		{
			llista = new SortedList<Tkey,TValue>();

		}

        public LlistaOrdenada(IEnumerable<KeyValuePair<Tkey, TValue>> llistaOrdenada):this()
        {
            AfegirMolts(llistaOrdenada);
        }

		public void Afegir(Tkey key, TValue value)
		{
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
		public void  AfegirMolts(IEnumerable<KeyValuePair<Tkey,TValue>> llista)
		{
            foreach (KeyValuePair<Tkey, TValue> un in llista)
				AfegirORemplaçar(un.Key, un.Value);
		}
        public void CanviClau(Tkey keyAnt,Tkey keyNew)
        {
            if (!Existeix(keyAnt))
                throw new Exception("no existeix la clau a remplaçar");
            if (Existeix(keyNew))
                throw new Exception("ja s'utilitza la clau");
            Afegir(keyNew, this[keyAnt]);
            Elimina(keyAnt);
        }
		public bool Elimina(Tkey key)
		{
			bool fet;
            try
            {
                Monitor.Enter(llista);

                fet = llista.Remove(key);
            }
            catch { fet = false; }
            finally
            {
                Monitor.Exit(llista);
                if (Updated != null)
                    Updated(this, new EventArgs());
                if (Removed != null)
                    Removed(this, new EventArgs());
            }
			return fet;
		}
        public bool[] Elimina(IEnumerable<Tkey> keysToRemove)
        {
            Llista<bool> seHaPodidoHacer = new Llista<bool>();
            if(keysToRemove !=null)
            foreach (Tkey key in keysToRemove)
              seHaPodidoHacer.Afegir(Elimina(key));
            return seHaPodidoHacer.ToTaula();
        }

		public void Buida()
		{
            try
            {
                Monitor.Enter(llista);
                llista.Clear();
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
		public void AfegirORemplaçar(Tkey clau, TValue nouValor)
		{
            try
            {
                Monitor.Enter(llista);
                if (llista.ContainsKey(clau))
                    llista[clau] = nouValor;
                else
                {
                    llista.Add(clau, nouValor);
                    if (Added != null)
                        Added(this, new EventArgs());
                }
                }
            finally
            {
                Monitor.Exit(llista);
                if (Updated != null)
                    Updated(this, new EventArgs());

            }
		}
		public int Count
		{ get {
            int count;
            Monitor.Enter(llista);
            count= llista.Count;
            Monitor.Exit(llista);
            return count;
        } }



        public bool Existeix(Tkey key)
		{
            bool existeix;
             Monitor.Enter(llista);
            existeix=llista.ContainsKey(key);
            Monitor.Exit(llista);
            return existeix;
		}
		public TValue this[Tkey key] {
			get {
				TValue value = default(TValue);
                try
                {
                    Monitor.Enter(llista);
                    if (llista.ContainsKey(key))
                        value = llista[key];
                }
                finally
                {
                    Monitor.Exit(llista);
                }
				return value;
			}
			set {
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

		public IEnumerator<KeyValuePair<Tkey, TValue>> GetEnumerator()
		{
			Llista<KeyValuePair<Tkey,TValue>> parelles = new Llista<KeyValuePair<Tkey, TValue>>();
			Monitor.Enter(llista);
			foreach (var par in llista)
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
