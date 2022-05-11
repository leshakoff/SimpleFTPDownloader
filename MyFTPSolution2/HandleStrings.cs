using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFTPSolution2
{
    /// <summary>
    /// Статический (и стратегический для меня)
    /// класс, предназначенный для работы со строками.
    /// </summary>
    public static class HandleStrings
    {
        private static string sepSlash = @"\";
        private static string sepBackSlash = @"/";                  // часто встречающиеся строки-сепараторы: слеш
        private static string sepNewLine = Environment.NewLine;     // и перенос строки. 
        private static string[] filenameArray;                      // Массив строк,
                                                                    // который будет разделяться сепаратором. 


        static HandleStrings()
        { }

        /// <summary>
        /// Метод, позволяющий получить название файла из полного пути.
        /// </summary>
        /// <param name="path">В качестве аргумента принимает путь. </param>
        /// <returns>Строка с названием файла и расширением, если оно есть.</returns>
        public static string GetFilename(string path)
        {
       
            if (path.Contains(sepSlash)) filenameArray = path.Split(sepSlash.ToCharArray()[0]);
            else if (path.Contains(sepBackSlash)) filenameArray = path.Split(sepBackSlash.ToCharArray()[0]);
            return filenameArray[filenameArray.Length - 1];
        }

        /// <summary>
        /// Метод, позволяющий получить новый путь, состоящий из старого пути и имени файла.
        /// </summary>
        /// <param name="path">Строка, в которой содержится путь. </param>
        /// <param name="filename">Строка с названием файла.</param>
        /// <returns>Строка с корректным названием пути. </returns>
        public static string GetNewPath(string path, string filename)
        {
            if (String.IsNullOrEmpty(filename)) return path;
            if (path.Contains(sepSlash)) return path + sepSlash + filename;
            else return path + sepBackSlash + filename;
        }

        /// <summary>
        /// Метод, позволяющий получить массив строк из одной строки, 
        /// в которую были записаны все полученные файлы. Если в строке
        /// содержится символ переноса строки, он удаляется.
        /// </summary>
        /// <param name="items">Строка со списком файлов.</param>
        /// <returns>Массив строк, состоящий из названий файлов.</returns>
        public static string[] GetSeparatesFiles(string items)
        {
            string[] newItems = items.Split(sepNewLine.ToCharArray()[0]);
            string[] itemstWithoutSep = new string[newItems.Length]; 
            for(int i = 0; i < newItems.Length; i++)
            {
                if (newItems[i].Contains('\n'))
                {
                    itemstWithoutSep[i] = newItems[i].TrimStart(sepNewLine.ToCharArray());
                }
                else itemstWithoutSep[i] = newItems[i];
            }
            return itemstWithoutSep;
        }

        /// <summary>
        /// Метод, объединяющий путь с именем файла. Если имя файла
        /// содержит символ переноса строки, он удаляется.
        /// </summary>
        /// <param name="pathFrom">Путь, который нужно объединить с именем файла.</param>
        /// <param name="item">Имя файла.</param>
        /// <returns>Строка, содержащая путь, объединённый с именем файла.</returns>
        public static string GetPathWithoutNewLine(string pathFrom, string item)
        {
            pathFrom += '/';
            if (item.Contains('\n')) pathFrom += item.TrimStart('\n');
            else pathFrom += item;
            return pathFrom;
        }

        /// <summary>
        /// Проверяет строку на пустоту. Когда мы получаем строку из ListDirectory, 
        /// он автоматически добавляет символ переноса строки перед каждым именем файла. 
        /// Когда мы, позднее, разделяем строку из ListDirectory, преобразуя её в массив, 
        /// последним элементом в массиве оказывается строка, состоящая из '\r\n'. 
        /// Когда мы избавляемся от символов переноса строки в массиве, остаётся пустая строка 
        /// на месте этого элемента. 
        /// </summary>
        /// <param name="item">Строка, которую нужно проверить на пустоту. </param>
        /// <returns>true, если строка пустая. 
        /// false, если строка не пустая. </returns>
        public static bool IsStringEmpty(string item)
        {
            if (item.ToCharArray().Length <= 1 || item.ToCharArray()[1] == ' ') return true;
            else return false;
        }

    }
}
