using System;
using System.Collections.Generic;
using System.Text;

namespace Rastreador
{
    public class Resultado
    {
        private string ulr { get; set; }

        private string html { get; set; }

        public string titulo { get { return Extraccion.ExtraeTitulo(this.html); } }

        public string resumen { get { return Extraccion.ExtraeResumen(this.html); } }

        public Uri link 
        { 
            get 
            {
                if (!string.IsNullOrEmpty(this.ulr))
                    return new Uri(this.ulr);

                return null;
            } 
        }

        public string ObtenMetaDato(string nombreMetaDato) 
        {
            return Extraccion.ExtraeMetaDato(this.html, nombreMetaDato);
        }

        public override string ToString()
        {
            return this.ulr;
        }

        public Resultado(string direccionURL, string contendio) 
        {
            this.ulr = direccionURL;
            this.html = contendio;
        }

    }
}
