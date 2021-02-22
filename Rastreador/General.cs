using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Rastreador
{
    public class General
    {
        public static List<string> ListaCodificacion 
        { 
            get 
            {
                return new List<string>() { "GB2312", "UCS-2", "EUC-JP", "Shift-JIS", "CP1252", "KOI8-U", "GB18030" };
            } 
        }


        public static bool EsHtmlLegible(string html)
        {
            if (string.IsNullOrEmpty(html))
                return false;

            HtmlDocument doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(html.ToLower());
            }
            catch { }

            if (doc != null)
            {
                HtmlNode nodo = doc.DocumentNode.SelectSingleNode("/html");

                if (nodo != null)
                {
                    return true;
                }
                else
                {
                    if (html.ToLower().Contains("<!doctype html>"))
                        return true;
                }

                return false;
            }

            return false;
        }

        public static bool UrlArchivo(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                url = url.ToLower();
                if (url.Contains(".pdf") || url.Contains(".rar") || url.Contains(".png") || url.Contains(".jpg") 
                    || url.Contains(".js") || url.Contains(".m4v") || url.Contains(".svg") 
                    || url.Contains(".css") || url.Contains(".xml") || url.Contains(".mp4") 
                    || url.Contains(".wmv") || url.Contains(".gif") || url.Contains(".mp3") 
                    || url.Contains(".jpeg") || url.Contains("pdfs.html") || url.Contains(".bmp") 
                    || url.Contains(".docx") || url.Contains(".xlsx") || url.Contains(".xls") 
                    || url.Contains(".ppt") || url.Contains(".pptx") || url.Contains(".doc")
                    || url.Contains(".txt") || url.Contains(".json") || url.Contains(".zip"))
                    return true;
            }
            return false;
        }

        public static void EscribeConsola(string mensaje, bool nuevaLinea, ConsoleColor colorLetra) 
        {
            if (!nuevaLinea)
                Console.SetCursorPosition(0, Console.CursorTop - 1);

            Console.ForegroundColor = ConsoleColor.Green;

            Console.Write($"{DateTime.Now} ==> ");

            Console.ForegroundColor = colorLetra;

            Console.Write(mensaje);

            Console.WriteLine();
        }
    }
}
