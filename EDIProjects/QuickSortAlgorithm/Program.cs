using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSortAlgorithm
{
    class Program
    {
        const int NUMBER_OF_DATA = 50000;

        static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
        static readonly string FilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/Resources/access_log_Aug95";
        static string csvFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\";
        static string[,] validDataTable = new string[NUMBER_OF_DATA, 7];

        static List<string> validData = new List<string>();
        static List<string> dataWithGetMethod = new List<string>();
        static List<string> dataWithOkStatus = new List<string>();
        static List<string> dataWithoutImages = new List<string>();
        static List<string> dataWithAllConditions = new List<string>();

        static void Main(string[] args)
        {
            FillData();

            Console.WriteLine("Number of valid data: " + validData.Count);
            Console.WriteLine("Number of data with GET method: " + dataWithGetMethod.Count);
            Console.WriteLine("Number of data with OK (200) status: " + dataWithOkStatus.Count);
            Console.WriteLine("Number of data without images: " + dataWithoutImages.Count);
            Console.WriteLine("Number of data with all above conditions: " + dataWithAllConditions.Count);

            Console.WriteLine("Saving File...");
            SaveToCSVFile(dataWithGetMethod, "dataWithGetMethod");
            SaveToCSVFile(dataWithOkStatus, "dataWithOkStatus");
            SaveToCSVFile(dataWithoutImages, "dataWithoutImages");
            SaveToCSVFile(dataWithAllConditions, "dataWithAllConditions");

            Console.WriteLine("Click to EXIT");
            Console.ReadLine();
        }

        private static void SaveToCSVFile(List<string> validData, string fileName)
        {
            fileName = csvFilePath + fileName;
            string txtfileName = fileName + ".txt";
            string csvFileName = fileName + ".csv";
            string wekaFileName = fileName + ".arff";

            if(File.Exists(txtfileName))
            {
                File.Delete(txtfileName);
            }
            
            using (StreamWriter csvFile = new StreamWriter(txtfileName, true))
            {
                csvFile.WriteLine("@RELATION NASA");
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@ATTRIBUTE host STRING");
                csvFile.WriteLine("@ATTRIBUTE date DATE \"dd/MM/yyyy\" ");
                csvFile.WriteLine("@ATTRIBUTE time DATE \"HH:mm:ss\"");
                csvFile.WriteLine("@ATTRIBUTE method STRING");
                csvFile.WriteLine("@ATTRIBUTE protocol STRING");
                csvFile.WriteLine("@ATTRIBUTE status INTEGER");
                csvFile.WriteLine("@ATTRIBUTE file STRING");
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@DATA");

                foreach (var line in validData)
                {
                    csvFile.WriteLine(line.Replace(" ", ","));
                }
            }

            if (File.Exists(wekaFileName))
            {
                File.Delete(wekaFileName);
            }

            File.Move(txtfileName, wekaFileName);
        }

        private static void FillData()
        {
            string line;
            int row = 0;

            StreamReader file = new StreamReader(FilePath);
            while ((line = file.ReadLine()) != null)
            {
                AddDataToTable(line, row);
                string validLine = "";

                for (int i = 0; i < 7; i++)
                {
                    if(validDataTable[row, i]!=String.Empty)
                    {
                        validLine += validDataTable[row, i] + " ";
                    }                 
                }

                validLine = validLine.Remove(validLine.Length - 1, 1);

                validData.Add(validLine);

                if (validDataTable[row, 3].Contains("GET"))
                {
                    dataWithGetMethod.Add(validLine);
                }

                if (validDataTable[row, 5] == "200")
                {
                    dataWithOkStatus.Add(validLine);
                }

                string fileExtension = Path.GetExtension(validDataTable[row, 6]);

                if (!ImageExtensions.Contains(fileExtension.ToUpper()))
                {
                    dataWithoutImages.Add(validLine);
                }

                if (validDataTable[row, 4]!=String.Empty && validDataTable[row, 3].Contains("GET") && validDataTable[row, 5] == "200" && !ImageExtensions.Contains(fileExtension.ToUpper()))
                {
                    dataWithAllConditions.Add(validLine);
                }

                row++;
            }

            file.Close();
        }

        private static void AddDataToTable(string line, int row)
        {
            validDataTable[row, 0] = GetAddress(line);
            validDataTable[row, 1] = GetDate(line);
            validDataTable[row, 2] = GetTime(line);
            validDataTable[row, 3] = GetMethod(line);
            validDataTable[row, 4] = GetProtocol(line);
            validDataTable[row, 5] = GetStatusCode(line);
            validDataTable[row, 6] = GetFile(line);
        }

        private static string[] GetResponseLine(string line)
        {
            int startIndex;
            int endIndex;

            startIndex = line.IndexOf('"');
            endIndex = line.IndexOf('"', startIndex + 1);

            string substring = line.Substring(startIndex + 1, endIndex - startIndex - 1);

            var result = substring.Split(' ');
            return result;
        }

        private static string GetFile(string line)
        {
            string[] result = GetResponseLine(line);

            return result[1];
        }

        private static string GetProtocol(string line)
        {
            string[] result = GetResponseLine(line);

            if (result.Length < 3)
            {
                return String.Empty;
            }

            return result[2];
        }

        private static string GetStatusCode(string line)
        {
            int startIndex;
            int endIndex;

            startIndex = line.IndexOf('"');
            endIndex = line.IndexOf('"', startIndex + 1);

            return line.Substring(endIndex + 2, 3);
        }

        private static string GetMethod(string line)
        {
            string[] result = GetResponseLine(line);

            return result[0];
        }

        private static string[] GetDateLine(string line)
        {
            int startIndex;
            int endIndex;

            startIndex = line.IndexOf('[');
            endIndex = line.IndexOf(']');

            string substring = line.Substring(startIndex + 1, endIndex - startIndex - 1);

            var result = substring.Split(' ');
            return result;
        }

        private static string GetDate(string line)
        {
            string[] result = GetDateLine(line);

            DateTime date = DateTime.ParseExact(result[0], "dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture);

            return date.ToShortDateString();
        }

        private static string GetTime(string line)
        {
            string[] result = GetDateLine(line);

            string res = result[0].Remove(0, 12);

            return res;
        }

        private static string GetAddress(string line)
        {
            return line.Substring(0, line.IndexOf(' '));
        }
    }
}
