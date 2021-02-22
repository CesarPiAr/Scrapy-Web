using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;

namespace Rastreador
{
    public class Rastreo
    {
        private Configuracion configuracion { get; set; }
        private List<Resultado> listaResultado { get; set; }

        private Queue<string> listaCola { get; set; }

        private int numeroProfundidad { get; set; }
        private int numeroPagina { get; set; }
        public Rastreo(Configuracion configuracion) 
        {
            this.configuracion = configuracion;
            listaResultado = new List<Resultado>();
            listaCola = new Queue<string>();

            General.EscribeConsola("Rastreo Cargador", true, ConsoleColor.Gray);
        }


        public void IniciaExtraccion(string url) 
        {
            try
            {
                Uri direccionUrl = new Uri(url);
                if (direccionUrl == null)
                    return;
            }
            catch { return; }


            General.EscribeConsola($"Iniciado con Web {url}", true, ConsoleColor.Gray);
            General.EscribeConsola($"Esperando ...", true, ConsoleColor.Gray);

            string contenido = ObtenHTML(url);

            if (!string.IsNullOrEmpty(contenido)) 
            {
                AgregaLinkCola(contenido, url);

                while (listaCola.Count > 0) 
                {
                    General.EscribeConsola($"Cantidad Pendiente: {listaCola.Count} <==> Cantidad Recolectada: {listaResultado.Count}", false, ConsoleColor.White);

                    string link = listaCola.Dequeue();
                    if (!string.IsNullOrEmpty(link)) 
                    {
                        if (listaResultado.Count(m => m.ToString() == link) == 0)
                        {
                            contenido = ObtenHTML(link);                            
                            listaResultado.Add(new Resultado(link, contenido));
                            this.numeroPagina++;

                            if (this.numeroPagina >= configuracion.numeroMaximoPaginaEncontrada && configuracion.numeroMaximoPaginaEncontrada > 0)
                            {
                                listaCola.Clear();
                            }
                            else 
                            {
                                if (this.numeroProfundidad < configuracion.profundidadRastreo || configuracion.profundidadRastreo == 0)
                                {
                                    AgregaLinkCola(contenido, url);
                                }
                            }
                        }
                    }
                }
            }
        }

        

        public List<Resultado> ObtenResultado { get { return listaResultado.ToList(); } }

        private void AgregaLinkCola(string html, string url) 
        {
            List<string> listaTemporal = Extraccion.ExtraeUrl(html, url, configuracion.permiteParametro);
            listaTemporal.ForEach(item => 
            {
                if (listaCola.Count(m => m.ToString() == item) == 0)
                    listaCola.Enqueue(item);
            });
            listaTemporal = new List<string>();
            this.numeroProfundidad++;
        }

        private string ObtenHTML(string url) 
        {
            string contenido = string.Empty;

            if (!string.IsNullOrEmpty(url))
            {
                contenido = ObtenHtmlPrimeraOpcion(url);

                if (string.IsNullOrEmpty(contenido))
                    contenido = ObtenHtmlSegundaOpcion(url);

                if (string.IsNullOrEmpty(contenido))
                    contenido = ObtenHtmlTerceraOpcion(url);
            }

            return contenido;
        }

        private void HabilitaProtocoloSeguridad(string url, int intento)
        {            
            Uri uri = new Uri(url);
            if (uri.Scheme == Uri.UriSchemeHttps)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
                {
                    return true;
                };

                switch (intento)
                {
                    case 1:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        break;
                    case 2:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                        break;
                    case 3:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        break;
                    case 4:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        break;
                }
            }
        }

        private class ClienteWeb : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.MaximumAutomaticRedirections = 50;
                request.AllowAutoRedirect = true;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip; // | DecompressionMethods.Brotli;
                return request;
            }
        }

        private string ObtenHtmlPrimeraOpcion(string url) 
        {
            string contenido = string.Empty;

            foreach (var item in this.configuracion.listaNombreNavegador)
            {
                int intentoSSL = 0;
            reintento:;
                try
                {
                    if (intentoSSL > 0)
                        HabilitaProtocoloSeguridad(url, intentoSSL);

                    using (ClienteWeb cliente = new ClienteWeb())
                    {
                        if (configuracion.proxy != null)
                            cliente.Proxy = configuracion.proxy;

                        cliente.Headers.Add("user-agent", item);
                        cliente.Headers.Add("accept-language", this.configuracion.idioma);
                        cliente.Encoding = this.configuracion.codificacion;
                        contenido = cliente.DownloadString(url);

                        string codificacion = string.Empty;
                        
                        if (General.EsHtmlLegible(contenido))                        
                        {
                            codificacion = Extraccion.ExtraeCodificacion(contenido);
                            if (!string.IsNullOrEmpty(codificacion) && Encoding.GetEncoding(codificacion) != Encoding.UTF8)
                            {
                                if (General.ListaCodificacion.Count(x => x == codificacion.ToLower()) > 0)
                                {
                                    codificacion = General.ListaCodificacion.Where(x => x == codificacion.ToLower()).Select(m => m).FirstOrDefault();
                                    cliente.Encoding = Encoding.GetEncoding(codificacion);
                                    contenido = cliente.DownloadString(url);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(contenido))
                        {
                            break;
                        }                        
                    }

                }
                catch (Exception ex)
                {
                    contenido = string.Empty;
                    SSLError(ex, ref intentoSSL);

                    if (intentoSSL > 0 && intentoSSL < 5)
                        goto reintento;
                }
            }
            return contenido;
        }

        private string ObtenHtmlSegundaOpcion(string url)
        {
            string contenido = string.Empty;

            try
            {
            redireccion:;
                HttpWebRequest solicitudWeb = (HttpWebRequest)WebRequest.Create(url);
                solicitudWeb.MaximumAutomaticRedirections = 50;
                solicitudWeb.AllowAutoRedirect = true;
                solicitudWeb.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip; // | DecompressionMethods.Brotli;

                if (configuracion.proxy != null)
                    solicitudWeb.Proxy = configuracion.proxy;

                string nuevaUrl = string.Empty;

                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)solicitudWeb.GetResponse())
                {
                    HttpStatusCode statusHttpWebResponse = myHttpWebResponse.StatusCode;
                    if (statusHttpWebResponse == HttpStatusCode.Found ||
                        statusHttpWebResponse == HttpStatusCode.OK)
                    {
                        nuevaUrl = myHttpWebResponse.ResponseUri.ToString();

                        if (General.UrlArchivo(nuevaUrl))
                        {
                            return string.Empty;
                        }

                        if (nuevaUrl != url)
                        {
                            url = nuevaUrl;
                            goto redireccion;
                        }

                        System.IO.Stream resStream = myHttpWebResponse.GetResponseStream();
                        using (var reader = new System.IO.StreamReader(resStream))
                        {
                            contenido = reader.ReadToEnd();
                            if (contenido.ToLower().StartsWith("<?xml version"))
                            {
                                contenido = "";
                            }
                        }

                        if (!string.IsNullOrEmpty(contenido))
                        {
                            if (!General.EsHtmlLegible(contenido))
                            {                             
                                contenido = string.Empty;
                            }
                        }
                    }
                }
            }
            catch { }

            return contenido;
        }

        private string ObtenHtmlTerceraOpcion(string url)
        {
            string contenido = string.Empty;

            try
            {
                HttpClientHandler clienteHandler = new HttpClientHandler();
                if (configuracion.proxy != null)
                    clienteHandler.Proxy = configuracion.proxy;

                var clienteHttp = new HttpClient(clienteHandler);
                
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                var res = clienteHttp.SendAsync(req);

                contenido = res.Result.Content.ReadAsStringAsync().Result;
                string codificacion = Extraccion.ExtraeCodificacion(contenido);

                if (!string.IsNullOrEmpty(codificacion))
                {
                    byte[] response = clienteHttp.GetByteArrayAsync(url).Result;

                    if (General.ListaCodificacion.Count(x => x == codificacion.ToLower()) > 0)
                    {
                        codificacion = General.ListaCodificacion.Where(x => x == codificacion.ToLower()).Select(m => m).FirstOrDefault();
                    }
                    else
                        codificacion = Encoding.UTF8.EncodingName;

                    contenido = Encoding.GetEncoding(codificacion).GetString(response, 0, response.Length - 1);
                }

                if (!string.IsNullOrEmpty(contenido))
                {
                    if (!General.EsHtmlLegible(contenido))
                    {
                        contenido = string.Empty;
                    }
                }

            }
            catch { }

            return contenido;
        }

        private void SSLError(Exception ex, ref int intentoSSL) 
        {
            if (ex.Message.Contains("SSL connection"))
                intentoSSL++;
        }

    }
}
