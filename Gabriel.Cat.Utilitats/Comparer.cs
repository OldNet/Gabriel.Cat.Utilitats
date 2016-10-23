using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public enum CompareTo
    {
        Iguales = 0,
        Inferior = -1,
        Superior = 1
    }
    public delegate int ComparadorEventHandler<T>(T x, T y);
    public  class Comparer<T> :System.Collections.Generic.Comparer<T>
    {
            ComparadorEventHandler<T> comparador;
            public Comparer(ComparadorEventHandler<T> comparer)
            {
            if (comparer == null) throw new ArgumentNullException("comparador","Comparer method is required");
                this.comparador = comparer;
            }

            public override int Compare(T x, T y)
            {
                return comparador(x, y);
            }
        
    }
}
