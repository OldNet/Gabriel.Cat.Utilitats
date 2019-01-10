using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public delegate void BitmapAnimatedFrameChangedEventHanlder(BitmapAnimated bmpAnimated,Bitmap frameActual);
  public  class BitmapAnimated:ObjectAutoId
    {
       Llista<KeyValuePair<Bitmap, int>> frames;
        bool animarCiclicamente;
        int frameAlAcabar;
        int index;
        int numeroDeRepeticionesFijas;
        Thread hiloAnimacion;
        public event BitmapAnimatedFrameChangedEventHanlder FrameChanged;
        public BitmapAnimated(IList<Bitmap> bmps,params int[] delays)
        {
            int i=0;
            numeroDeRepeticionesFijas = 1;
            frameAlAcabar = 0;
            animarCiclicamente = true;
            frames = new Llista<KeyValuePair<Bitmap, int>>();
            if (bmps != null)
                    for(int j=0;j<bmps.Count;j++)
                    {
                        frames.Add(new KeyValuePair<Bitmap, int>(bmps[j], delays[i]));
                        if (delays.Length > i)
                            i++;
                    }

        }
        public bool AnimarCiclicamente
        {
            get
            {
                return animarCiclicamente;
            }

            set
            {
                animarCiclicamente = value;
            }
        }
        public int FrameAlAcabar
        {
          get { return frameAlAcabar; }
            set { frameAlAcabar = Math.Abs(value) % frames.Count; }
        }
        public int ActualFrameIndex
        {
            get { return this.index; }
            set { index = value % frames.Count; }
        }
        public Bitmap ActualBitmap
        {
            get { return frames[ActualFrameIndex].Key; }
        }

        public bool MostrarPrimerFrameAlAcabar
        {
            get
            {
                return frameAlAcabar==0;
            }

            set
            {
                if (value)
                    frameAlAcabar = 0;
                else frameAlAcabar = frames.Count - 1;
            }
        }
        public int FrameASaltarAnimacionCiclica
        {
            get;set;
        }
        public int NumeroDeRepeticionesFijas
        {
            get
            {
                return numeroDeRepeticionesFijas;
            }

            set
            {
                numeroDeRepeticionesFijas = value;
            }
        }

        public KeyValuePair<Bitmap,int> this[int index]
        {
            get { return frames[index]; }
        }
        public void AddFrame(Bitmap bmp,int delay=500,int posicion=-1)
        {
            if (bmp == null || delay < 0)
                throw new ArgumentException();

            KeyValuePair<Bitmap, int> frame = new KeyValuePair<Bitmap, int>(bmp, delay);

            if (posicion < 0 || posicion > frames.Count)
                frames.Add(frame);
            else
                frames.Insert(posicion, frame);
         }
       
        public void RemoveFrame(int index)
        {
            if (frames.Count < index + 1)
                throw new ArgumentOutOfRangeException();
            frames.RemoveAt(index);
        }
        public void Start()
        {
            if (FrameChanged == null)
                throw new Exception("FrameChanged doesn't asigned ");
            if (hiloAnimacion != null && hiloAnimacion.IsAlive)
                hiloAnimacion.Abort();
            hiloAnimacion = new Thread(() => {
                int numeroDeRepeticiones = 0;
                do
                {
                    for (int i = 0; i < frames.Count; i++)
                    {
                        FrameChanged(this, frames[ActualFrameIndex].Key);
                        Thread.Sleep(frames[ActualFrameIndex].Value);
                        ActualFrameIndex++;
                        if (numeroDeRepeticiones > 0)
                            if (ActualFrameIndex == FrameASaltarAnimacionCiclica)
                                ActualFrameIndex++;
                    }
                    numeroDeRepeticiones++;
                } while (numeroDeRepeticiones<numeroDeRepeticionesFijas||animarCiclicamente);
                
                  FrameChanged(this, frames[FrameAlAcabar].Key);


            });
            hiloAnimacion.Start();
        }
        public void Stop()
        {
            if (hiloAnimacion != null && hiloAnimacion.IsAlive)
                hiloAnimacion.Suspend();
        }
        public void Finsh()
        {
            if(hiloAnimacion!=null&&hiloAnimacion.IsAlive)
                hiloAnimacion.Abort();
        }


        
    }
}
