using System;
using System.Text;

namespace UploadDownload7
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.UploadFile uploadFile = new Core.UploadFile();
            var Resul = uploadFile.UploadFileHendler(@"D:\Вн в кармане.rar", 2,"305",null, ConsoleWrite);
            while(true) Console.ReadKey();
        }
        public static void ConsoleWrite(string str, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(str);
        }
    }
}
