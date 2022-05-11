using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Serilog;

namespace MyFTPSolution2
{
    /// <summary>
    /// Класс, отвечающий за взаимодействие с FTP-сервером. 
    /// </summary>
    public static class FTPActions
    {
        public delegate void Message(string text);      // Определяем делегат для вывода сообщений.
        private static Message mes      = Text;         // Инициализируем делегат методом, выводящим сообщение на консоль.
        private static FtpWebRequest    request;        // Создаём переменную для запроса к серверу,
        private static FtpWebResponse   response;       // и переменную для ответа.
        private static Stream           responseStream; // Stream и StreamReader используются 
        private static StreamReader     reader;         // в нескольких методах, поэтому выносим 
                                                        // их в класс. 
        private static string Login;                    // Определяем поля для логина и
        private static string Password;                 // пароля, по которым мы будем авторизоваться
                                                        // на FTP-сервере. 

        public static string HostPath { get; private set; } // создаём свойство для адреса FTP-сервера


        /// <summary>
        /// Конструктор инциализирует адрес FTP-сервера, 
        /// и устанавливает логгер. 
        /// </summary>
        static FTPActions() 
        { 
            HostPath = "ftp://192.168.0.103";
            Login = "root";
            Password = "toor";

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File($"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}" +
            @"\logs.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }

        /// <summary>
        /// Метод, позволяющий выводить сообщения на консоль.
        /// </summary>
        /// <param name="message">Принимает в качестве аргумента строку,
        /// которую нужно вывести на консоль.</param>
        public static void Text(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Метод, позволяющий получить детальный список содержимого FTP-сервера. 
        /// </summary>
        public static void ShowFiles()
        {
            try
            {

                request = (FtpWebRequest)WebRequest.Create(HostPath);
                Log.Information($"Создан FTPWeb-реквест по адресу: {HostPath}");

                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                Log.Information("Создана команда, которая будет отправлена на сервер.");

                request.Credentials = new NetworkCredential(Login, Password);
                Log.Information("Пройдена авторизация на сервер.");

                response = (FtpWebResponse)request.GetResponse();
                Log.Information("Получен ответ от сервера.");

                responseStream = response.GetResponseStream();
                Log.Information("Получен поток с сервера.");
                reader = new StreamReader(responseStream);
                Log.Information("Запущено чтение потока.");

                string content = reader.ReadToEnd();
                Log.Information("Поток преобразован в строку");
                mes(content);

                mes($"Список файлов и директорий получен, " +
                            $"статус FTP-сервера: {response.StatusDescription}");
                Log.Information("Список файлов и директорий получен.");

                reader.Close();
                Log.Information("Поток закрыт.");
                response.Close();
                Log.Information("Соединение с сервером закрыто.");
            }
            catch (WebException ex)
            {

                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
        }

        /// <summary>
        /// Приватный метод для получения 
        /// содержимого FTP-сервера или папки на FTP-сервере. 
        /// Используется для получения коротких имён содержимого. 
        /// </summary>
        /// <param name="pathFrom">В качестве аргумента принимает адрес, 
        /// по которому необходимо найти файлы и/или папки. </param>
        /// <returns>Строка с коротким названием файлов и/или папок. </returns>
        private static string GetFiles(string pathFrom)
        {
            string content = "";
            try
            {
                Log.Information($"Запущен метод получения файлов по адресу: {pathFrom}");
                request = (FtpWebRequest)WebRequest.Create(pathFrom); // создаём запрос к FTP-серверу по адресу

                request.Method = WebRequestMethods.Ftp.ListDirectory; // определяем метод для обращения к серверу

                request.Credentials = new NetworkCredential(Login, Password); // авторизуемся на сервере

                response = (FtpWebResponse)request.GetResponse(); // получаем ответ от сервера

                responseStream = response.GetResponseStream(); // получаем поток ответа
                reader = new StreamReader(responseStream); // читаем поток

                content = reader.ReadToEnd(); // записываем полученый ответ в строку. 

                Log.Information($"Список файлов и директорий получен, " +
                                $"статус FTP-сервера: {response.StatusDescription}");
                reader.Close();
                response.Close(); 
            }
            catch (WebException ex)
            {
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }
            return content;

        }

        /// <summary>
        /// Метод, позволяющий загрузить файл на FTP-сервер.
        /// </summary>
        /// <param name="pathFrom">Строка с адресом, откуда мы хотим загрузить файл</param>
        /// <param name="pathTo">Строка с адресом, куда мы его хотим загрузить.
        /// Если грузим с FTP-сервера, оставляем строку пустой.
        /// Аргумент нужен для того, чтобы работала рекурсия. </param>
        public static void UploadFile(string pathFrom, string pathTo)
        {
            try
            {
                if (String.IsNullOrEmpty(pathTo)) pathTo = HostPath + '/';     // проверяем путь. Если он пустой, 
                                                                               // значит, путём будет папка
                                                                               // сервера. 

                Log.Information($"Запущен метод загрузки файла из [{pathFrom}] в [{pathTo}]");

                string filename = HandleStrings.GetFilename(pathFrom);         // получаем имя файла из пути.

                request = (FtpWebRequest)WebRequest.Create(pathTo + filename); // создаём
                                                                               // запрос к серверу, 
                                                                               // в котором мы
                                                                               // будем создавать 
                                                                               // файл с названием,
                                                                               // идентичным исходнику.

                request.Method = WebRequestMethods.Ftp.UploadFile; // определяем метод для загрузки файла на сервер

                request.Credentials = new NetworkCredential(Login, Password); // авторизуемся на сервере. 

                byte[] fileContents; // создаём массив байтов, в котором будет храниться файл. 
                using (reader = new StreamReader(pathFrom)) // кодируем файл в байты
                {
                    fileContents = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                }

                request.ContentLength = fileContents.Length;              // определяем размер содержимого

                using (Stream requestStream = request.GetRequestStream()) // создаём запос к серверу,
                                                                          // и записываем байты в поток 
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (response = (FtpWebResponse)request.GetResponse()) // получаем ответ от сервера. 
                {
                    Log.Information($"Загрузка файла {filename} завершена, " +
                        $"статус Ftp-сервера: {response.StatusDescription}");
                    mes($"Загрузка файла {filename} завершена, " +
                        $"статус Ftp-сервера: {response.StatusDescription}");
                }
            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (WebException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }

        }

        /// <summary>
        /// Метод, позволяющий загрузить папку на FTP-сервер. 
        /// </summary>
        /// <param name="pathFrom">Строка с адресом, откуда мы хотим загрузить папку (конечный слэш не нужен).</param>
        /// <param name="pathTo">Строка с адресом, куда мы хотим её загрузить.
        /// Если грузим с FTP-сервера, оставляем строку пустой.
        /// Аргумент нужен для правильной работы рекурсии. </param>
        public static void UploadDir(string pathFrom, string pathTo)
        {
            try
            {
                string dirName = HandleStrings.GetFilename(pathFrom);   // получаем имя директории 
                if (String.IsNullOrEmpty(pathTo)) pathTo = HandleStrings.GetNewPath(HostPath, dirName);   
                // проверяем строку пути на пустоту, 
                // если строка пустая, оставляем в пути
                // адрес сервера. 
                else pathTo = HandleStrings.GetNewPath(pathTo, dirName);

                Log.Information($"Запущена команда загрузки папки из [{pathFrom}] в [{pathTo}]"); 

                request = (FtpWebRequest)WebRequest.Create(pathTo);     // создаём запрос к серверу по новому пути
                request.Method = WebRequestMethods.Ftp.MakeDirectory;   // определяем метод для работы с сервером
                request.Credentials = new NetworkCredential(Login, Password);   // авторизуемся на сервере

                using (response = (FtpWebResponse)request.GetResponse()) // Получаем ответ от сервера. 
                {
                    Log.Information($"Папка с именем {dirName} создана в {pathTo}, " +
                        $"статус Ftp-сервера: {response.StatusDescription}");
                }

                string[] dirAll = Directory.GetFileSystemEntries(pathFrom); // Получаем все директории
                                                                            // и файлы из папки, которую
                                                                            // мы хотим загрузить на сервер.


                // оч топорно использовать Contains('.'), но я честно не придумала метода лучше :( 
                foreach (string i in dirAll) // итак, проходимся циклом по каждому файлу из списка загружаемой папки
                {
                    if (i.Contains('.')) // если в имени файла есть точка,
                                         // решаем, что это - файл, и загружаем
                                         // файл. Теперь в параметрах мы передаём не просто pathTo, но ещё 
                                         // и добавляем к нему слеш, 
                                         // чтобы файл загрузился не просто рядом с загружаемой папкой, а в саму
                                         // загружаемую папку. 
                    {
                        UploadFile(i, pathTo + '/');
                    }
                    else                // если же файл не содержит точки, будем считать, 
                                        // что это директория, и вызовем метод для загрузки директории. 
                                        // МОЯ ПЕРВАЯ РЕКУРСИЯ УРА XD
                    {
                        UploadDir(i, pathTo + '/');
                    }
                }

                Log.Information($"Загрузка папки {HandleStrings.GetFilename(pathFrom)}" +
                                $" вместе со всем её содержимым" +
                                $" завершена, статус FTP-сервера: " +
                                $"{response.StatusDescription}");
                mes($"Загрузка папки {HandleStrings.GetFilename(pathFrom)}" +
                    $" вместе со всем её содержимым " +
                    $" завершена, статус FTP-сервера: " +
                    $"{response.StatusDescription}");

            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (WebException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }
        }

        /// <summary>
        /// Метод, позволяющий скачать файл с FTP-сервера. 
        /// </summary>
        /// <param name="filename">Имя файла, которое мы хотим скачать. </param>
        /// <param name="pathFrom">Путь, откуда мы хотим его скачать.
        /// Если мы качаем из корневой папки сервера, оставляем pathTo пустым.
        /// Этот аргумент нужен для корректной работы рекурсии.</param>
        /// <param name="pathTo">Путь, куда мы будем загружать файл.</param>
        public static void DownloadFile(string filename, string pathTo, string pathFrom)
        {
            try
            {
                if (String.IsNullOrEmpty(pathFrom))                             // проверяем путь, откуда грузить папку,
                                                                                // на пустоту. Если pathFrom пустой,
                                                                                // оставляем в пути адрес сервера. 
                {
                    pathFrom = HandleStrings.GetNewPath(HostPath, filename);
                    pathTo = HandleStrings.GetNewPath(pathTo, filename);
                } 

                Log.Information($"Запущен метод скачивания файла из [{pathFrom}] в [{pathTo}]");

                request = (FtpWebRequest)WebRequest.Create(pathFrom); // создаём запрос к серверу

                request.Credentials = new NetworkCredential(Login, Password); // авторизуемся

                request.Method = WebRequestMethods.Ftp.DownloadFile; // указываем, что мы будем скачивать файл

                response = (FtpWebResponse)request.GetResponse(); // получаем ответ

                using (Stream ftpStream = response.GetResponseStream()) // преобразум ответ в поток
                using (Stream fileStream = File.Create(pathTo)) // перекладываем байтики из потока в файл
                {
                    ftpStream.CopyTo(fileStream);
                }

                Log.Information($"Скачивание файла {HandleStrings.GetFilename(pathFrom)}" +
                    $" завершено, статус FTP-сервера: " +
                    $"{response.StatusDescription}");
                mes($"Скачивание файла {HandleStrings.GetFilename(pathFrom)}" +
                    $" завершено, статус FTP-сервера: " +
                    $"{response.StatusDescription}");
            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (WebException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }
        }

        /// <summary>
        /// Метод, позволяющий скачать директорию с FTP-сервера. 
        /// </summary>
        /// <param name="dirName">Имя директории, которую мы будем скачивать.</param>
        /// <param name="pathTo">Путь, куда мы хотим её скачать.</param>
        /// <param name="pathFrom">Путь, из которого будем скачивать папку.
        /// Если мы качаем из корневой папки сервера, оставляем pathTo пустым.
        /// Этот аргумент нужен для корректной работы рекурсии.</param>
        public static void DownloadDir(string dirName, string pathTo, string pathFrom)
        {
            try
            { 
                if (String.IsNullOrEmpty(pathFrom)) pathFrom = HandleStrings.GetNewPath(HostPath, dirName);
                // проверяем строку пути на пустоту, 
                // если строка пустая, оставляем в пути
                // адрес сервера. 
                pathTo = HandleStrings.GetNewPath(pathTo, dirName);


                Log.Information($"Запущена команда скачивания папки из [{pathFrom}] в [{pathTo}]");

                if (!Directory.Exists(pathTo)) // проверяем, не существует ли уже такая папка  на компьютере. 
                {
                                                            // если такой папки в конечном пути не существует, 
                    Directory.CreateDirectory(pathTo);      // создаём её 
                    string dirItems = GetFiles(pathFrom);   // получаем из папки, которую хотим скачать,
                                                            // все внутренние файлы
                    string[] items = HandleStrings.GetSeparatesFiles(dirItems); // преобразуем их в массив строк
                    foreach (string i in items)                                 // и проверяем содержимое массива.
                    {
                        if (i.Contains('.'))                                    // если файл содержит точку,
                        {
                                                                                // то это будет файл. 
                            string newPathFrom = HandleStrings.GetPathWithoutNewLine(pathFrom, i); 
                            string newPathTo = HandleStrings.GetNewPath(pathTo, i);
                            DownloadFile(i, newPathFrom, newPathTo);       // качаем его.
                        }
                        else if (!HandleStrings.IsStringEmpty(i))
                        // проверяем итем на пустоту - при получении 
                        // списка файлов иногда последним элементом
                        // является эскейп-символ переноса строки, \n.
                        // Проверяем, не закрался ли коварный \n в наш список, и, если нет -
                        // определяем файл как директорию, и качаем директорию.
                        {
                            string newPathFrom = HandleStrings.GetNewPath(pathFrom, i);
                            string newPathTo = HandleStrings.GetNewPath(pathTo, i);
                            DownloadDir("", newPathTo, newPathFrom);    // при рекурсии, когда папки скачиваются из папок,
                                                                        // указывать имя директории не нужно, поэтому
                                                                        // передаём в качестве аргумента пустую строку
                        }
                    }
                }
                else
                {
                    mes($"Директория с именем " +
                        $"{HandleStrings.GetFilename(pathTo)} " +
                        $"уже существует! Скачивание не удалось.");
                }

                Log.Information($"Скачивание папки {HandleStrings.GetFilename(dirName)}" +
                $" завершено, статус FTP-сервера: " +
                $"{response.StatusDescription}");

                mes($"Скачивание папки {HandleStrings.GetFilename(dirName)}" +
                    $" завершено, статус FTP-сервера: " +
                    $"{response.StatusDescription}");
            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (WebException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }
        }

        /// <summary>
        /// Метод, позволяющий удалить файл с FTP-сервера. 
        /// </summary>
        /// <param name="filename">Имя файла.</param>
        /// <param name="pathFrom">Путь, из которого следует удалить файл. 
        /// Если мы удаляем файл из основной папки FTP-сервера,
        /// отправляем в качестве аргумента пустую строку.</param>
        public static void DeleteFile(string filename, string pathFrom)
        {
            try
            {
                string path;            
                if (String.IsNullOrEmpty(pathFrom)) path = HandleStrings.GetNewPath(HostPath, filename);        
                                                                            // проверяем строку пути на пустоту, 
                                                                            // если строка пустая, оставляем в пути
                                                                            // адрес сервера. 
                else path = HandleStrings.GetNewPath(pathFrom, filename);   // Если какой-то путь имеется, объединяем
                                                                            // его с именем файла. 


                request = (FtpWebRequest)WebRequest.Create(path);           // создаём запрос к серверу.

                request.Method = WebRequestMethods.Ftp.DeleteFile;          // определяем метод для удаления

                request.Credentials = new NetworkCredential(Login, Password);   // авторизуемся

                response = (FtpWebResponse)request.GetResponse(); // получаем ответ

                Log.Information($"Файл {filename} удалён, " +
                    $"статус FTP-сервера: " +
                    $"{response.StatusDescription}");
                mes($"Файл {filename} удалён, " +
                    $"статус FTP-сервера: " +
                    $"{response.StatusDescription}");

            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (WebException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }

        }

        /// <summary>
        /// Метод, позволяющий удалить директорию с FTP-сервера со всем её содержимым. 
        /// </summary>
        /// <param name="dirName">Имя директории.</param>
        /// <param name="pathFrom">Путь, из которого следует удалить файл. 
        /// Если мы удаляем файл из основной папки FTP-сервера,
        /// отправляем в качестве аргумента пустую строку.</param>
        public static void DeleteDir(string dirName, string pathFrom)
        {
            // так как WebRequestMethods.Ftp.RemoveDirectory не справляется
            // с удалением директории, в которой уже что-то лежит, 
            // то удаление тоже приходится осуществлять похожим на скачивание методом -
            // только здесь мы сначала удаляем всё содержимое папки, которую нужно удалить, 
            // очищаем её от файлов и директорий(ура! рекурсия!), 
            // а потом удаляем саму папку.
            try
            {
                string path;
                if (String.IsNullOrEmpty(pathFrom)) path = HandleStrings.GetNewPath(HostPath, dirName);
                                                                         // проверяем строку пути на пустоту, 
                                                                         // если строка пустая, оставляем в пути
                                                                         // адрес сервера. 
                else path = HandleStrings.GetNewPath(pathFrom, dirName); // Если какой-то путь имеется, объединяем
                                                                         // его с именем файла.

                string dirItems = GetFiles(path);                       // получаем из скачиваемой папки
                                                                        // все внутренние файлы
                string[] items = HandleStrings.GetSeparatesFiles(dirItems); // преобразуем их в массив строк
                foreach (string i in items)
                {
                    if (i.Contains('.')) 
                    {
                        DeleteFile(i, path);
                    }
                    else if (!HandleStrings.IsStringEmpty(i))
                    {
                        DeleteDir(i, path);
                    }
                }

                request = (FtpWebRequest)WebRequest.Create(path);   // только теперь, когда папка пуста, 
                                                                    // создаём запрос к серверу, 
                                                                    // который будет удалять уже очищенную 
                                                                    // директорию. 

                request.Method = WebRequestMethods.Ftp.RemoveDirectory; // определяем метод

                request.Credentials = new NetworkCredential(Login, Password); // авторизуемся

                response = (FtpWebResponse)request.GetResponse(); // получаем ответ

                Log.Information($"Папка {dirName} была удалена вместе со всем её содержимым!");
                mes($"Папка {dirName} была удалена вместе со всем её содержимым!");
            }
            catch (IOException)
            {
                mes("ERROR!");
                mes("Такого файла не существует.");
                Log.Error("Произошла ошибка при обращении к файлу.");
            }
            catch (WebException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла ошибка при обращении к сети.");
            }
            catch (SystemException ex)
            {
                mes("ERROR!");
                mes(ex.Message);
                Log.Error("Произошла системная ошибка.");
            }
        }


    }
}
