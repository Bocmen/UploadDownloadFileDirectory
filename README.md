# Сохранение загрузка файлов или целых директорий
Зачем вам нужно данное решение?
я так полагаю для загрузки данных и их обмена между устройствами 
  
Вы можете загружать как файлы так и директории. Все данные хранятся на [wdho.ru](https://wdho.ru), но вы можете переопределить некоторые методы и использовать другой ресурс для хранения файлов
  
## Загрузка (Upload)
### Вы можете загружать отдельно файлы

Создаём ээкземпляр класса для загрузки файла

`UploadDownload7.Core.UploadFile HendlerUploadFileClass = new UploadDownload7.Core.UploadFile();`

Вызываем метод загрузки файла:
* <b>PatchFile</b> - путь к файлу
* <b>MaxFilesUpload</b> - максимальное число одновременно загружаемых на сервер частей файла
* <b>Password</b> - пароль для шифрования главного файла (при загрузки больших файлов они разбиваются на части, эти части автоматически шифруются и все данные о частях хранятся в главном файле пароль которому задаётся пользователем. Если файл небольшой то пароль применяется к самому файлу)
* <b>massenge</b> - можно указать метод для логирования  `Massenge(string Text, ConsoleColor Color)` где `string Text` это строка самого сообщения, а `ConsoleColor Color` перечисление цветов содержащиеся в стандартной библиотеке System

`ResulUploadFileInfo InfoUpload = HendlerUploadFileClass.UploadFileHendler(string PatchFile, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null);`

В результате получаем <b>ResulUploadFileInfo</b> который содержит в себе:
* <b>UrlSave</b> - Ссылка на сохранёный файл (или главный файл с инфой)
* <b>InfoSave</b> - Если true то ссылка ведёт на сам файл, если false то ссылка ведёт на файл с информациео о том где и как хранятся частии этого файла

### Вы можете загружать отдельно директории (в разработке)

## Сохранение (Download)
### Вы можете сохранять отдельно файлы

Создаём ээкземпляр класса для сохранения файла

`UploadDownload7.Core.DownloadFile HendlerDownloadFileClass = new UploadDownload7.Core.DownloadFile();`

Вызываем метод сохранения файла:
* <b>UrlFile</b> - ссылка выданная ранее при загрузке файла (ResulUploadFileInfo.UrlSave)
* <b>PatchTo</b> - директория куда сохранить файл
* <b>NameFile</b> - название файла вместе с раширением. К примеру Exemple.txt (Не обязаттельный параметр, но он ускоряет работу поскольку приложению не надо отправлять запрос на сервер для получения имени файла)
* <b>MaxFilesUpload</b> - максимальное число одновременно загружаемых на сервер частей файла
* <b>Password</b> - пароль для рашифровки файла
* <b>massenge</b> - можно указать метод для логирования  `Massenge(string Text, ConsoleColor Color)` где `string Text` это строка самого сообщения, а `ConsoleColor Color` перечисление цветов содержащиеся в стандартной библиотеке System

`HendlerDownloadFileClass.DownloadHendler(string UrlFile, string PatchTo, string NameFile = null, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null);`

### Вы можете сохранять отдельно директории (в разработке)

## Немного про сервис хранения информации

Как говорилось информация хрантсся на сервисе [wdho.ru](https://wdho.ru), для рабоы с ним используется набор инсттрументов реализованный ранее [Посмотреть набор инструментов](https://github.com/Bocmen/UploadDownloadHendler_wdho.ru)

Если необходимо использовать другой сервис, то классы `UploadFile`, `DownloadFile` содержат в себе переменную `functionAndSetting` в которую помещается при инцилизации экземпляр класса `FunctionAndSetting` в котором вы можете переопределить такие методы как:
* `public virtual string Upload(byte[] File, string Name)`
  * Метод загружающий массив byte[] на сервис и возвращает ссылку на загруженный файл
* `public virtual string GetNameFile(string Url)`
  * Метод получающий имя файла по его ссылке
* `public virtual byte[] GetFile(string Url)`
  * Метод скачивающий файл в виде массива byte[] по его ссылке
* `public virtual string GeyKeyFile(string Url)`
  * Метод получабщий личный индификотор файла по его ссылке
