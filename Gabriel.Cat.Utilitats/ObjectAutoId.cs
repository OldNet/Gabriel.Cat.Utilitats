using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public  class ObjectAutoId:IClauUnicaPerObjecte,IComparable<ObjectAutoId>
    {
        static Semaphore semaphorId = new Semaphore(1, 1);
        static long genA = 0;
        static long genB = 0;

        long partA,partB;
        string idUnic;
        public ObjectAutoId()
        {
            semaphorId.WaitOne();
            if(genB==long.MaxValue)
            {
                genA++;
                genB = 0;
            }
            partB = genB++;
            partA = genA;
            semaphorId.Release();
        }
        public string IdAuto
        {
            get
            {
                if(idUnic==null)
                {
                    idUnic = partA.ToString().PadLeft(long.MaxValue.ToString().Length, '0') + partB.ToString().PadLeft(long.MaxValue.ToString().Length, '0');
                }
                return idUnic;
              
            }
        }

        IComparable IClauUnicaPerObjecte.Clau
        {
            get
            {
                return IdAuto;
            }
        }

        int IComparable<ObjectAutoId>.CompareTo(ObjectAutoId other)
        {
            int compareTo = other == null ? (int)Gabriel.Cat.CompareTo.Inferior : partA.CompareTo(other.partA);
            if ((int)Gabriel.Cat.CompareTo.Iguales == compareTo)
                compareTo = partB.CompareTo(other.partB);
            return compareTo;
        }
    }
}
