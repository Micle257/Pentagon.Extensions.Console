using System;

namespace Pentagon.Utilities.Console.Demo
{
    using Controls;
    using Security;
    using Console = System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            var swi = new SwitchCliControl("Is itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs itIs it?", true);

          var result =  swi.Run();

            Console.WriteLine();

            var sad = new SecretTextCliControl("Name id",  SecretTextOutputMode.Asterisk).Run();

            Console.WriteLine();
            Console.WriteLine(sad.ConvertToString());
            Console.ReadKey();
        }
    }
}
