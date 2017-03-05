using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace Gabriel.Cat.Extension
{
    public delegate void TemporizadorEventHandler(Temporizador temporizador);
   public class Temporizador:ObjectAutoId
    {
       public TemporizadorEventHandler Elapsed;
       Thread hiloEjecucion;
       int tiempoCiclo;
       bool estaEsperando;


       public Temporizador():this(10000)
       {
       }
       public Temporizador(int milisegundosCiclo)
       {
           TiempoCiclo = milisegundosCiclo;
       }

       public int TiempoCiclo
       {
           get { return tiempoCiclo; }
           set { tiempoCiclo = value; }
       }
       public int Interval {
           get { return TiempoCiclo; }
           set { TiempoCiclo = value; }
       }
       public bool EstaEsperando
       {
           get { return estaEsperando; }
           private set { estaEsperando = value; }
       }
       public void Start()
       {
           hiloEjecucion = new Thread(() =>
           {
               while (true)
               {
                   estaEsperando = true;
                   Thread.Sleep(TiempoCiclo);
                   estaEsperando = false;
                   if (Elapsed != null)
                       Elapsed(this);
               }
           });
           hiloEjecucion.Start();
       }
       public void Stop()
       {
           IStopAndAbort(false);
       }
       public void StopAndAbort()
       {
           IStopAndAbort(true);
       }
       private void IStopAndAbort(bool abortar)
       {
           if (hiloEjecucion != null && hiloEjecucion.IsAlive)
           {
               while (estaEsperando&&!abortar) Thread.Sleep(100);//asi no aborta el metodoConParametros
               hiloEjecucion.Abort();
           }
       }
    }

}
