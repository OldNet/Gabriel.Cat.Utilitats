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
  public  class BitmapAnimated
    {
       Llista<KeyValuePair<Bitmap, int>> frames;
        bool animarCiclicamente;
        bool mostrarPrimerFrameAlAcabar;
        int index;
        Thread hiloAnimacion;
        public event BitmapAnimatedFrameChangedEventHanlder FrameChanged;
        public BitmapAnimated(IEnumerable<Bitmap> bmps,params int[] delays)
        {
            int i=0;
            mostrarPrimerFrameAlAcabar = true;
            animarCiclicamente = true;
            frames = new Llista<KeyValuePair<Bitmap, int>>();
            if (bmps != null)
                    foreach (Bitmap bmp in bmps)
                    {
                        frames.Afegir(new KeyValuePair<Bitmap, int>(bmp, delays[i]));
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
                return mostrarPrimerFrameAlAcabar;
            }

            set
            {
                mostrarPrimerFrameAlAcabar = value;
            }
        }

        public KeyValuePair<Bitmap,int> this[int index]
        {
            get { return frames[index]; }
        }
        public void AddFrame(Bitmap bmp,int delay)
        {
            frames.Afegir(new KeyValuePair<Bitmap, int>(bmp, delay));
        }
        public void RemoveFrame(int index)
        {
            if (frames.Count < index + 1)
                throw new ArgumentOutOfRangeException();
            frames.Elimina(index);
        }
        public void Start()
        {
            if (FrameChanged == null)
                throw new Exception("FrameChanged doesn't asigned ");
            if (hiloAnimacion != null && hiloAnimacion.IsAlive)
                hiloAnimacion.Abort();
            hiloAnimacion = new Thread(() => {
                do
                {
                    for (int i = 0; i < frames.Count; i++)
                    {
                        FrameChanged(this, frames[ActualFrameIndex].Key);
                        Thread.Sleep(frames[ActualFrameIndex].Value);
                        ActualFrameIndex++;
                    }
                } while (animarCiclicamente);
                if (MostrarPrimerFrameAlAcabar)
                  FrameChanged(this, frames[0].Key);


            });
            hiloAnimacion.Start();
        }
        public void Finsh()
        {
            if(hiloAnimacion!=null&&hiloAnimacion.IsAlive)
                hiloAnimacion.Abort();
        }


        
    }
}
