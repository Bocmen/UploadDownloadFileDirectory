# UploadDownloadFileDirectory
Зачем вам нужно данное решение?
я так полагаю для загрузки данных и их обмена между устройствами 
  
Вы можете загружать как файлы так и директории. Все данные хранятся на [wdho.ru](https://wdho.ru), но вы можете переобпределить некоторые методы и использовать другой ресурс для хранения файлов
  
## Загрузка
### Вы можете загружать отдельно файлы

Создаём ээкземпляр класса для загрузки файла

`UploadDownload7.Core.UploadFile HendlerUploadFileClass = new UploadDownload7.Core.UploadFile();`

Вызываем метод загрузки файла:
* <b>PatchFile</b> - путь к файлу
* MaxFilesUpload - максимальное число одновременно загружаемых на Сервер частей файла
* Password - пароль для шифрования главного файла (при загрузки больших файлов они разбиваются на части, эти части автоматически шифруются и все данные о частях хранятся в главном файле пароль которому задаётся пользователем. Если файл небольшой то пароль применяется к самому файлу)
* massenge - можно указать метод для логирования  Massenge(string Text, ConsoleColor Color) где string Text это строка самого сообщения, а ConsoleColor Color перечисление цветов содержащиеся в стандартной библиотеке System

`ResulUploadFileInfo InfoUpload = HendlerUploadFileClass.UploadFileHendler(string PatchFile, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null);`

В результате получаем ResulUploadFileInfo который содержит в себе:
* UrlSave - Ссылка на сохранёный файл (или главный файл с инфой)
* InfoSave - Если true то ссылка ведёт на сам файл, если false то ссылка ведёт на файл с информациео о том где и как хранятся частии этого файла
