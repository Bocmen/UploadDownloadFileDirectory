using System;
using System.Text;

namespace UploadDownload7
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.Loget loget = new Core.Loget(new Core.FunctionAndSetting());
            //  loget.GenereteName("Test", 20);
            //   loget.SearchName("Test",20);
            //   loget.Delite();
            //   loget.AddData("Привет");
            //   loget.AddData("Это проверка");

            Core.UploadDownloadFile.DownloadFile downloadFile = new Core.UploadDownloadFile.DownloadFile();
            Core.UploadDownloadFile.UploadFile uploadFile = new Core.UploadDownloadFile.UploadFile();
            Core.UploadDirectory uploadDirectory = new Core.UploadDirectory();

           // var resul = uploadFile.UploadFileHendler(@"C:\Users\den20\Videos\Grand Theft Auto V\Grand Theft Auto V 2020.01.23 - 22.52.12.23.DVR.mp4", 10,"305", ConsoleWrite);
            //  var Resul = uploadFile.UploadFileHendler(@"D:\Вн в кармане.rar", 20,"305", ConsoleWrite);
            //  downloadFile.DownloadHendler("https://wdho.ru/576c", @"D:\Test", MaxFilesUpload: 20, Password: "305");
            //     var Resul =  uploadDirectory.UploadFolder(@"C:\Users\den20\OneDrive\Изображения\Новая папка (2)", "3434",10, ConsoleWrite);
            //     downloadDirectory.Download(@"D:\dfd", Resul, "3434", 2, ConsoleWrite);
            //  downloadFile.DownloadHendler("https://wdho.ru/9o3m", @"D:\Test",massenge: ConsoleWrite, Password:"305",MaxFilesUpload:5);
            //D:\Test
            uploadDirectory.UploadFolder(@"D:\Test","sds",10, ConsoleWrite);
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
