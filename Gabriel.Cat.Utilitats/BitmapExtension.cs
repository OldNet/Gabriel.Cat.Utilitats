/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 07/10/2017
 * Hora: 2:33
 * Licencia GNU GPL V3
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Gabriel.Cat.Extension;
using System.Drawing;
namespace Gabriel.Cat
{
	/// <summary>
	/// Description of BitmapExtension.
	/// </summary>
	public static class BitmapExtension
	{
		public static unsafe void SetFragment(byte* bmpTotalInicio,int alturaBmpTotal,int anchuraBmpTotal,bool bmpTotalIsArgb,byte* bmpFragmentoInicio,int alturaBmpFragmento,int anchuraBmpFragmento,bool bmpFragmentoIsArgb,System.Drawing.Point posicionFragmento)
		{
			const int OPCIONIGUALES = 0;
			const int OPCIONSINCON = 1;
			const int OPCIONCONSIN = 2;
			//por acabar
			int totalPixelesLinea;
			int lineas=alturaBmpTotal-posicionFragmento.Y;
			byte*[] ptrsBmpTotal;
			byte*[] ptrsBmpFragmento;
			int bytesPixelBmpTotal=bmpTotalIsArgb?4:3;
			int bytesPixelBmpFragmento=bmpFragmentoIsArgb?4:3;
			int opcion;
			//tener en cuenta las posiciones negativas del fragmento...
			if(lineas<alturaBmpFragmento)
				lineas=alturaBmpFragmento;
			totalPixelesLinea=anchuraBmpTotal-posicionFragmento.X;
			if(totalPixelesLinea<anchuraBmpFragmento)
				totalPixelesLinea=anchuraBmpFragmento;
			
			ptrsBmpFragmento=new byte*[lineas];
			ptrsBmpTotal=new byte*[lineas];
			//posiciono todos los punteros
			
			//pongo  la opcion aqui asi solo se escoge una vez y no en cada linea como estaba antes :)
			opcion=bmpTotalIsArgb.Equals(bmpFragmentoIsArgb)?OPCIONIGUALES:bmpTotalIsArgb?OPCIONCONSIN:OPCIONSINCON;
			
			switch(opcion)
			{
					//pongo las lineas
				case OPCIONIGUALES:
					for(int i=0;i<lineas;i++)
					{
						SetLinea(ptrsBmpTotal[i],bytesPixelBmpTotal,ptrsBmpFragmento[i],totalPixelesLinea);
					}
					break;
				case OPCIONCONSIN:
					for(int i=0;i<lineas;i++)
					{
						SetLineaCS(ptrsBmpTotal[i],ptrsBmpFragmento[i],totalPixelesLinea);
					}
					break;
				case OPCIONSINCON:
					for(int i=0;i<lineas;i++)
					{
						SetLineaCS(ptrsBmpTotal[i],ptrsBmpFragmento[i],totalPixelesLinea);
					}
					break;
			}
		}
		static unsafe void SetLineaSC(byte* ptrBmpTotal,byte* ptrBmpFragmento,int totalPixelesLinea)
		{
			const int RGB=3;
			for(int j=0;j<totalPixelesLinea;j++)
			{
				//pongo cada pixel
				ptrBmpFragmento++;//me salto el byte de la transparencia porque la imagenTotal no tiene
				for(int k=0;k<RGB;k++){
					*ptrBmpTotal=*ptrBmpFragmento;
					ptrBmpTotal++;
					ptrBmpFragmento++;
				}
			}
			
		}
		static unsafe void SetLineaCS(byte* ptrBmpTotal,byte* ptrBmpFragmento,int totalPixelesLinea)
		{
			const byte SINTRANSPARENCIA=0xFF;
			const int RGB=3;
			for(int j=0;j<totalPixelesLinea;j++)
			{
				//pongo cada pixel
				*ptrBmpTotal=SINTRANSPARENCIA;//como no tiene transparencia pongo el byte de la transparencia a sin
				ptrBmpTotal++;
				for(int k=0;k<RGB;k++){
					*ptrBmpTotal=*ptrBmpFragmento;
					ptrBmpTotal++;
					ptrBmpFragmento++;
				}
				
			}
		}
		static unsafe void SetLinea(byte* ptrBmpTotal,int bytesPixel,byte* ptrBmpFragmento,int totalPixelesLinea)
		{
			
			//pongo cada pixel
			for(int j=0;j<totalPixelesLinea;j++)
			{
				
				//pongo cada byte
				for(int k=0;k<bytesPixel;k++){
					*ptrBmpTotal=*ptrBmpFragmento;
					ptrBmpTotal++;
					ptrBmpFragmento++;
				}
			}

			

		}
		public static void SetFragment(this Bitmap bmpTotal,Bitmap bmpFragmento,Point posicionFragmento)
		{
			unsafe{
				
				bmpTotal.TrataBytes((ptrBmpTotal)=>{
				                    	fixed(byte* ptrBmpFragmento=bmpFragmento.GetBytes())
				                    	{
				                    		
				                    		SetFragment(ptrBmpTotal,bmpTotal.Height,bmpTotal.Width,bmpTotal.IsArgb(),ptrBmpFragmento,bmpFragmento.Height,bmpFragmento.Width,bmpFragmento.IsArgb(),posicionFragmento);
				                    		
				                    	}

				                    });
				
				
			}
		}
		public static Point GetRelativePoint(this Rectangle rect,Point point)
		{
			return new Point(rect.Left-point.X,rect.Top-point.Y);
		}
	}
}
