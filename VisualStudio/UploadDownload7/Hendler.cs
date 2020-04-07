/*
|
|
| vk.com      https://vk.com/denisivanov220
+---------------------------------------------- ProjectUrl https://github.com/Bocmen/UploadDownloadFileDirectory/tree/Ver-2
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
        /// Загрузка папок и файлов (Upload)
        /// </summary>
        public class UploadDirectory
        {
            /// <summary>
            /// Набор функций для данного класа некоторые из них можно переопределить
            /// </summary>
            private FunctionAndSetting functionAndSetting = new FunctionAndSetting();
            /// <summary>
            /// Необходима для рандомного пароля
            /// </summary>
            private static Random random = new Random();
            /// <summary>
            /// Временный файл содержащий в себе состояние загрузки
            /// </summary>
            private string TemporaryFileInfoSaved;


            public string UploadFolder(string Patch, string Password = null, byte MaxFilesUpload = 1, FunctionAndSetting.Massenge massenge = null)
            {
                BdDirectory Bd = new BdDirectory { UploadetFiles = new List<OneElemSaveDir>(0) };
                UploadFile uploadFile = new UploadFile(ref functionAndSetting);
                TemporaryFileInfoSaved = Path.Combine(functionAndSetting.GetPatch(), Path.GetFileName(Patch) + FunctionAndSetting.ExpansionUploadDirectory);
                // Преверяем если загружалась ли папка неудачно ранее
                return null;
            }

            /// <summary>
            /// Получение списка файлов и папок
            /// </summary>
            /// <param name="Patch"></param>
            /// <returns></returns>
            private List<string> GetFilesFolders(string Patch)
            {
                if (Directory.Exists(Patch))
                {
                    List<string> Dat = new List<string>();
                    // Получение список файлов
                    string[] all = Directory.GetFiles(Patch);
                    Dat.AddRange(all);
                    // Получение список папок
                    all = Directory.GetDirectories(Patch);
                    foreach (var Elem in all) Dat.AddRange(GetFilesFolders(Elem));
                    if (Dat.Count > 0)
                        return Dat;
                    else
                        return new List<string> { Patch };
                }
                else return new List<string> { Patch };
            }
        }
        /// <summary>
        /// Скачивание файла или директории с файлами (Download)
        /// </summary>
        public class DownloadDirectory
        {
            /// <summary>
            /// Набор функций для данного класа некоторые из них можно переопределить
            /// </summary>
            private FunctionAndSetting functionAndSetting = new FunctionAndSetting();
            /// <summary>
            /// Временный файл содержащий в себе состояние загрузки
            /// </summary>
            private string TemporaryFileInfoSaved;

            public void Download(string Patch, string UrlFile, string Password = null, byte MaxFilesUpload = 1, FunctionAndSetting.Massenge massenge = null)
            {
                if (!Directory.Exists(Patch)) { if (massenge != null) massenge.Invoke("[-] Неправильно указан путь к директории\r\n", ConsoleColor.Red); return; }
                // Качаем главный файл
                try
                {
                    BdDirectory bdDirectory = JsonConvert.DeserializeObject<BdDirectory>(Encoding.UTF8.GetString(functionAndSetting.DeCompress(functionAndSetting.Download(UrlFile), Password)));
                    if (bdDirectory.UploadetFiles.Count <= 0)
                    {
                        if (massenge != null) massenge.Invoke("[Error] Ошибка получения главного файла", ConsoleColor.Red);
                        return;
                    }
                    else
                    {
                        TemporaryFileInfoSaved = Path.Combine(Patch, functionAndSetting.GetKeyFile(UrlFile) + FunctionAndSetting.ExpansionDownloadDirectory);
                        List<string> maskDownload = new List<string>(0);
                        if (File.Exists(TemporaryFileInfoSaved))
                        {
                            string[] res = File.ReadAllText(TemporaryFileInfoSaved).Split("\r\n");
                            for (long i = 1; i < res.LongLength; i++) maskDownload.Add(res[i]);
                        }
                        else File.WriteAllText(TemporaryFileInfoSaved, null);
                        DownloadFile downloadFile = new DownloadFile(functionAndSetting);
                        foreach (var Elem in bdDirectory.UploadetFiles)
                        {
                            if (!maskDownload.Contains(Elem.ResulUploadFileInfo.UrlSave))
                            {
                                // Проверяем является запись пустой папкой
                                if (Elem.ResulUploadFileInfo.UrlSave == null)
                                {
                                    Directory.CreateDirectory((Elem.SaveDirectory != null && Elem.SaveDirectory.Length > 0 && Elem.SaveDirectory.Replace('/', '\\')[0] == '\\') ? Path.Combine(Patch, Elem.SaveDirectory.Remove(0, 1)) : Path.Combine(Patch, Elem.SaveDirectory));
                                }
                                else
                                {
                                    string PatchSave = GetPatchLocal(Elem.SaveDirectory) != null ? Path.Combine(Patch, GetPatchLocal(Elem.SaveDirectory)) : Patch;
                                    Directory.CreateDirectory(PatchSave);
                                    downloadFile.DownloadHendler(Elem.ResulUploadFileInfo.UrlSave, PatchSave, Path.GetFileName(Elem.SaveDirectory), MaxFilesUpload, Elem.Password, massenge);
                                }
                            resSevInfo: try { File.AppendAllText(TemporaryFileInfoSaved, "\r\n" + Elem.ResulUploadFileInfo.UrlSave); } catch { goto resSevInfo; }
                            }
                        }
                    }
                resDel: try { if (File.Exists(TemporaryFileInfoSaved)) File.Delete(TemporaryFileInfoSaved); } catch { goto resDel; }
                }
                catch (Exception e)
                {
                    if (massenge != null) massenge.Invoke("[Error] Ошибка получения главного файла", ConsoleColor.Red);
                    return;
                }
            }

            private string GetPatchLocal(string Patch)
            {
                if (!(Patch.Contains('\\') || Patch.Contains('/')))
                {
                    return null;
                }
                else
                {
                    Patch = Patch.Replace('/', '\\');
                    string s = Patch.Substring(0, Patch.LastIndexOf('\\'));
                    s = s == "" ? null : s;
                    if (s != null && s.Length >= 0 && s[0] == '\\') s = s.Remove(0, 1);
                    return s == "" ? null : s;
                }
            }
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
            private FunctionAndSetting functionAndSetting = new FunctionAndSetting();
            /// <summary>
            /// Необходима для рандомного пароля
            /// </summary>
            private static Random random = new Random();
            /// <summary>
            /// Класс для сохранения состояния загрузки
            /// </summary>
            public Loget loget;
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
            //==================================================== Инцилизация
            public UploadFile(Loget loget) { this.loget = loget; functionAndSetting = loget.function; }
            public UploadFile(FunctionAndSetting functionAndSetting, Loget loget) { this.functionAndSetting = functionAndSetting; this.loget = loget; this.loget.function = this.functionAndSetting; }
            public UploadFile(ref FunctionAndSetting functionAndSetting, ref Loget loget) { this.functionAndSetting = functionAndSetting; loget = new Loget(functionAndSetting); }
            public UploadFile(FunctionAndSetting functionAndSetting) { this.functionAndSetting = functionAndSetting; loget = new Loget(functionAndSetting); }
            public UploadFile(ref FunctionAndSetting functionAndSetting) { this.functionAndSetting = functionAndSetting; loget = new Loget(functionAndSetting); }
            public UploadFile() { loget = new Loget(functionAndSetting); }
            //==================================================== Методы
            public ResulUploadFileInfo UploadFileHendler(string PatchFile, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null)
            {
                if (!File.Exists(PatchFile)) { if (massenge != null) massenge.Invoke("[Error] Такого пути к файлу не существует", ConsoleColor.Red); return new ResulUploadFileInfo(); }
                this.MaxFilesUpload = MaxFilesUpload;
                Count = 0;
                uploadFileInfo = new UploadFileInfo { NameFale = Path.GetFileName(PatchFile), Parts = new List<DataSaveInfo>(0) };
                // Проверяем если файл меньше одной части то сразу он сохраняется
                if ((new System.IO.FileInfo(PatchFile)).Length <= (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart))
                {
                    if (massenge != null) massenge.Invoke("[Info] Файл не будет разбит на несколько частей", ConsoleColor.Blue);
                    return new ResulUploadFileInfo { UrlSave = UploadDownloadWdho.Upload.UploadHendler(functionAndSetting.Compress(File.ReadAllBytes(PatchFile), Password), Path.GetFileName(PatchFile)).url, InfoSave = true };
                }
                else
                {
                    //  loget.GenereteName(Path.GetFileName(PatchFile), 30, Path.Combine(Path.GetDirectoryName(PatchFile), Path.GetFileName(PatchFile) + FunctionAndSetting.ExpansionUpload));
                    // Запуск потока чтения основного файла который будет разбит
                    FileStream file = new FileStream(PatchFile, FileMode.Open);
                    // Необходимо чтобы индифицировать части
                    long c = long.MaxValue;
                    // Получаем сколько будет весить одна часть в байтах
                    ulong @byte = (ulong)functionAndSetting.MaxMbPart * FunctionAndSetting.OneMb;
                    // Сколько раз нужно пройтись по файлу чтобы считать одну часть
                    long count = (long)((@byte % (FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead) > 0) ? (long)(@byte / (FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead)) + 1 : (long)(@byte / (FunctionAndSetting.OneMb * functionAndSetting.MaxFilePrtRead)));
                    if (massenge != null) massenge.Invoke(String.Format("[Info] Файл будет разбит на {0} частей", (file.Length % (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart) > 0 ? file.Length / (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart) + 1 : file.Length / (FunctionAndSetting.OneMb * functionAndSetting.MaxMbPart))), ConsoleColor.Blue);
                    // Проверяем какие файлы были рание загруженны
                    if (loget.SearchName(Path.GetFileName(PatchFile), FunctionAndSetting.TimeLiveFilesLog, Path.Combine(Path.GetDirectoryName(PatchFile), Path.GetFileName(PatchFile) + FunctionAndSetting.ExpansionUpload)))
                    {
                    resUpdTimeLog: if (!loget.UpdTime()) goto resUpdTimeLog;
                        string[] PartInfo = loget.GetListData();
                        if (PartInfo[0] == @byte.ToString())
                        {
                            for (long i = 1; i < PartInfo.LongLength; i++)
                            {
                                uploadFileInfo.Parts.Add(JsonConvert.DeserializeObject<DataSaveInfo>(PartInfo[i]));
                                if (massenge != null) massenge.Invoke("[Info] Ранее эта часть была загружена восстанавливаю данные", ConsoleColor.Green);
                            }
                        }
                    }
                    else
                    {
                        loget.GenereteName(Path.GetFileName(PatchFile), FunctionAndSetting.TimeLiveFilesLog, Path.Combine(Path.GetDirectoryName(PatchFile), Path.GetFileName(PatchFile) + FunctionAndSetting.ExpansionUpload));
                        loget.AddData(@byte.ToString());
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
                    }
                    file.Close();
                    while (Count > 0) ;// Ждём загрузку оставшихся файлов
                    uploadFileInfo.Parts.Sort(new ShortStruct()); // Сортировка бд
                    loget.Delite();
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
                    dataSaveInfo.Size = vs.LongLength;
                    // Добавляем информацию в общию бд
                    uploadFileInfo.Parts.Add(dataSaveInfo);
                restTwo: if (!loget.AddData(JsonConvert.SerializeObject(dataSaveInfo))) goto restTwo;
                    // Уменьшаем значение загружаемых на данный момент файлов
                    Count--;
                    // Оповещание о загрузке файла
                    if (massenge != null) massenge.Invoke("Загружен файл с ID: " + dataSaveInfo.FileIdName, ConsoleColor.Gray);
                }
                catch (Exception e)
                {
                    if (massenge != null) massenge.Invoke(String.Format("Произошла ошибка {0} при загрузке осуществляю перезапуск {1}", e.Message, dataSaveInfo.FileIdName), ConsoleColor.Yellow);
                    goto restart;
                }
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
            private FunctionAndSetting functionAndSetting = new FunctionAndSetting();
            //==================================================== Данные необходимые для работы
            /// <summary>
            /// Бд файла содержит в себе ссылки на все части файла его название и.т.д (для более подробного ознакомления смотри структуру)
            /// </summary>
            private UploadFileInfo uploadFileInfo;
            /// <summary>
            /// Предыдущий Id части файла
            /// </summary>
            private long OldIdSave = long.MaxValue;
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
            /// Класс для сохранения состояния загрузки
            /// </summary>
            public Loget loget;
            ///=================================================== Инцилизация
            public DownloadFile(FunctionAndSetting functionAndSetting) { this.functionAndSetting = functionAndSetting; loget = new Loget(ref this.functionAndSetting); }
            public DownloadFile(ref FunctionAndSetting functionAndSetting) { this.functionAndSetting = functionAndSetting; loget = new Loget(ref this.functionAndSetting); }
            public DownloadFile(FunctionAndSetting functionAndSetting, Loget loget) { this.functionAndSetting = functionAndSetting; loget = new Loget(ref this.functionAndSetting); }
            public DownloadFile(ref FunctionAndSetting functionAndSetting, ref Loget loget) { this.functionAndSetting = functionAndSetting; loget = new Loget(ref this.functionAndSetting); }
            public DownloadFile() { loget = new Loget(ref functionAndSetting); }
            //==================================================== Методы
            public void DownloadHendler(string UrlFile, string PatchTo, string NameFile = null, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null)
            {
                if (!Directory.Exists(PatchTo)) { if (massenge != null) massenge.Invoke("[Error] Такой директории не существует", ConsoleColor.Red); return; }
                this.MaxFilesUpload = MaxFilesUpload;
                Count = 0;
                // Качаем главный файл
                byte[] FileHead = functionAndSetting.DeCompress(UploadDownloadWdho.Download.GetBytesFile(UrlFile), Password);
                try
                {
                    uploadFileInfo = JsonConvert.DeserializeObject<UploadFileInfo>(Encoding.UTF8.GetString(FileHead));
                    try
                    {
                        List<string> IgnorePart = new List<string>(0);
                        if (loget.SearchName(uploadFileInfo.NameFale + functionAndSetting.GetKeyFile(UrlFile) + FunctionAndSetting.ExpansionDownload, FunctionAndSetting.TimeLiveFilesLog, Path.Combine(PatchTo, uploadFileInfo.NameFale + functionAndSetting.GetKeyFile(UrlFile) + FunctionAndSetting.ExpansionDownload)))
                        {
                            string[] allfiles = Directory.GetFiles(loget.GetUrlSaveData());
                            foreach (var Elem in allfiles)
                            {
                                string s = Path.GetFileName(Elem).Replace(FunctionAndSetting.ExpansionDownload, null);
                                IgnorePart.Add(s);
                                if (!File.Exists(Path.Combine(loget.GetUrlSaveData(), s))) OldIdSave = Math.Max(OldIdSave, Convert.ToInt64(s));
                            }
                        }
                        else
                        {
                            loget.GenereteName(uploadFileInfo.NameFale + functionAndSetting.GetKeyFile(UrlFile) + FunctionAndSetting.ExpansionDownload, FunctionAndSetting.TimeLiveFilesLog, Path.Combine(PatchTo, uploadFileInfo.NameFale + functionAndSetting.GetKeyFile(UrlFile) + FunctionAndSetting.ExpansionDownload));
                        }

                        file = new FileStream(Path.Combine(PatchTo, uploadFileInfo.NameFale), FileMode.OpenOrCreate);
                        
                        foreach (var Elem in uploadFileInfo.Parts)
                        {
                            if (IgnorePart.Contains(Elem.FileIdName.ToString())) { if (massenge != null) massenge.Invoke("[Info] Файл был ранее загружен", ConsoleColor.Green); continue; }
                            while (Count >= MaxFilesUpload) ; // Ждем своей очереди загрузки
                            Count++;
                            Task.Run(() => DovnloadFile(Elem));
                        }
                        while (Count > 0) ;// Ждём загрузку оставшихся файлов
                        UploadPart();
                        file.Close();
                    restDel: if (!loget.Delite()) goto restDel;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                catch (Exception e)
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
                string s = Path.Combine(loget.GetUrlSaveData(), (OldIdSave + 1).ToString());
                if (File.Exists(s))
                {
                    try
                    {
                        byte[] vs = File.ReadAllBytes(s);
                        file.Write(vs, 0, vs.Length);
                        File.Delete(s);
                        OldIdSave++;
                    }
                    catch { }
                    UploadPart();
                }
            }
            /// <summary>
            /// Асинхронный загрузщик частей
            /// </summary>
            /// <param name="dataSaveInfo">Информация о загружаемой части</param>
            private void DovnloadFile(DataSaveInfo dataSaveInfo)
            {
                File.WriteAllBytes(Path.Combine(loget.GetUrlSaveData(), dataSaveInfo.FileIdName.ToString()), functionAndSetting.DeCompress(functionAndSetting.Download(dataSaveInfo.UrlFileID), dataSaveInfo.Password));
                Count--;
            }
        }
        /// <summary>
        /// Некоторые функции необходмые для работы двух классов (UploadFile и DownloadFile)
        /// </summary>
        public class FunctionAndSetting
        {
            //======================================================================== Константы
            /// <summary>
            /// Переменная хранящая кол во byyte в одном мегабайте
            /// </summary>
            public const uint OneMb = 1048576;
            /// <summary>
            /// Расширение файла с инфо о состоянии загрузки (Upload)
            /// </summary>
            public const string ExpansionUpload = ".logUploadFile";
            /// <summary>
            /// Расширегнеие файла и директории с инфо о состоянии загрузки (Download)
            /// </summary>
            public const string ExpansionDownload = ".logDownload";
            /// <summary>
            /// Расширегнеие файла с инфо о состоянии загрузки директории (UploadDirectory)
            /// </summary>
            public const string ExpansionUploadDirectory = ".lofInfoUploadDir";
            /// <summary>
            /// Расширегнеие файла с инфо о состоянии загрузки директории (DownloadDirectory)
            /// </summary>
            public const string ExpansionDownloadDirectory = ".lofInfoDownloadDir";
            /// <summary>
            /// Время жизни временных файлов
            /// </summary>
            public const ulong TimeLiveFilesLog = 20; // минут
            //======================================================================== Переменные
            /// <summary>
            /// Для генерации рандомной строки
            /// </summary>
            private static Random random = new Random();
            /// <summary>
            /// Делегат необходимый для логирования операции
            /// </summary>
            public delegate void Massenge(string Text, ConsoleColor Color);
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
            //======================================================================== Методы
            /// <summary>
            /// Генерация рандомной строки
            /// </summary>
            /// <param name="Leng">Длина строки</param>
            /// <param name="RandChars">Из каких символов она будет состоять</param>
            /// <returns></returns>
            public virtual string GenerateRandomString(byte Leng = 5, string RandChars = "qQwWeErRtTyYuUiIoOpPaAsSdDfFgGhHjJkKlLzZxXcCvVbBnNmM1234567890")
            {
                if (RandChars == null || RandChars == "") return null;
                string res = "";
                for (byte i = 0; i < Leng; i++)
                {
                    res += RandChars[random.Next(0, RandChars.Length)];
                }
                return res;
            }
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
            public virtual byte[] Download(string Url)
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
            public virtual string GetKeyFile(string Url)
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
        /// Данный класс служит средством для записи текущего состояния загрузки
        /// </summary>
        public class Loget
        {
            public FunctionAndSetting function;
            private string Name;
            private const string expansion = ".log";

            /// <summary>
            /// Инцилизация логов
            /// </summary>
            /// <param name="Name">что то типо личного ключа</param>
            public Loget()
            {
                function = new FunctionAndSetting();
            }
            /// <summary>
            /// Инцилизация логов
            /// </summary>
            /// <param name="Name">что то типо личного ключа</param>
            public Loget(ref FunctionAndSetting function)
            {
                this.function = function;
            }
            /// <summary>
            /// Инцилизация логов
            /// </summary>
            /// <param name="Name">что то типо личного ключа</param>
            public Loget(FunctionAndSetting function)
            {
                this.function = function;
            }
            /// <summary>
            /// Создание временного места хранения логов
            /// </summary>
            /// <param name="Name"></param>
            /// <returns></returns>
            public virtual string GenereteName(string Name = null, ulong DeliteMinute = 20, string Check = null)
            {
                string Patch = Path.Combine(function.GetPatch(), Name);
                if (!Directory.Exists(Patch))
                {
                    Directory.CreateDirectory(Patch);
                    File.WriteAllText(Path.Combine(Patch, "DateTimeInfo.txt"), DateTime.Now.ToString("HH-mm-ss-dd-MM-yyyy") + "\r\n###" + DeliteMinute + "\r\n###" + Name + "\r\n###" + Check);
                    this.Name = Patch;
                    return Patch;
                }
            Res:
                Patch = Path.Combine(function.GetPatch(), string.Format("{0}DEN{1}", function.GenerateRandomString(), Name));
                if (!Directory.Exists(Patch))
                {
                    Directory.CreateDirectory(Patch);
                    File.WriteAllText(Path.Combine(Patch, "DateTimeInfo.txt"), DateTime.Now.ToString("HH-mm-ss-dd-MM-yyyy") + "\r\n###" + DeliteMinute + "\r\n###" + Name + "\r\n###" + Check);
                    this.Name = Patch;
                    return Patch;
                }
                else goto Res;
            }
            /// <summary>
            /// Удаление данных
            /// </summary>
            /// <returns></returns>
            public virtual bool Delite()
            {
                try
                {
                    Directory.Delete(Name, true);
                    return true;
                }
                catch { return false; }
            }
            /// <summary>
            /// Добавление новых данных к существующим
            /// </summary>
            /// <param name="Data">Данные</param>
            /// <param name="Key">Ключ</param>
            /// <returns></returns>
            public virtual bool AddData(string Data, string Key = "null")
            {
                try
                {
                    CheckAndAddNewKey(Key);
                    File.AppendAllText(Path.Combine(Name, Key + expansion), "\r\n#: " + Data);
                    return true;
                }
                catch { return false; }
            }
            /// <summary>
            /// Запись данных (перезаписывает имеющиеся)
            /// </summary>
            /// <param name="Data">Данные</param>
            /// <param name="Key">Ключ</param>
            /// <returns></returns>
            public virtual bool SetDAta(string Data, string Key = "null")
            {
                try
                {
                    CheckAndAddNewKey(Key);
                    File.AppendAllText(Path.Combine(Name, Key + expansion), Data);
                    return true;
                }
                catch { return false; }
            }
            /// <summary>
            /// Проверяет есть ли такой ключ и при его отсутствии создаёт его
            /// </summary>
            /// <param name="Key">Ключ</param>
            /// <returns></returns>
            public virtual void CheckAndAddNewKey(string Key)
            {
                if (!CheckKey(Key)) File.WriteAllText(Path.Combine(Name, Key + expansion), null);
            }
            /// <summary>
            /// Получение данных
            /// </summary>
            /// <param name="Key"></param>
            /// <returns></returns>
            public virtual string[] GetListData(string Key = "null")
            {
                if (!CheckKey(Key)) return null;
                try
                {
                    string fl = File.ReadAllText(Path.Combine(Name, Key + expansion));
                    List<string> res = new List<string>();
                    string[] s = fl.Split("\r\n");
                    foreach (var elem in s)
                    {
                        if (elem.Contains("#: ") && elem.IndexOf("#: ") == 0)
                        {
                            res.Add(elem.Remove(0, 3));
                        }
                    }
                    return res.ToArray();
                }
                catch { return null; }
            }
            /// <summary>
            /// Проверка есть ли такой ключ True - Да, False - Нет
            /// </summary>
            /// <param name="Key"></param>
            /// <returns></returns>
            public virtual bool CheckKey(string Key)
            {
                if (!File.Exists(Path.Combine(Name, Key + expansion))) return false;
                return true;
            }
            /// <summary>
            /// Поиск элемента записей
            /// </summary>
            /// <param name="Name">Имя записи</param>
            /// <param name="Minute">Мах время прошедшие с момента создания записи</param>
            /// <param name="Check">Проверка доп ключа</param>
            public virtual bool SearchName(string Name, double Minute, string Check = "Not CheckContent")
            {
                string[] Dirs = Directory.GetDirectories(function.GetPatch());
                double TotalMinutes = long.MaxValue;
                bool resulb = false;
                foreach (var elem in Dirs)
                {
                    if (!File.Exists(Path.Combine(elem, "DateTimeInfo.txt")))
                    {
                        try
                        {
                            Directory.Delete(elem, true);
                        }
                        catch { }
                        continue;
                    }
                    string[] inf = File.ReadAllText(Path.Combine(elem, "DateTimeInfo.txt")).Split("\r\n###");
                    if (inf.Length == 4)
                    {
                        string[] dt = inf[0].Split('-');
                        DateTime dateTime = new DateTime(
                            Convert.ToInt32(dt[5]),
                            Convert.ToInt32(dt[4]),
                            Convert.ToInt32(dt[3]),
                            Convert.ToInt32(dt[0]),
                            Convert.ToInt32(dt[1]),
                            Convert.ToInt32(dt[2])
                            );
                        double minute = (DateTime.Now - dateTime).TotalMinutes;
                        if (minute <= Convert.ToInt64(inf[1]))
                        {
                            if (minute < Minute)
                            {
                                if (inf[2] == Name && (minute < TotalMinutes && Check != "Not CheckContent" ? inf[3] == Check : true))
                                {
                                    TotalMinutes = minute;
                                    this.Name = elem;
                                    resulb = true;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                Directory.Delete(elem, true);
                            }
                            catch { }
                        }
                    }
                }
                return resulb;
            }
            /// <summary>
            /// Обновление времени создания
            /// </summary>
            /// <returns></returns>
            public virtual bool UpdTime()
            {
                try
                {
                    string text = File.ReadAllText(Path.Combine(Path.Combine(function.GetPatch(), Name), "DateTimeInfo.txt"));
                    File.WriteAllText(
                        Path.Combine(Path.Combine(function.GetPatch(), Name), "DateTimeInfo.txt"),
                        DateTime.Now.ToString("HH-mm-ss-dd-MM-yyyy") + text.Remove(0, text.IndexOf("\r\n###"))
                        );
                    return true;
                }
                catch (Exception e) { return false; }
            }
            /// <summary>
            /// Получение временной папки где можно хранить файлы
            /// </summary>
            /// <returns></returns>
            public virtual string GetUrlSaveData()
            {
                string res = Path.Combine(Name, "FolderData");
                if (!Directory.Exists(res)) Directory.CreateDirectory(res);
                return res;
            }
        }
        //========================================================================= Структуры
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
        /// <summary>
        /// Информация о загруженном файле
        /// </summary>
        public struct OneElemSaveDir
        {
            /// <summary>
            /// Пароль к файлу
            /// </summary>
            public string Password;
            /// <summary>
            /// Информация о загруженной части (ссылка на неё и способ загрузки)
            /// </summary>
            public ResulUploadFileInfo ResulUploadFileInfo;
            /// <summary>
            /// Локальный путь к файлу
            /// </summary>
            public string SaveDirectory;
        }
        /// <summary>
        /// Структура для сохранения целой директории
        /// </summary>
        public struct BdDirectory
        {
            public List<OneElemSaveDir> UploadetFiles;
        }
    }
}