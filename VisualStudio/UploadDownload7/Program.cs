using System;
using System.Text;

namespace UploadDownload7
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.DownloadFile downloadFile = new Core.DownloadFile();
            Core.UploadFile uploadFile = new Core.UploadFile();
            Core.UploadDirectory uploadDirectory = new Core.UploadDirectory();
            Core.DownloadDirectory downloadDirectory = new Core.DownloadDirectory();
            //  var Resul = uploadFile.UploadFileHendler(@"D:\Вн в кармане.rar", 20,"305", ConsoleWrite);
            //  downloadFile.DownloadHendler("https://wdho.ru/576c", @"D:\Test", MaxFilesUpload: 20, Password: "305");
             var Resul =  uploadDirectory.UploadFolder(@"C:\Users\den20\Videos\Grand Theft Auto V", "1",10, ConsoleWrite);
            //  downloadDirectory.Download(@"D:\dfd", "https://wdho.ru/dwVq", null, 2, ConsoleWrite);
            while (true) Console.ReadKey();
        }
        // 5 гигов https://wdho.ru/576c
        // 246 мб  https://wdho.ru/576z
        // sads https://wdho.ru/58um
        public static void ConsoleWrite(string str, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(str);
        }
    }
}
