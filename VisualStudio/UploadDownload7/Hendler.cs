﻿/*
|
|
| vk.com      https://vk.com/denisivanov220
+----------------------------------------------
| github.com  https://github.com/Bocmen
|
|
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UploadDownload7
{
    namespace Core
    {
        /// <summary>
        /// Загрузка папок и файлов
        /// </summary>
        public class UploadDirectory
        {

        }
        /// <summary>
        /// Загрузка файла
        /// </summary>
        public class UploadFile
        {
            //==================================================== То что можно переопределить
            /// <summary>
            /// Набор функций для данного класа некоторые из них можно переопределить
            /// </summary>
            public FunctionAndSetting functionAndSetting = new FunctionAndSetting();
            /// <summary>
            /// Необходима для рандомного пароля
            /// </summary>
            private static Random random = new Random();
            //==================================================== Данные необходимые для работы
            /// <summary>
            /// Бд файла содержит в себе ссылки на все части файла его название и.т.д (для более подробного ознакомления смотри структуру)
            /// </summary>
            private UploadFileInfo uploadFileInfo;
            /// <summary>
            /// Текущие количество загружеемых одновременно файлов
            /// </summary>
            private byte Count = 0;
            /// <summary>
            /// Максимальное число одновременно загружаемых файлов
            /// </summary>
            private byte MaxFilesUpload = 1;
            /// <summary>
            /// Путь к временному файлу хранящитй в себе перечень успешнозагруженных файлов
            /// </summary>
            private string StateInfoPatchFile;
            //==================================================== Методы
            public ResulUploadFileInfo UploadFileHendler(string PatchFile, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null)
            {
                if (!File.Exists(PatchFile)) { if (massenge != null) massenge.Invoke("[Error] Такого пути к файлу не существует", ConsoleColor.Red); return new ResulUploadFileInfo(); }
                this.MaxFilesUpload = MaxFilesUpload;
                StateInfoPatchFile = Path.Combine(Path.GetDirectoryName(PatchFile), Path.GetFileName(PatchFile) + ".logUploadFile");
                uploadFileInfo = new UploadFileInfo { NameFale = Path.GetFileName(PatchFile), Parts = new List<DataSaveInfo>(0) };
                // Проверяем если файл меньше одной части то сразу он сохраняется
                if ((new System.IO.FileInfo(PatchFile)).Length <= (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart))
                {
                    if (massenge != null) massenge.Invoke("[Info] Файл не будет разбит на несколько частей", ConsoleColor.Blue);
                    return new ResulUploadFileInfo { UrlSave = UploadDownloadWdho.Upload.UploadHendler(functionAndSetting.Compress(File.ReadAllBytes(PatchFile), Password), Path.GetFileName(PatchFile)).url, InfoSave = true };
                }
                else
                {
                    // Запуск потока чтения основного файла который будет разбит
                    FileStream file = new FileStream(PatchFile, FileMode.Open);
                    // Необходимо чтобы индифицировать части
                    long c = long.MaxValue;
                    // Получаем сколько будет весить одна часть в байтах
                    ulong @byte = (ulong)functionAndSetting.MaxMbPart * FunctionAndSetting.OneMb;
                    // Сколько раз нужно пройтись по файлу чтобы считать одну часть
                    long count = (long)((@byte % (FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead) > 0) ? (long)(@byte / (FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead)) + 1 : (long)(@byte / (FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead)));
                    if (massenge != null) massenge.Invoke(String.Format("[Info] Файл будет разбит на {0} частей", (file.Length % (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart) > 0 ? file.Length / (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart) + 1 : file.Length / (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart))), ConsoleColor.Blue);

                    if (File.Exists(StateInfoPatchFile))
                    {

                        string[] PartInfo = File.ReadAllText(StateInfoPatchFile).Split("\r\n");
                        if (PartInfo[0] == @byte.ToString())
                        {
                            for (long i = 1; i < (PartInfo.LongLength - 1); i++)
                            {
                                uploadFileInfo.Parts.Add(JsonConvert.DeserializeObject<DataSaveInfo>(PartInfo[i]));
                                if (massenge != null) massenge.Invoke("[Info] Ранее эта часть была загружена восстанавливаю данные", ConsoleColor.Green);
                            }
                        }
                    }
                    else
                    {
                        File.WriteAllText(StateInfoPatchFile, @byte + "\r\n");
                    }

                    // Разбитие на файлы
                    while (file.Position < file.Length - 1)
                    {
                        if (SearchIdFileBd((c + 1))) { file.Position += Math.Min((long)@byte, (long)(file.Length - file.Position)); if (massenge != null) massenge.Invoke("[Info] Эта часть была ранее загружена и успешно восстановлена", ConsoleColor.Green); c++; continue; }
                        // Добавляем в список информацию о названии части
                        DataSaveInfo dataSaveInfo = new DataSaveInfo { FileIdName = ++c, Password = random.Next(0, int.MaxValue - 1).ToString() };
                        List<byte> BtPart = new List<byte>(0);
                        // Запись данных в временный файл
                        for (long i = 0; i < count; i++)
                        {
                            byte[] vs = new byte[Math.Min((FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead), file.Length - file.Position)];
                            file.Read(vs, 0, vs.Length);
                            BtPart.AddRange(vs);
                        }
                        if (massenge != null) massenge.Invoke("[Info] Файл разбит", ConsoleColor.Blue);
                        while (Count >= MaxFilesUpload - 1) ; // Ждем своей очереди загрузки
                        Count++;
                        Task.Run(() => HendlerUpload(dataSaveInfo, functionAndSetting.Compress(BtPart.ToArray(), dataSaveInfo.Password), massenge));
                        //function.Compress();
                    }
                    file.Close();
                    while (Count > 0) ;// Ждём загрузку оставшихся файлов
                    uploadFileInfo.Parts.Sort(new ShortStruct()); // Сортировка бд
                    if (File.Exists(StateInfoPatchFile)) File.Delete(StateInfoPatchFile);
                    if (massenge != null) massenge.Invoke("[Info] Файл успешно загружен", ConsoleColor.Green);
                    return new ResulUploadFileInfo { UrlSave = functionAndSetting.Upload(functionAndSetting.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(uploadFileInfo)), Password), "info"), InfoSave = false };
                }
            }
            /// <summary>
            /// Проверка есть ли часть файла с {long id}
            /// </summary>
            /// <param name="id">id который нужно проверить</param>
            /// <returns>True если найден False если такого в списке нет</returns>
            private bool SearchIdFileBd(long id)
            {
                foreach (var Elem in uploadFileInfo.Parts)
                {
                    if (Elem.FileIdName == id)
                    {
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// Асинхронный загрузщик файлов в AnonFile
            /// </summary>
            /// <param name="dataSaveInfo">Информация о загружаемой части файла</param>
            /// <param name="vs">Файл в виде массива byte</param>
            /// <param name="massenge">delegate для логирования состояния</param>
            private void HendlerUpload(DataSaveInfo dataSaveInfo, byte[] vs, FunctionAndSetting.Massenge massenge = null)
            {
            restart:
                try
                {
                    // Записываем данные
                    dataSaveInfo.UrlFileID = functionAndSetting.Upload(vs, dataSaveInfo.FileIdName.ToString());
                    // Добавляем информацию в общию бд
                    uploadFileInfo.Parts.Add(dataSaveInfo);
                    // Уменьшаем значение загружаемых на данный момент файлов
                    Count--;
                    // Оповещание о загрузке файла
                    if (massenge != null) massenge.Invoke("Загружен файл с ID: " + dataSaveInfo.FileIdName, ConsoleColor.Gray);
                    dataSaveInfo.Size = vs.LongLength;
                restTwo:
                    try
                    {
                        File.AppendAllText(StateInfoPatchFile, JsonConvert.SerializeObject(dataSaveInfo) + "\r\n");
                    }
                    catch
                    {
                        goto restTwo;
                    }
                }
                catch (Exception e)
                {
                    if (massenge != null) massenge.Invoke(String.Format("Произошла ошибка {0} при загрузке осуществляю перезапуск {1}", e.Message, dataSaveInfo.FileIdName), ConsoleColor.Yellow);
                    goto restart;
                }
            }
            /// <summary>
            /// Информация о загруженном файле
            /// </summary>
            public struct ResulUploadFileInfo
            {
                /// <summary>
                /// Ссылка на сохранёный файл (или главный файл с инфой)
                /// </summary>
                public string UrlSave;
                /// <summary>
                /// Если true то ссылка ведёт на сам файл, если false то ссылка ведёт на файл с информациео о том где и как хранятся частии этого файла
                /// </summary>
                public bool InfoSave;
            }
        }
        /// <summary>
        /// Скачиване файла
        /// </summary>
        public class DownloadFile
        {
            //==================================================== То что можно переопределить
            /// <summary>
            /// Набор функций для данного класа некоторые из них можно переопределить
            /// </summary>
            public FunctionAndSetting functionAndSetting = new FunctionAndSetting();
            //==================================================== Данные необходимые для работы
            /// <summary>
            /// Бд файла содержит в себе ссылки на все части файла его название и.т.д (для более подробного ознакомления смотри структуру)
            /// </summary>
            private UploadFileInfo uploadFileInfo;
            private long OldIdSave = long.MinValue;
            /// <summary>
            /// Текущие количество загружеемых одновременно файлов
            /// </summary>
            private byte Count = 0;
            /// <summary>
            /// Максимальное число одновременно загружаемых файлов
            /// </summary>
            private byte MaxFilesUpload = 1;
            /// <summary>
            /// Итоговый загружаемый файл (его поток)
            /// </summary>
            private FileStream file;
            /// <summary>
            /// Временная папка для загрузки частей
            /// </summary>
            private string FolderPart;
            /// <summary>
            /// Количество уже загруженных файлов
            /// </summary>
            private int CountUploadetPart = 0;
            //==================================================== Методы
            public void DownloadHendler(string UrlFile, string PatchTo, string NameFile = null, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null)
            {
                if (!Directory.Exists(PatchTo)) { if (massenge != null) massenge.Invoke("[Error] Такой директории не существует", ConsoleColor.Red); return; }
                this.MaxFilesUpload = MaxFilesUpload;
                // Качаем главный файл
                byte[] FileHead = functionAndSetting.DeCompress(UploadDownloadWdho.Download.GetBytesFile(UrlFile), Password);
                try
                {
                    uploadFileInfo = JsonConvert.DeserializeObject<UploadFileInfo>(Encoding.UTF8.GetString(FileHead));
                    FolderPart = Path.Combine(PatchTo, uploadFileInfo.NameFale + functionAndSetting.GeyKeyFile(UrlFile) + ".Folder");
                reset:
                    try
                    {
                        List<string> IgnorePart = new List<string>(0);

                        if (Directory.Exists(FolderPart))
                        {
                            string[] allfiles = Directory.GetFiles(FolderPart);
                            foreach (var Elem in allfiles)
                            {
                                if (Elem.Contains(".saved"))
                                {
                                    string s = Path.GetFileName(Elem).Replace(".saved", null);
                                    IgnorePart.Add(s);
                                    OldIdSave = Math.Max(OldIdSave, Convert.ToInt64(s));
                                    CountUploadetPart++;
                                }
                            }
                        }
                        else { Directory.CreateDirectory(FolderPart); OldIdSave = long.MaxValue; }

                        if (File.Exists(Path.Combine(PatchTo, uploadFileInfo.NameFale)))
                            file = new FileStream(Path.Combine(PatchTo, uploadFileInfo.NameFale), FileMode.Append);
                        else
                            file = new FileStream(Path.Combine(PatchTo, uploadFileInfo.NameFale), FileMode.OpenOrCreate);

                        foreach (var Elem in uploadFileInfo.Parts)
                        {
                            if (IgnorePart.Contains(Elem.FileIdName.ToString())) { if (massenge != null) massenge.Invoke("[Info] Файл был ранее загружен", ConsoleColor.Green); continue; }
                            while (Count >= MaxFilesUpload) UploadPart(); // Ждем своей очереди загрузки
                            Count++;
                            Task.Run(() => DovnloadFile(Elem));
                        }
                        while (Count > 0 || CountUploadetPart != uploadFileInfo.Parts.Count) UploadPart();// Ждём загрузку оставшихся файлов
                        Directory.Delete(FolderPart, true);
                        file.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                catch
                {
                    File.WriteAllBytes(((NameFile == null) ? Path.Combine(PatchTo, functionAndSetting.GetNameFile(UrlFile)) : Path.Combine(PatchTo, NameFile)), FileHead);
                }
                if (massenge != null) massenge.Invoke("[Info] Файл успешно скачан", ConsoleColor.Green);
            }
            /// <summary>
            /// Обьединение загруженных частей
            /// </summary>
            private void UploadPart()
            {
                string s = Path.Combine(FolderPart, (OldIdSave + 1).ToString());
                if (File.Exists(s + ".saved") && File.Exists(s))
                {
                    byte[] vs = File.ReadAllBytes(s);
                    file.Write(vs, 0, vs.Length);
                    CountUploadetPart++;
                    File.Delete(s);
                    OldIdSave++;
                }
            }
            /// <summary>
            /// Асинхронный загрузщик частей
            /// </summary>
            /// <param name="dataSaveInfo">Информация о загружаемой части</param>
            private void DovnloadFile(DataSaveInfo dataSaveInfo)
            {
                File.WriteAllBytes(Path.Combine(FolderPart, dataSaveInfo.FileIdName.ToString()), functionAndSetting.DeCompress(functionAndSetting.GetFile(dataSaveInfo.UrlFileID), dataSaveInfo.Password));
                File.Create(Path.Combine(FolderPart, dataSaveInfo.FileIdName.ToString() + ".saved"));
                Count--;
            }
        }
        /// <summary>
        /// Некоторые функции необходмые для работы двух классов (UploadFile и DownloadFile)
        /// </summary>
        public class FunctionAndSetting
        {
            /// <summary>
            /// Делегат необходимый для логирования операции
            /// </summary>
            public delegate void Massenge(string Text, ConsoleColor Color);
            /// <summary>
            /// Переменная хранящая кол во byyte в одном мегабайте
            /// </summary>
            public const uint OneMb = 1048576;
            /// <summary>
            /// Название временной папки
            /// По умолчанию UploadDownload7TemporaryFolder
            /// </summary>
            public string NammeFolderData = "UploadDownload7TemporaryFolder";
            /// <summary>
            /// Максимальный размер одной части
            /// По умолчанию 25
            /// </summary>
            public uint MaxMbPart = 25;
            /// <summary>
            /// В мегабайтах. Какими кусочками читать загружаемый файл 
            /// По умолчанию 5
            /// </summary>
            public byte MaxFilePrtRead = 5;

            /// <summary>
            /// Шифровка массива паролем
            /// </summary>
            /// <param name="vs">Массив byte который необходимо зашифровать</param>
            /// <param name="Password">Пароль шифрующий массив</param>
            /// <returns>Массив зашифрованных byte</returns>
            public virtual byte[] Compress(byte[] vs, string Password = null)
            {
                if (Password == null) return vs;

                byte[] str = Encoding.Default.GetBytes(Password);

                for (long i = 0; i < vs.LongLength; i++)
                {
                    int NumPassElem = (int)(i + 1) % (str.Length + 1);
                    if (NumPassElem == 0)
                    {
                        vs[i] ^= str[str.Length - 1];
                    }
                    else
                    {
                        vs[i] ^= str[NumPassElem - 1];
                    }
                }
                return vs;
            }
            /// <summary>
            /// Расшифровка массива по паролю
            /// </summary>
            /// <param name="vs">Массив зашифрованных byte</param>
            /// <param name="Password">пароль для разшифровки</param>
            /// <returns>Расшифрованный массив byte</returns>
            public virtual byte[] DeCompress(byte[] vs, string Password = null)
            {
                if (Password == null) return vs;

                byte[] str = Encoding.Default.GetBytes(Password);

                for (long i = 0; i < vs.LongLength; i++)
                {
                    int NumPassElem = (int)(i + 1) % (str.Length + 1);
                    if (NumPassElem == 0)
                    {
                        vs[i] ^= str[str.Length - 1];
                    }
                    else
                    {
                        vs[i] ^= str[NumPassElem - 1];
                    }
                }
                return vs;
            }
            /// <summary>
            /// Загрузка файла
            /// Вы можете переопределить данный метод
            /// </summary>
            /// <param name="File">Мыссив byte файла</param>
            /// <param name="Name">Название файла</param>
            /// <returns>Ссылка на файл</returns>
            public virtual string Upload(byte[] File, string Name)
            {
                return UploadDownloadWdho.Upload.UploadHendler(File, Name).url;
            }
            /// <summary>
            /// Получение название файла по его ссылке
            /// Вы можете переопределить данный метод
            /// </summary>
            /// <param name="Url">Ссылка на файл</param>
            /// <returns>Название файла</returns>
            public virtual string GetNameFile(string Url)
            {
                return UploadDownloadWdho.Function.GetInfoFile(Url).NameFile;
            }
            /// <summary>
            /// Получение файла по его ссылке
            /// Вы можете переопределить данный файл
            /// </summary>
            /// <param name="Url">Ссылка нна файл</param>
            /// <returns>Файл в виде массмва byte</returns>
            public virtual byte[] GetFile(string Url)
            {
                return UploadDownloadWdho.Download.GetBytesFile(Url);
            }
            /// <summary>
            /// Получение пути к временной папке
            /// Если вы используете другую платформу (не Windows) вам необходимо переопределить данный метод
            /// </summary>
            /// <returns>Путь к временной папке</returns>
            public virtual string GetPatch()
            {
                return System.IO.Path.Combine(System.IO.Path.GetTempPath(), NammeFolderData);
            }
            /// <summary>
            /// Получение личного индификаторафайла из его ссылки
            /// </summary>
            /// <param name="Url">Ссылка на файл</param>
            /// <returns>Личный индификатор</returns>
            public virtual string GeyKeyFile(string Url)
            {
                return Url.Substring(Url.Replace('/', '\\').LastIndexOf('\\') + 1, Url.Length - Url.Replace('/', '\\').LastIndexOf('\\') - 1);
            }
        }
        /// <summary>
        /// Класс для сортировки бд
        /// </summary>
        public class ShortStruct : IComparer<DataSaveInfo>
        {
            int IComparer<DataSaveInfo>.Compare(DataSaveInfo x, DataSaveInfo y)
            {
                if (x.FileIdName > y.FileIdName)
                {
                    return 1;
                }
                else if (x.FileIdName < y.FileIdName)
                {
                    return -1;
                }
                return 0;
            }
        }
        /// <summary>
        /// Информация о загруженном одном файле
        /// </summary>
        public struct UploadFileInfo
        {
            /// <summary>
            /// Название файла
            /// </summary>
            public string NameFale;
            /// <summary>
            /// Информация о разбитых частях файла
            /// </summary>
            public List<DataSaveInfo> Parts;
            /// <summary>
            /// Если файл весит меньше Setting.MaxMbPart то он сразу сохраняется и ссылка на него тут хранится
            /// </summary>
            public string UrlFile;
        }
        /// <summary>
        /// Информация об одной части 
        /// </summary>
        public struct DataSaveInfo
        {
            /// <summary>
            /// Личный индификатор файла
            /// </summary>
            public long FileIdName;
            /// <summary>
            /// Пароль к файлу
            /// </summary>
            public string Password;
            /// <summary>
            /// Ссылка на файл
            /// </summary>
            public string UrlFileID;
            /// <summary>
            /// Размер части
            /// </summary>
            public long Size;
        }
    }
}