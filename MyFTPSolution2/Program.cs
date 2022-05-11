using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Serilog;


/// <summary>
/// Внимание! Логи сохраняются на рабочий стол, 
/// адрес сервера а также логин и пароль вшиты в класс FTPActions.
/// а в какой кодировке запихать картинки, я так чот и не въехала.  
/// Поэтому всё передаётся, работает клёво, но вот картинки 
/// так и остаются битыми.
/// Код вышел огромным, любые комментарии и замечания к нему
/// выслушаю с удовольствием!
/// 
/// В классе FTPActions, в методах удаления, скачивания и загрузки 
/// очень много одинакового кода. Но я так и не поняла, как с этим
/// можно справиться и как от этого избавиться. 
/// 
/// У меня была мысль передавать WebRequestMethods в качестве параметра, 
/// в одну универсальную функцию, 
/// но в response.Method я почему-то не могу передать что-то из агрумента функции :/ 
/// 
/// Поэтому пока что решение вот такое.
/// 
/// </summary>

namespace MyFTPSolution2
{
    class Program
    {
        public delegate void Message(string text);  // объявляем и сразу инициализируем делегат для
        public static Message message = Text;       // вывода сообщений в консоль.
        public static string path;                  // Часто используемые в методах переменные 
        public static string filename;              // выносим в класс. 

        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0])
                {
                    case "-list":
                        FTPActions.ShowFiles();
                        break;


                    case "-info":
                        Log.Information("Запущена команда отображения информации");
                        message("----------------------------------------------------------------------\n" +
                            "  СПИСОК ДОСТУПНЫХ КОМАНД:\n" +
                            " -list         [показать весь список файлов, находящихся на FTP-сервере]\n" +
                            " -info         [получить справку]\n" +
                            " -fileupl      [загрузить файл на FTP-сервер]\n" +
                            " -filedownl    [скачать файл с FTP-сервера]\n" +
                            " -filedel      [удалить файл с FTP-сервера]\n" +
                            " -dirupl       [загрузить папку на FTP-сервер]\n" +
                            " -dirdownl     [скачать папку с FTP-сервера]\n" +
                            " -dirdel       [удалить папку с FTP-сервера]\n" +
                            "----------------------------------------------------------------------");
                        break;


                    case "-fileupl":
                        UploadFile();
                        break;

                    case "-filedownl":
                        DownloadFile();
                        break;

                    case "-filedel":
                        DeleteFile();
                        break;
                    case "-dirupl":
                        UploadDir();
                        break;
                    case "-dirdownl":
                        DownloadDir();
                        break;
                    case "-dirdel":
                        DeleteDir();
                        break;

                    default:
                        Log.Error("Введён неверный и/или неопознаваемый флаг.");
                        message("ERROR!");
                        message("Неизвестная команда.");
                        break;
                }
            }
        }

        public static void Text(string message)
        {
            Console.WriteLine(message);
        }

        public static void UploadFile()
        {
            message("\n----------------------------------------------------------------------\n" +
                    "Введите полный путь до файла " +
                    "(с расширением), который вы хотите загрузить на сервер: ");
            path = Console.ReadLine();
            message("\n----------------------------------------------------------------------\n");
            FTPActions.UploadFile(path, "");
        }

        public static void UploadDir()
        {
            message("\n\nУкажите полный путь до папки, которую нужно загрузить на FTP-сервер: ");
            path = Console.ReadLine();
            message("\n----------------------------------------------------------------------\n");
            FTPActions.UploadDir(path, "");
        }

        public static void DownloadFile()
        {
            message("\n----------------------------------------------------------------------\n" +
                    "Укажите имя файла (с расширением), " +
                    "который вы хотите скачать с сервера:");
            filename = Console.ReadLine();
            message("\n\nУкажите полный путь, по которому нужно сохранить файл: ");
            path = Console.ReadLine();
            message("\n----------------------------------------------------------------------\n");
            FTPActions.DownloadFile(filename, "", path);
        }

        public static void DownloadDir()
        {
            message("\n----------------------------------------------------------------------\n" +
                    "Укажите имя папки " +
                    "(без дополнительных символов)," +
                    " которую вы хотите скачать с сервера:");
            filename = Console.ReadLine();
            message("\n\nУкажите полный путь, по которому нужно сохранить папку: ");
            path = Console.ReadLine();
            message("\n----------------------------------------------------------------------\n");
            FTPActions.DownloadDir(filename, path, "");
        }

        public static void DeleteFile()
        {
            message("\n----------------------------------------------------------------------\n" +
                    "Укажите имя файла (с расширением), " +
                    "который вы хотите удалить с сервера: ");
            path = Console.ReadLine();
            message("\n----------------------------------------------------------------------\n");
            FTPActions.DeleteFile(path, "");
        }

        public static void DeleteDir()
        {
            message("\n----------------------------------------------------------------------\n" +
            "Укажите имя папки, " +
            "которую вы хотите удалить с сервера, " +
            "без дополнительных символов: ");
            path = Console.ReadLine();
            message("\n----------------------------------------------------------------------\n");
            FTPActions.DeleteDir(path, "");
        }


    }
}
