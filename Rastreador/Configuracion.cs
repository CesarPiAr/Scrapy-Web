using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Rastreador
{
    public class Configuracion
    {
        /// <summary>
        /// 
        /// </summary>
        public Configuracion()
        {
            this.proxy = null;
            listaNombreNavegador = new List<string>();
            listaNombreNavegador.Add("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0)");
            listaNombreNavegador.Add("Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)");

            idioma = "es-ES,es;q=0.9,en;q=0.8";
            codificacion = Encoding.UTF8;

            profundidadRastreo = 100;
            numeroMaximoPaginaEncontrada = 1000;
            permiteParametro = false;
        }

        /// <summary>
        /// configuracion proxy 
        /// </summary>
        public WebProxy proxy { get; set; }

        /// <summary>
        /// Lista de User-Agent
        /// </summary>
        public List<string> listaNombreNavegador { get; set; }

        /// <summary>
        /// configuracion accept-language
        /// </summary>
        public string idioma { get; set; }

        public Encoding codificacion { get; set; }

        public int profundidadRastreo { get; set; }

        /// <summary>
        /// numero máximo de paginas encontradas, si el valor es 0 entonces es ilimitado
        /// </summary>
        public int numeroMaximoPaginaEncontrada { get; set; }

        /// <summary>
        /// Aceptar Url con parametros luego del signo ?
        /// </summary>
        public bool permiteParametro { get; set; }
    }
}
