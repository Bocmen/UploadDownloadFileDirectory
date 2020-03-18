/*
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
using System.Text;
using System.IO;
using System.Net;

namespace UploadDownloadWdho
{
    public static class Upload
    {

        /// <summary>
        /// Загрузка файла
        /// </summary>
        /// <param name="PatchFile">Путь к файллу на диске</param>
        /// <returns>Информация о загруженном файле</returns>
        public static InfoFile UploadFile(string PatchFile)
        {
            return UploadHendler(File.ReadAllBytes(PatchFile), PatchFile.Remove(0, PatchFile.Replace('\\', '/').LastIndexOf('/')));
        }
        /// <summary>
        /// Загрузчик файлов
        /// </summary>
        /// <param name="FileByte">Массив byte файла</param>
        /// <param name="NameFile">Название файла</param>
        /// <returns>Информация о загруженном файле</returns>
        public static InfoFile UploadHendler(byte[] FileByte, string NameFile)
        {
            DatKeys keys = GetKeysData();
            InfoFile infoFile = new InfoFile();
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>(0);
            keyValuePairs.Add(Setting.Search__sessionid, keys._sessionid);
            keyValuePairs.Add(Setting.Search_cTracker, keys.cTracke);
            keyValuePairs.Add(Setting.Search_maxChunkSize, keys.maxChunkSize);
            keyValuePairs.Add(Setting.Search_fileFolder, keys.fileFolder);
            keyValuePairs.Add("files[]", new FormUpload.FileParameter(FileByte, NameFile + ".txt", "application/octet-stream"));
            var Resul = FormUpload.MultipartFormDataPost(keys.Url, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0", keyValuePairs);
            string StrRes;
            using (StreamReader stream = new StreamReader(Resul.GetResponseStream(), Encoding.GetEncoding(Resul.CharacterSet)))
            {
                StrRes = System.Text.RegularExpressions.Regex.Unescape(stream.ReadToEnd());
            }
            infoFile.delete_url = ParsJson(StrRes, "delete_url");
            infoFile.error = ParsJson(StrRes, "error");
            infoFile.info_url = ParsJson(StrRes, "info_url");
            infoFile.name = ParsJson(StrRes, "name");
            if (infoFile.name.Contains('.')) infoFile.name = infoFile.name != null ? infoFile.name.Substring(0, infoFile.name.LastIndexOf('.')) : infoFile.name;
            infoFile.size = ParsJson(StrRes, "size");
            infoFile.stats_url = ParsJson(StrRes, "stats_url");
            infoFile.url = ParsJson(StrRes, "url");
            return infoFile;
        }
        /// <summary>
        /// В ответе у них Json содержит ошибки и не получается десериализовать поэтому было принято решения написать парсер значений
        /// </summary>
        /// <param name="Json">Json txt</param>
        /// <param name="Name">Название искомой переменной</param>
        /// <returns>значение переменной</returns>
        private static string ParsJson(string Json, string Name)
        {

            string resul = null;
            try
            {
                resul = Json.Remove(0, Json.IndexOf(Name));
                resul = resul.Remove(0, resul.IndexOf(":") + 1);
                int One = int.MaxValue, Two = int.MaxValue, Three = int.MaxValue;

                if (resul.Contains(',')) One = resul.IndexOf(',');
                if (resul.Contains(']')) Two = resul.IndexOf(']');
                if (resul.Contains('}')) Three = resul.IndexOf('}');

                resul = resul.Substring(0, Math.Min(Math.Min(One, Two), Three));

                if (resul.Contains("null")) return null;

                if (resul.Contains('\"'))
                {
                    resul = resul.Remove(0, resul.IndexOf('\"') + 1);
                    resul = resul.Substring(0, resul.IndexOf('\"'));
                    return resul;
                }
                // Эта часть проверки не подверглась
                return resul.Replace(" ", null).Replace("\r", null).Replace("\n", null).Replace("\t", null);
            }
            catch { }
            return resul;
        }
        /// <summary>
        /// Получение одноразовых ключей для загрузки файла
        /// </summary>
        /// <returns>Набор ключей</returns>
        private static DatKeys GetKeysData()
        {
            // Получения HTml главной станицы загрузки
            string Html = (new WebClient()).DownloadString(Setting.UrlHomePage); DatKeys datKeys = new DatKeys();
            // Поиск переменных
            // Поиск ссылки
            string Resul = Html.Remove(0, Html.IndexOf(Setting.SearchScript));
            Resul = Resul.Remove(Resul.IndexOf('\''), Resul.Length - Resul.IndexOf('\''));
            datKeys.Url = Resul;
            // Поиск название папки (обычно он null)
            Resul = Html.Remove(0, Html.IndexOf(Setting.Search_fileFolder));
            datKeys.fileFolder = Resul.Substring(Resul.IndexOf('\'') + 1, Resul.Remove(0, Resul.IndexOf('\'') + 1).IndexOf('\''));
            // Поиск maxChunkSize (хз для чего он)
            Resul = Html.Remove(0, Html.IndexOf(Setting.Search_browserXHR2Support));
            Resul = Resul.Remove(0, Resul.IndexOf(Setting.Search_maxChunkSize));
            Resul = Resul.Substring(Resul.IndexOf('=') + 1, Resul.IndexOf(';') - (Resul.IndexOf('=') + 1));
            datKeys.maxChunkSize = Convert.ToUInt32(Resul);
            // Поиск _sessionid (какой то личный индификатор "сессии")
            Resul = Html.Remove(0, Html.IndexOf(Setting.Search__sessionid));
            datKeys._sessionid = Resul.Substring(Resul.IndexOf('\'') + 1, Resul.Remove(0, Resul.IndexOf('\'') + 1).IndexOf('\''));
            // Поиск cTracke (какой то ещё один ключ)
            Resul = Html.Remove(0, Html.IndexOf(Setting.Search_cTracker));
            datKeys.cTracke = Resul.Substring(Resul.IndexOf('\'') + 1, Resul.Remove(0, Resul.IndexOf('\'') + 1).IndexOf('\''));
            return datKeys;
        }
        /// <summary>
        /// Структура для ключей
        /// </summary>
        private struct DatKeys
        {
            public string Url;
            public string _sessionid;
            public string cTracke;
            public uint maxChunkSize;
            public string fileFolder;
        }
    }
    public static class Download
    {
        /// <summary>
        /// Загрузка файла по псевдоссылке
        /// </summary>
        /// <param name="PatchSave">Куда сохранить</param>
        /// <param name="Url">Псевдоссылка на файл</param>
        public static void DownloadFile(string PatchSave, string Url)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(GetUrlFinal(Url), PatchSave);
        }
        /// <summary>
        /// Получить массив byte файла
        /// </summary>
        /// <param name="Url">Псевдоссылка на файл</param>
        /// <returns>массив byte файла</returns>
        public static byte[] GetBytesFile(string Url)
        {
            WebClient webClient = new WebClient();
            return webClient.DownloadData(GetUrlFinal(Url));
        }
        /// <summary>
        /// Получение ссылки на загрузку файла
        /// </summary>
        /// <param name="Url">Псевдоссылка на файл</param>
        /// <returns>ссылка на файл</returns>
        public static string GetUrlFinal(string Url)
        {
            string GetCookie = null;
            string Urli = GetUrlOne(Url, Url, ref GetCookie);
            string Resul = getHTML(Urli, ref GetCookie);
            Resul = Resul.Remove(0, Resul.IndexOf(Setting.Search_UrlFinal));
            Resul = Resul.Substring(0, Resul.IndexOf("\""));
            Resul = Url + "?" + Resul;
            return Resul;
        }
        /// <summary>
        /// Получения ссылки со страницей на которой есть ссылка на загрузку
        /// </summary>
        /// <param name="Url">Ссылка на страницу со сылкой</param>
        /// <param name="UrlAppend">Псевдоссылка на файл</param>
        /// <param name="Cookie">Записать Cookie которые будут обнаружены</param>
        /// <returns>Ссылка на сайт с финальной ссылкой загрузки</returns>
        private static string GetUrlOne(string Url, string UrlAppend, ref string Cookie)
        {
            string Resul = getHTML(Url, ref Cookie);
            Resul = Resul.Remove(0, Resul.IndexOf(String.Format(Setting.Search_UrlOne, UrlAppend)));
            Resul = Resul.Substring(0, Resul.IndexOf('\''));
            return Resul;
        }
        /// <summary>
        /// Специальный метод получения Html кода поддерживающий запись предыдущей страницы посещения 
        /// и Cookie которые автоматически записываются при их наличии
        /// </summary>
        /// <param name="url">Ссылка получения Html</param>
        /// <param name="Cookie">Cookie которые записываются или принимаются для использования</param>
        /// <param name="Refer">Предыдущая страница</param>
        /// <returns>HTML код страницы</returns>
        private static string getHTML(string url, ref string Cookie, string Refer = null)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.AllowAutoRedirect = false;//Запрещаем автоматический редирект
                httpWebRequest.Method = "GET"; //Можно не указывать, по умолчанию используется GET.

                httpWebRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0");
                httpWebRequest.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                httpWebRequest.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
                httpWebRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
                httpWebRequest.Headers.Add("Host", "wdho.ru");
                httpWebRequest.Headers.Add("Connection", "keep-alive");
                if (Cookie != null) httpWebRequest.Headers.Add("Cookie", Cookie);

                if (Refer != null) httpWebRequest.Referer = Refer;

                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var stream = httpWebResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, Encoding.GetEncoding(httpWebResponse.CharacterSet)))
                        {
                            Cookie = httpWebResponse.Headers["Set-Cookie"];
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
    public static class Setting
    {
        // Upload
        public const string UrlHomePage = @"https://wdho.ru/index.html?upload=1";
        public const string SearchScript = @"https://wdho.ru/core/page/ajax/file_upload_handler.ajax.php?r=wdho.ru";
        public const string Search__sessionid = @"_sessionid:";
        public const string Search_fileFolder = @"fileFolder";
        public const string Search_maxChunkSize = @"maxChunkSize";
        public const string Search_browserXHR2Support = @"browserXHR2Support";
        public const string Search_cTracker = @"cTracker";
        // Dovnload
        public const string Search_UrlOne = @"{0}?pt=";
        public const string Search_UrlFinal = @"download_token";
        // Function
        public const string Search_NameFile = @"heading-1";
    }
    /// <summary>
    /// Не моё решение
    /// Используется для составление формы отправки файла с доп. параметрами
    /// </summary>
    public static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, userAgent, contentType, formData);
        }
        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }
    }
    /// <summary>
    /// Информация о загруженном файле
    /// </summary>
    [Serializable]
    public class InfoFile
    {
        /// <summary>
        /// Название файла
        /// </summary>
        public string name;
        /// <summary>
        /// Размер файла
        /// </summary>
        public string size;
        /// <summary>
        /// Ошибки возникшие при загрузке
        /// </summary>
        public string error;
        /// <summary>
        /// Псевдоссылка на загрузку файла
        /// </summary>
        public string url;
        /// <summary>
        /// Псевдоссылка на удаление файла
        /// </summary>
        public string delete_url;
        /// <summary>
        /// Ссылка на страницу с информацией о файле
        /// </summary>
        public string info_url;
        /// <summary>
        /// Ссылка на статисктику файла
        /// </summary>
        public string stats_url;

        public override string ToString()
        {
            return String.Format("name: {0}\r\nsize: {1}\r\nerror: {2}\r\nurl: {3}\r\ndelete_url: {4}\r\ninfo_url: {5}\r\nstats_url: {6}", name, size, error, url, delete_url, info_url, stats_url);
        }
    }
    public static class Function
    {
        private static WebClient webClient = new WebClient();
        public static GetedInfoFile GetInfoFile(string Url)
        {
            GetedInfoFile infoFile = new GetedInfoFile();
            infoFile.stats_url = GetStatiUrl(Url);
            string Html = webClient.DownloadString(Url);
            // Получение имени
            infoFile.NameFile = Html.Remove(0, Html.IndexOf(Setting.Search_NameFile));
            infoFile.NameFile = infoFile.NameFile.Substring(infoFile.NameFile.IndexOf('>') + 1, infoFile.NameFile.IndexOf('<') - (infoFile.NameFile.IndexOf('>') + 1));
            infoFile.NameFile = infoFile.NameFile.Substring(0, infoFile.NameFile.LastIndexOf(".txt"));//Имя файла: Био.pptx.txt
            // Отсекаем ненужную часть Html
            Html = Html.Remove(0, Html.IndexOf("Размер файла:"));
            // Получаем размер файла
            if (Html.Contains("MB")) {
                infoFile.Size = Html.Substring(Html.IndexOf(":") + 1, Html.IndexOf("MB") - (Html.IndexOf(":") + 1)).Replace('.', ',');
            }
            else
            {
                infoFile.Size = Html.Substring(Html.IndexOf(":") + 1, Html.IndexOf("KB") - (Html.IndexOf(":") + 1)).Replace('.', ',');
            }
            // Получение даты загрузки фйла
            infoFile.UploadData = Html.Remove(0, Html.IndexOf("Файл загружен"));
            infoFile.UploadData = infoFile.UploadData.Substring(infoFile.UploadData.IndexOf(":") + 1, infoFile.UploadData.IndexOf("<") - (infoFile.UploadData.IndexOf(":") + 1));
            // Получения количества загрузок
            Html = Html.Remove(0, Html.IndexOf("Файл скачали"));
            infoFile.CountDownload = Convert.ToUInt32(Html.Substring(Html.IndexOf(':') + 1, Html.IndexOf('<') - 1 - Html.IndexOf(':')));
            return infoFile;
        }
        /// <summary>
        /// Получение ссылки на страницу со статистикой
        /// </summary>
        /// <param name="UrlFile">Ссылка на файл</param>
        /// <returns></returns>
        public static string GetStatiUrl(string UrlFile)
        {
            return UrlFile + "~s";
        }

        /// <summary>
        /// Получаемая информация о файле методом GetInfoFile по ссылке на файл
        /// </summary>
        public class GetedInfoFile
        {
            /// <summary>
            /// Название файла
            /// </summary>
            public string NameFile;
            /// <summary>
            /// Размер файла
            /// </summary>
            public string Size;
            /// <summary>
            /// Когда был загружен файл
            /// </summary>
            public string UploadData;
            /// <summary>
            /// Количесттво скачиваний
            /// </summary>
            public uint CountDownload;
            /// <summary>
            /// Ссылка на страницу со статистикой
            /// </summary>
            public string stats_url;

            public override string ToString()
            {
                return String.Format("NameFile: {0}\r\nSize: {1} MB\r\nUploadData: {2}\r\nCountDownload: {3}\r\nstats_url: {4}", NameFile, Size, UploadData, CountDownload, stats_url);
            }
        }
    }
}
