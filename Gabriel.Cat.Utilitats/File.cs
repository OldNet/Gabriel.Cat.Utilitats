using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//estaria bien añadir al menucontextual de windows estas cosas :)
//estaria bien que al inicio de windows pueda acabar el trabajo pendiente
//estaria bien que acabara el trabajo abriendo cualquier archivo origen o destino a no ser que sea copiar que luego solo es destino
//estaria bien que hubiesen miniaturas para todas las extensiones que representen lo que se hace y la miniatura sea del archivo original.(se tendria que guardar a parte)
//se tendria que guardar el idRapido del archivo para poderlo encontrar en caso de copiar(mover a no ser que este duplicado no se encontraria...)
//poder hacer a la vez varios procesos que no se tocan por que estan en distintas unidades(aunque quizas es dificil de determinar si una particion es de un disco y otra de otro...)
    //la idea es que si no se molestan no es necesario que se esperen no?
    //en caso de que se molesten o no siempre se pueden poner en marcha aunque tarde más :) pero habria un semaforo y se haria una cosa a la vez :) 
namespace Gabriel.Cat
{
    /// <summary>
    /// Copy and move files like a torrent
    /// </summary>
   public class FileTorrent
    {
        /// <summary>
        /// Extensión archivo temporal mientras se copia en el destino
        /// </summary>
        public const string EXTENSIONCOPY = ".CopyTorrent.File";
        /// <summary>
        /// Extensión archivo temporal mientras se mueve el archivo en la fuente (el original). 
        /// </summary>
        public const string EXTENSIONMOVESOURCE = ".MoveTorrentSource.File";
        /// <summary>
        /// Extensión archivo temporal mientras se mueve el archivo en el destino. 
        /// </summary>
        public const string EXTENSIONMOVEDESTINATION = ".MoveTorrentDestination.File";
        /// <summary>
        /// Extensión archivo temporal mientras se cancela el mover el archivo y vuelve al origen. La tendrá hasta volver a ser como antes
        /// </summary>
        public const string EXTENSIONCANCELMOVESOURCE = ".CancelMoveTorrentSource.File";//es mover pero al revés
        /// <summary>
        /// Extensión archivo temporal mientras se cancela el mover el archivo y vuelve al origen. La tendrá hasta que acabe y pueda ser borrado
        /// </summary>
        public const string EXTENSIONCANCELMOVEDESTINATION = ".CancelMoveTorrentDestination.File";//es mover pero al revés
        //de mover a copiar en el source cuando acaba de copiar en el destino recibe lo que le falta del destino(osea la parte movida se copia para tenerlos)...mejor hacer extension de transicion para copi y luego continuar como una copia normal
        ///<summary>
        /// Extensión archivo temporal mientras se convierte el mover en copiar.la parte movida se copia al original para luego continuar como una copia normal. extension puesta en el origen
        /// </summary>
        public const string EXTENSIONTRANSICIONMOVECOPYSOURCE = ".TransicionMoveToCopyTorrentSource.File";
        ///<summary>
        /// Extensión archivo temporal mientras se convierte el mover en copiar.la parte movida se copia al original para luego continuar como una copia normal. extension puesta en el destino
        /// </summary>
        public const string EXTENSIONTRANSICIONMOVECOPYDESTINATION = ".TransicionMoveToCopyTorrentDestination.File";
        //de copiar a mover en el source se borra la parte pasada para poder pasar la extension a la de mover en ambos archivos (hay una extension de transición)
        ///<summary>
        /// Extensión archivo temporal mientras se convierte el copiar en mover.la parte copiada se borra al original para luego continuar como un mover normal. extension puesta en el origen
        /// </summary>
        public const string EXTENSIONTRANSICIONCOPYMOVESOURCE = ".TransicionCopyToMoveTorrentSource.File";
        ///<summary>
        /// Extensión archivo temporal mientras se convierte  el copiar en mover.la parte copiada se borra al original para luego continuar como un mover normal.extension puesta en el destino
        /// </summary>
        public const string EXTENSIONTRANSICIONCOPYMOVEDESTINATION = ".TransicionCopyToMoveTorrentDestination.File";
        //cancelar el copiar a mover o el mover a copiar (tiene que volver al origen y borrar el destino) con estos casos no habrán más a contemplar :)
    }
}
