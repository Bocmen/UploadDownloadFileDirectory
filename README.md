# UploadDownloadFileDirectory
Зачем вам нужно данное решение?
я так полагаю для загрузки данных и их обмена между устройствами 
  
Вы можете загружать как файлы так и директории. Все данные хранятся на [wdho.ru](https://wdho.ru), но вы можете переобпределить некоторые методы и использовать другой ресурс для хранения файлов
  
## Загрузка
### Вы можете загружать отдельно файлы

// Создаём ээкземпляр класса для загрузки файла

UploadDownload7.Core.UploadFile HendlerUploadFileClass = new UploadDownload7.Core.UploadFile();

/// <summary>
/// Максимальное число одновременно загружаемых файлов
/// </summary>

HendlerUploadFileClass.UploadFileHendler(string PatchFile, byte MaxFilesUpload = 1, string Password = null, FunctionAndSetting.Massenge massenge = null);
