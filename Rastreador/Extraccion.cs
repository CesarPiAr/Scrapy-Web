using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Rastreador
{
    public class Extraccion
    {

        public static string ExtraeCodificacion(string html)
        {
            string codificacion = string.Empty;

            if (!string.IsNullOrEmpty(html)) 
            {
                try
                {
                    HtmlDocument documentoHtml = new HtmlDocument();
                    documentoHtml.LoadHtml(html);

                    HtmlNode nodo = documentoHtml.DocumentNode.SelectSingleNode("//meta[@charset]");
                    if (nodo != null)
                        codificacion = nodo.Attributes["charset"].Value;
                }
                catch
                {
                }
            }

            return codificacion;
        }

        public static string ExtraeTitulo(string html)
        {
            string titulo = string.Empty;            

            string[] rutaXPath = {
                "//head//meta[@property=\"og:title\"]"
                , "//h1[1]"
                , "//head//meta[@name=\"twitter:title\"]"
                };

            if (!string.IsNullOrEmpty(html))
            {
                HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode nodo = null;

                try
                {
                    nodo = doc.DocumentNode.SelectSingleNode("//title");
                    if (nodo != null)
                    {
                        titulo = nodo.InnerText;
                    }

                }
                catch { }

                if (string.IsNullOrEmpty(titulo))
                {                    
                    foreach (var item in rutaXPath)
                    {
                        try
                        {
                            nodo = doc.DocumentNode.SelectSingleNode(string.Format("{0}", item));
                            if (nodo != null)
                            {
                                titulo = nodo.Attributes["content"].Value;
                                break;
                            }
                        }
                        catch { }                            
                    }                    
                }

                nodo = null;
                doc = null;
            }
            return titulo;
        }

        public static string ExtraeResumen(string html)
        {
            string resumen = string.Empty;

            string[] rutaXPath = {
                "//meta[@name=\"twitter:description\"]"
                , "//meta[@name='description']"
                , "//meta[@name='DESCRIPTION']"
                , "//meta[@name=\"DESCRIPTION\"]"
                , "//meta[@name='DESCRIPTION']"
                , "//meta[@name=\"description\"]"
                , "//meta[@property=\"og:description\"]"
                };

            if (!string.IsNullOrEmpty(html))
            {
                try
                {                    
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    HtmlNode nodo = null;

                    foreach (var item in rutaXPath)
                    {
                        try
                        {
                            nodo = doc.DocumentNode.SelectSingleNode(item);
                            if (nodo != null)
                            {
                                resumen = nodo.Attributes["content"].Value.Trim();
                                break;
                            }
                        }
                        catch { }                            
                    }

                    if (string.IsNullOrEmpty(resumen))
                    {
                        try
                        {                                
                            nodo = doc.DocumentNode.SelectSingleNode("//h2");

                            if (nodo != null)
                                resumen = nodo.OuterHtml;                                
                        }
                        catch { }
                    }

                    doc = null;
                    
                }
                catch { }

            }
            return resumen;
        }

        public static string ExtraeMetaDato(string html, string nombreMetaDato)
        {
            string contenido = string.Empty;

            if (!string.IsNullOrEmpty(html))
            {
                try
                {
                    if (string.IsNullOrEmpty(nombreMetaDato))
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);

                        HtmlNode nodo = null;
                        
                        try
                        {
                            nodo = doc.DocumentNode.SelectSingleNode($"//meta[@name=\"{nombreMetaDato}\"]");
                            if (nodo != null)
                            {
                                contenido = nodo.Attributes["content"].Value.Trim();
                            }
                        }
                        catch { }
                        
                        doc = null;
                    }
                }
                catch { }

            }
            return contenido;
        }

        public static List<string> ExtraeUrl(string html, string urlBase, bool permiteParametro)
        {
            List<string> listaTemporal = new List<string>();
            Uri dominio = new Uri(urlBase);
            try
            {
                if (!string.IsNullOrEmpty(html))
                {
                    if (html.Contains("href=\"//") || html.Contains("href='//"))
                    {
                        html = html.Replace("href=\"//", string.Concat("href=", '"', urlBase.Substring(0, urlBase.IndexOf(":")), "//"));
                        html = html.Replace("href='//", string.Concat("href=", "'", urlBase.Substring(0, urlBase.IndexOf(":")), "//"));
                    }

                    html = html.Replace(string.Concat("href=", '"', '/'), string.Concat("href=", '"', dominio.Host, '/'));
                    html = html.Replace(string.Concat("href=", "'", "/"), string.Concat("href=", "'", dominio.Host, "/"));

                    HtmlAgilityPack.HtmlDocument documentoHtml = new HtmlAgilityPack.HtmlDocument();
                    documentoHtml.LoadHtml(html);
                    string url = string.Empty;

                    foreach (HtmlNode node in documentoHtml.DocumentNode.SelectNodes("//a"))
                    {
                        //dominio = new Uri(url);

                        try
                        {
                            url = ExtraeUrlEtiquetaA(node.OuterHtml);
                            url = url.Trim();
                            string href = url;

                            if (!string.IsNullOrEmpty(url))
                            {
                                if (url[0] != '/' && !url.StartsWith("www.") && !url.StartsWith("http"))
                                {
                                    if (!url.StartsWith(dominio.Scheme) && url.StartsWith(dominio.Host))
                                        url = $"{dominio.Scheme}://{url}";
                                    else if (!url.StartsWith(dominio.Scheme) && !url.StartsWith(dominio.Host))
                                        url = $"{dominio.Scheme}://{dominio.Host}/{url}";
                                }

                                if (url[0] != '#' && !url.Contains("javascript:") && !url.Contains("mailto:"))
                                {
                                    url = url.Replace(".twitter", string.Empty);

                                    url = url.Replace("https//", "https://");
                                    url = url.Replace("http//", "http://");
                                    url = url.Replace("https/", "https://");
                                    url = url.Replace("http/", "http://");

                                    if (url.StartsWith("www."))
                                        url = string.Concat(dominio.Scheme, "://", url);

                                    if (!url.StartsWith("http"))
                                    {
                                        if (url.Contains(".asp") || url.Contains(".htm") || url.Contains(".php") || url.Contains(".js"))
                                        {
                                            url = url.Substring(0, url.LastIndexOf("/"));
                                        }

                                        url = $"{dominio.Scheme}://{dominio.Host}/{url}";

                                        if (new Uri(url).Host == dominio.Host)
                                            listaTemporal.Add(url);
                                    }
                                    else
                                    {
                                        if (new Uri(url).Host == dominio.Host)
                                            listaTemporal.Add(url);
                                        
                                        if (url.Contains("?"))
                                        {
                                            url = WebUtility.UrlDecode(url);
                                            Uri parametros = new Uri(url);

                                            string nuevaUrlTemporal = string.Empty;

                                            if (url.Contains("url="))
                                            {
                                                nuevaUrlTemporal = System.Web.HttpUtility.ParseQueryString(parametros.Query).Get("url");
                                            }

                                            if (url.Contains("u="))
                                            {
                                                nuevaUrlTemporal = System.Web.HttpUtility.ParseQueryString(parametros.Query).Get("u");
                                            }

                                            if (url.Contains("link="))
                                            {
                                                nuevaUrlTemporal = System.Web.HttpUtility.ParseQueryString(parametros.Query).Get("link");
                                            }

                                            if (!string.IsNullOrEmpty(nuevaUrlTemporal))
                                            {
                                                nuevaUrlTemporal = (nuevaUrlTemporal.Trim()).Replace(" ", "+");

                                                if (dominio.Host == new Uri(nuevaUrlTemporal).Host)
                                                    listaTemporal.Add(nuevaUrlTemporal);
                                            }


                                            parametros = null;
                                        }

                                    }
                                }
                            }
                        }
                        catch { }
                        url = string.Empty;
                    }
                    documentoHtml = null;
                }
            }
            catch { }            

            return DepuraURL(listaTemporal.Distinct().Where(m => !General.UrlArchivo(m)).ToList(), permiteParametro);
        }


        private static string ExtraeUrlEtiquetaA(string html)
        {
            string url = string.Empty;

            if (!string.IsNullOrEmpty(html))
            {
                html = html.Replace("'", "\"");
                string filtroHref = @"href=\""(.*?)\""";
                string filtroDataHref = @"data-href=\""(.*?)\""";
                string filtroHreflang = @"hreflang=\""(.*?)\""";
                try
                {
                    var match = Regex.Match(html, filtroHref, RegexOptions.IgnoreCase);
                    if (match.Success)
                        url = match.Groups[1].Value;

                    if (string.IsNullOrEmpty(url))
                    {
                        match = Regex.Match(html, filtroDataHref, RegexOptions.IgnoreCase);
                        if (match.Success)
                            url = match.Groups[1].Value;
                    }

                    if (string.IsNullOrEmpty(url))
                    {
                        match = Regex.Match(html, filtroHreflang, RegexOptions.IgnoreCase);
                        if (match.Success)
                            url = match.Groups[1].Value;
                    }
                    url = url.Replace("\t", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                }
                catch { }
            }
            return url;
        }

        private static List<string> DepuraURL(List<string> listaTemporal, bool permiteParametro)
        {
            List<string> nuevo = new List<string>();
            try
            {
                int cantidad = 0;
                int total = listaTemporal.Count();
                if (listaTemporal.Count() > 0)
                {
                    foreach (string item in listaTemporal)
                    {
                        cantidad++;
                        string nuevoLink = string.Empty;

                        try
                        {
                            nuevoLink = item.Replace("&amp;", "&");

                            nuevoLink = nuevoLink.Replace("://", "***");
                            nuevoLink = nuevoLink.Replace("//", "/");
                            nuevoLink = nuevoLink.Replace("***", "://");

                            if (nuevoLink.Contains("+"))
                                nuevoLink = nuevoLink.Replace("+", "%2B");

                            nuevoLink = WebUtility.UrlDecode(nuevoLink).Trim();
                            nuevoLink = WebUtility.HtmlDecode(nuevoLink).Trim();

                            if (nuevoLink.StartsWith("www."))
                                nuevoLink = string.Concat("http://", nuevoLink);

                            if (!permiteParametro)
                            {
                                if (nuevoLink.Contains("?"))
                                    nuevoLink = nuevoLink.Remove(nuevoLink.IndexOf("?"));

                                if (nuevoLink.Contains("#"))
                                    nuevoLink = nuevoLink.Remove(nuevoLink.IndexOf("#"));
                            }

                            nuevoLink = nuevoLink.TrimEnd('/');
                            nuevoLink = nuevoLink.Replace("/../", "/");                           

                            if (nuevoLink.EndsWith("}}"))
                                nuevoLink = string.Empty;

                            if (!string.IsNullOrEmpty(nuevoLink))
                                nuevo.Add(nuevoLink);
                        }
                        catch { }
                    }

                    listaTemporal = null;
                }
            }
            catch { }

            return nuevo.Distinct().ToList();
        }
    }
}
