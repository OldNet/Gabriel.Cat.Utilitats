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
			//por acabar
			int totalPixelesLinea;
			int lineas=alturaBmpTotal-posicionFragmento.Y;
			byte*[] ptrsBmpTotal;
			byte*[] ptrsBmpFragmento;
			int bytesPixelBmpTotal=bmpTotalIsArgb?4:3;
			int bytesPixelBmpFragmento=bmpFragmentoIsArgb?4:3;
		//tener en cuenta las posiciones negativas del fragmento...
			if(lineas<alturaBmpFragmento)
				lineas=alturaBmpFragmento;
			totalPixelesLinea=anchuraBmpTotal-posicionFragmento.X;
			if(totalPixelesLinea<anchuraBmpFragmento)
				totalPixelesLinea=anchuraBmpFragmento;
			
			ptrsBmpFragmento=new byte*[lineas];
			ptrsBmpTotal=new byte*[lineas];
			//posiciono todos los punteros
			
			//pongo las lineas
			for(int i=0;i<lineas;i++)
			{
				SetLinea(ptrsBmpTotal[i],bmpTotalIsArgb,bytesPixelBmpTotal,ptrsBmpFragmento[i],bmpFragmentoIsArgb,bytesPixelBmpFragmento,totalPixelesLinea);
			}
		}//si fuera lento separar por cada if y luego llamar a un metodo distinto
		static unsafe void SetLinea(byte* ptrBmpTotal,bool bmpTotalIsArgb,int bytesPixelBmpTotal,byte* ptrBmpFragmento,bool bmpFragmentoIsArgb,int bytesPixelBmpFragmento,int totalPixelesLinea)
		{
			const byte SINTRANSPARENCIA=0xFF;
			if(bmpTotalIsArgb.Equals(bmpFragmentoIsArgb)){
				//pongo cada pixel
				for(int j=0;j<totalPixelesLinea;j++)
				{
					
					//pongo cada byte
					for(int k=0;k<bytesPixelBmpTotal;k++){
						*ptrBmpTotal=*ptrBmpFragmento;
						ptrBmpTotal++;
						ptrBmpFragmento++;
					}
				}
			}
			else if(bmpTotalIsArgb&&!bmpFragmentoIsArgb){
				for(int j=0;j<totalPixelesLinea;j++)
				{
					//pongo cada pixel
					*ptrBmpTotal=SINTRANSPARENCIA;//como no tiene transparencia pongo el byte de la transparencia a sin
					ptrBmpTotal++;
					for(int k=0;k<bytesPixelBmpFragmento;k++){
						*ptrBmpTotal=*ptrBmpFragmento;
						ptrBmpTotal++;
						ptrBmpFragmento++;
					}
					
				}
			}
			else if(!bmpTotalIsArgb&&bmpFragmentoIsArgb){
				for(int j=0;j<totalPixelesLinea;j++)
				{
					//pongo cada pixel
					ptrBmpFragmento++;//me salto el byte de la transparencia porque la imagenTotal no tiene
					for(int k=0;k<bytesPixelBmpTotal;k++){
						*ptrBmpTotal=*ptrBmpFragmento;
						ptrBmpTotal++;
						ptrBmpFragmento++;
					}
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
