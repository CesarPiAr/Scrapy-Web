using System;

namespace Rastreador.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            General.EscribeConsola("Iniciado Proceso", true, ConsoleColor.Gray);

            Rastreador.Rastreo entidad = new Rastreo(new Configuracion());

            entidad.IniciaExtraccion("https://elcomercio.pe/");

            int contador = 0;
            foreach (var item in entidad.ObtenResultado)
            {
                contador++;
                Console.WriteLine($"{contador} ==> {item.ToString()} // {item.titulo}");
            }

            entidad = null;

            General.EscribeConsola("Finaliza Proceso", true, ConsoleColor.Blue);

            Console.ReadLine();

        }
    }
}
