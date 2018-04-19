using System;
using System.Collections.Generic;
using System.Data;
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
        const int SESSION_DURATION = 30;

        static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".XBM" };
        static readonly string FilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/Resources/access_log_Aug95";
        static string csvFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Resources\\";

        static string[,] sortedValidDataTable = new string[NUMBER_OF_DATA, 7];
        static string[,] validDataTable = new string[NUMBER_OF_DATA, 7];
        static string[,] popularSites;
        static List<string> mostPopularSites = new List<string>();

        static List<string> validData = new List<string>();
        static List<string> dataWithAllConditions = new List<string>();
        static List<int> dataWithAllConditionsIndexes = new List<int>();
        static string[,] tableWithAllConditions;
        static List<string> sites = new List<string>();
        static List<string> users = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Filling data...");
            FillData();
            Console.WriteLine("DONE");

            Console.WriteLine("Chosing popular sites...");
            ChoseMostPopularSites();
            Console.WriteLine("DONE");

            Console.WriteLine("Preparing files...");
            PrepareFiles();
            Console.WriteLine("DONE");

            Console.WriteLine("Click to EXIT");
            Console.ReadLine();
        }

        private static void ChoseMostPopularSites()
        {
            for(int i = 0; i < sites.Count; i++)
            {
                if (Convert.ToDouble(popularSites[i,2]) > 0.5)
                {
                    mostPopularSites.Add(popularSites[i, 0]);
                }
            }
        }
 
        private static void PrepareFiles()
        {
            var list = new List<string>();
            var validSessionLines = new List<string>();
            var validSessionCatLines = new List<string>();
            var validUserLines = new List<string>();
            var allValidSessions = new List<Session>();

            foreach (var user in users)
            {
                var usrs = new List<User>();
                var sess = new List<Session>();
                var times = new List<DateTime>();

                int operations = 0;

                for (int j = 0; j < dataWithAllConditionsIndexes.Count; j++)
                {
                    if (tableWithAllConditions[j, 0]==user)
                    {
                        var u = new User { Name = user, Time = DateTime.ParseExact(tableWithAllConditions[j, 2], "HH:mm:ss", CultureInfo.InvariantCulture), Site = tableWithAllConditions[j, 6] };
                        operations++;
                        usrs.Add(u);
                    }
                }

                if (operations != 0)
                {
                    var startSessionTime = usrs.First().Time;
                    var endSessionTime = usrs.Last().Time;
                    var time = endSessionTime - startSessionTime;

                    double countOfSessions = Math.Ceiling((double)time.TotalSeconds / 1800);

                    times.Add(startSessionTime);

                    for (int s = 1; s < countOfSessions; s++)
                    {
                        times.Add(times[s - 1].AddMinutes(30));
                    }

                    foreach (var u in usrs)
                    {
                        foreach (var t in times)
                        {
                            if (u.Time >= t && u.Time <= t.AddMinutes(30))
                            {
                                u.Session = times.IndexOf(t) + 1;
                            }
                        }
                    }

                    var validSessions = new List<Session>();

                    for (int s = 0; s < countOfSessions; s++)
                    {
                        if (usrs.Where(q => q.Session == s + 1).Any())
                        {
                            var diff = (usrs.Where(q => q.Session == s + 1).Last().Time - usrs.Where(q => q.Session == s + 1).First().Time).TotalSeconds;

                            var avgtime = diff / usrs.Where(q => q.Session == s + 1).Count();

                            var vs = new Session { User = user, AverageTime = avgtime, NumberOfOperations = usrs.Where(q => q.Session == s + 1).Count(), SessionTime = diff };

                            if (vs.SessionTime != 0)
                            {
                                vs.Number = s + 1;
                                validSessions.Add(vs);
                            }
                            else
                            {
                                usrs.Remove(usrs.Where(q => q.Session == s + 1).First());
                            }
                        }
                    }

                    foreach (var s in validSessions)
                    {
                        if(s.SessionTime<0)
                        {
                            s.SessionTime = 0 - s.SessionTime;
                        }

                        if(s.AverageTime<0)
                        {
                            s.AverageTime = 0 - s.AverageTime;
                        }

                        string line = $"{s.User},{s.Number},{Math.Round(s.SessionTime, 2)},{s.NumberOfOperations},{Math.Round(s.AverageTime, 2)}";

                        foreach (var site in mostPopularSites)
                        {
                            int counter = 0;
                            foreach (var u in usrs.Where(q => q.Session == s.Number))
                            {
                                if (u.Site == site)
                                {
                                    counter++;
                                }
                            }

                            if (counter > 0)
                            {
                                line += ",T";
                            }
                            else
                            {
                                line += ",F";
                            }
                        }


                        validSessionLines.Add(line);
                    }

                    if (validSessions.Any())
                    {
                        string userLine = $"{validSessions.First().User},{validSessions.Count}";

                        foreach (var site in mostPopularSites)
                        {
                            int counter = 0;
                            foreach (var u in usrs)
                            {
                                if (u.Site == site)
                                {
                                    counter++;
                                }
                            }

                            if (counter > 0)
                            {
                                userLine += ",T";
                            }
                            else
                            {
                                userLine += ",F";
                            }
                        }

                        validUserLines.Add(userLine);
                    }

                    allValidSessions.AddRange(validSessions);
                }
            }

            validSessionCatLines.AddRange(validSessionLines);

            var sortedSessionTimes = allValidSessions.Select(q => q.SessionTime).Distinct().ToList();
            sortedSessionTimes.Sort();

            double elementsInParts = Math.Ceiling((double)sortedSessionTimes.Count / 3);
            double lastPartCount = sortedSessionTimes.Count - (elementsInParts * 2);

            int firstPartIndex = (int)elementsInParts;
            int middlePartIndex = (int)elementsInParts * 2;

            double shortValueMin = sortedSessionTimes.GetRange(0, firstPartIndex - 1).First();
            double shortValueMax = sortedSessionTimes.GetRange(0, firstPartIndex - 1).Last();

            double mediumValueMin = sortedSessionTimes.GetRange(firstPartIndex, middlePartIndex - firstPartIndex).First();
            double mediumValueMax = sortedSessionTimes.GetRange(firstPartIndex, middlePartIndex - firstPartIndex).Last();

            double largeValueMin = sortedSessionTimes.GetRange(middlePartIndex, sortedSessionTimes.Count - middlePartIndex).First();
            double largeValueMax = sortedSessionTimes.GetRange(middlePartIndex, sortedSessionTimes.Count - middlePartIndex).Last();

            var sortedAverageTimes = allValidSessions.Select(q => q.AverageTime).Distinct().ToList();
            sortedAverageTimes.Sort();

            double elementsInPartsAvg = Math.Ceiling((double)sortedSessionTimes.Count / 3);
            double lastPartCountAvg = sortedAverageTimes.Count - (elementsInParts * 2);

            int firstPartIndexAvg = (int)elementsInParts;
            int middlePartIndexAvg = (int)elementsInParts * 2;

            double shortValueMinAvg = sortedAverageTimes.GetRange(0, firstPartIndexAvg - 1).First();
            double shortValueMaxAvg = sortedAverageTimes.GetRange(0, firstPartIndexAvg - 1).Last();

            double mediumValueMinAvg = sortedAverageTimes.GetRange(firstPartIndexAvg, middlePartIndexAvg - firstPartIndexAvg).First();
            double mediumValueMaxAvg = sortedAverageTimes.GetRange(firstPartIndexAvg, middlePartIndexAvg - firstPartIndexAvg).Last();

            double largeValueMinAvg = sortedAverageTimes.GetRange(middlePartIndexAvg, sortedAverageTimes.Count - middlePartIndexAvg).First();
            double largeValueMaxAvg = sortedAverageTimes.GetRange(middlePartIndexAvg, sortedAverageTimes.Count - middlePartIndexAvg).Last();


            foreach (var s in allValidSessions)
            {
                string line = $"{s.User},{s.Number},{Math.Round(s.SessionTime, 2)},{s.NumberOfOperations},{Math.Round(s.AverageTime, 2)}";

                if (s.SessionTime >= shortValueMin && s.SessionTime < mediumValueMin)
                {
                    s.SessionTimeCathegory = "short";
                }
                else if (s.SessionTime >= mediumValueMin && s.SessionTime < largeValueMin)
                {
                    s.SessionTimeCathegory = "average";
                }
                else if (s.SessionTime >= largeValueMin && s.SessionTime <= largeValueMax)
                {
                    s.SessionTimeCathegory = "long";
                }

                if (s.AverageTime >= shortValueMinAvg && s.AverageTime < mediumValueMinAvg)
                {
                    s.AverageTimeCathegory = "small";
                }
                else if (s.AverageTime >= mediumValueMinAvg && s.AverageTime < largeValueMinAvg)
                {
                    s.AverageTimeCathegory = "medium";
                }
                else if (s.AverageTime >= largeValueMinAvg && s.AverageTime <= largeValueMaxAvg)
                {
                    s.AverageTimeCathegory = "large";
                }

                if(s.NumberOfOperations >= 1 && s.NumberOfOperations <= 16)
                {
                    s.NumberOfOperationsCategory = "small";
                }
                else if(s.NumberOfOperations >= 17 && s.NumberOfOperations <= 32)
                {
                    s.NumberOfOperationsCategory = "medium";
                }
                else if(s.NumberOfOperations >= 33 && s.NumberOfOperations <= 98)
                {
                    s.NumberOfOperationsCategory = "large";
                }

                for(int l=0;l<validSessionCatLines.Count;l++)
                {
                    if(validSessionCatLines[l].Contains(line))
                    {
                        validSessionCatLines[l] = validSessionCatLines[l].Replace(line, $"{s.User},{s.Number},{s.SessionTimeCathegory},{s.NumberOfOperations},{s.AverageTimeCathegory}");
                        //validSessionCatLines[l] = validSessionCatLines[l].Replace(line, $"{s.NumberOfOperationsCategory},{s.SessionTimeCathegory},{s.AverageTimeCathegory}");
                    }
                }
            }

            string txtSessionFileName = csvFilePath + "sessions.txt";
            string wekaSessionFileName = csvFilePath + "sessions.arff";

            if (File.Exists(txtSessionFileName))
            {
                File.Delete(txtSessionFileName);
            }

            using (StreamWriter csvFile = new StreamWriter(txtSessionFileName, true))
            {
                csvFile.WriteLine("@RELATION NASA-SESSIONS");
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@ATTRIBUTE host STRING");
                csvFile.WriteLine("@ATTRIBUTE session-number INTEGER");
                csvFile.WriteLine("@ATTRIBUTE session-time INTEGER");
                csvFile.WriteLine("@ATTRIBUTE operations INTEGER");
                csvFile.WriteLine("@ATTRIBUTE average-time INTEGER");
                foreach (var site in mostPopularSites)
                {
                    csvFile.WriteLine("@ATTRIBUTE " + site + " {T,F}");
                }
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@DATA");

                foreach (var line in validSessionLines)
                {
                    csvFile.WriteLine(line);
                }
            }

            if (File.Exists(wekaSessionFileName))
            {
                File.Delete(wekaSessionFileName);
            }

            File.Move(txtSessionFileName, wekaSessionFileName);


            string txtUserFileName = csvFilePath + "users.txt";
            string wekaUserFileName = csvFilePath + "users.arff";

            if (File.Exists(txtUserFileName))
            {
                File.Delete(txtUserFileName);
            }

            using (StreamWriter csvFile = new StreamWriter(txtUserFileName, true))
            {
                csvFile.WriteLine("@RELATION NASA-USERS");
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@ATTRIBUTE user STRING");
                csvFile.WriteLine("@ATTRIBUTE sessions INTEGER");
                foreach (var site in mostPopularSites)
                {
                    csvFile.WriteLine("@ATTRIBUTE " + site + " {T,F}");
                }
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@DATA");

                foreach (var line in validUserLines)
                {
                    csvFile.WriteLine(line);
                }
            }

            if (File.Exists(wekaUserFileName))
            {
                File.Delete(wekaUserFileName);
            }

            File.Move(txtUserFileName, wekaUserFileName);


            string txtSessionCatFileName = csvFilePath + "sessions_cat.txt";
            string wekaSessionCatFileName = csvFilePath + "sessions_cat.arff";

            if (File.Exists(txtSessionCatFileName))
            {
                File.Delete(txtSessionCatFileName);
            }

            using (StreamWriter csvFile = new StreamWriter(txtSessionCatFileName, true))
            {
                csvFile.WriteLine("@RELATION NASA-USERS-CATEGORY");
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@ATTRIBUTE host STRING");
                csvFile.WriteLine("@ATTRIBUTE session-number INTEGER");
                csvFile.WriteLine("@ATTRIBUTE operations {small,medium,large}");
                csvFile.WriteLine("@ATTRIBUTE session-time {short,average,long}");
                csvFile.WriteLine("@ATTRIBUTE average-time {small,medium,large}");
                foreach (var site in mostPopularSites)
                {
                    csvFile.WriteLine("@ATTRIBUTE " + site + " {T,F}");
                }
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@DATA");

                foreach (var line in validSessionCatLines)
                {
                    csvFile.WriteLine(line);
                }
            }

            if (File.Exists(wekaSessionCatFileName))
            {
                File.Delete(wekaSessionCatFileName);
            }

            File.Move(txtSessionCatFileName, wekaSessionCatFileName);

        }

        private static void FillData()
        {
            string line;
            int row = 0;

            StreamReader file = new StreamReader(FilePath);
            while ((line = file.ReadLine()) != null)
            {
                AddDataToTable(line, row);
                row++;
            }


            for(int i = 0; i<NUMBER_OF_DATA;i++)
            {
                for(int j=0;j<7;j++)
                {
                    validDataTable[i, j] = sortedValidDataTable[i, j];
                }
            }

            Sort(sortedValidDataTable, 0);

            file.Close();

            for (int i = 0;i< NUMBER_OF_DATA; i++)
            {
                string validLine = "";

                for (int j = 0; j < 7; j++)
                {
                    if (sortedValidDataTable[i, j] != String.Empty)
                    {
                        validLine += sortedValidDataTable[i, j] + " ";
                    }
                }

                validLine = validLine.Remove(validLine.Length - 1, 1);

                validData.Add(validLine);

                string fileExtension = Path.GetExtension(sortedValidDataTable[i, 6]);

                if (!ImageExtensions.Contains(fileExtension.ToUpper()) && sortedValidDataTable[i, 6] != "/")
                {
                    sites.Add(sortedValidDataTable[i, 6]);
                }

                if (sortedValidDataTable[i, 4] != String.Empty && sortedValidDataTable[i, 3].Contains("GET") && sortedValidDataTable[i, 5] == "200" && !ImageExtensions.Contains(fileExtension.ToUpper()) && sortedValidDataTable[i, 6] != "/")
                {
                    dataWithAllConditions.Add(validLine);
                    dataWithAllConditionsIndexes.Add(i);
                    users.Add(sortedValidDataTable[i, 0]);
                }
            }

            users = users.Distinct().ToList();

            tableWithAllConditions = new string[dataWithAllConditions.Count, 7];

            int r1 = 0;
            for (int i = 0; i < NUMBER_OF_DATA; i++)
            {
                if(dataWithAllConditionsIndexes.Contains(i))
                {
                    for(int j=0;j<7;j++)
                    {
                        tableWithAllConditions[r1, j] = sortedValidDataTable[i, j];
                    }
                    r1++;
                }
            }

            Sort(tableWithAllConditions, 0);

            sites = sites.Distinct().ToList();

            popularSites = new string[sites.Count, 3];
            int r = 0;

            foreach (var site in sites)
            {
                int counter = 0;
                
                foreach (var element in dataWithAllConditions)
                {
                    if (element.Contains(site))
                    {
                        counter++;
                    }                
                }

                popularSites[r, 0] = site;
                popularSites[r, 1] = counter.ToString();
                popularSites[r, 2] = ((100.0 * counter) / NUMBER_OF_DATA).ToString();

                r++;
            }
        }

        private static void AddDataToTable(string line, int row)
        {
            sortedValidDataTable[row, 0] = GetAddress(line);
            sortedValidDataTable[row, 1] = GetDate(line);
            sortedValidDataTable[row, 2] = GetTime(line);
            sortedValidDataTable[row, 3] = GetMethod(line);
            sortedValidDataTable[row, 4] = GetProtocol(line);
            sortedValidDataTable[row, 5] = GetStatusCode(line);
            sortedValidDataTable[row, 6] = GetFile(line);
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

        private static void Sort(string[,] array, int sortCol)
        {
            int colCount = array.GetLength(1), rowCount = array.GetLength(0);
            if (sortCol >= colCount || sortCol < 0)
                throw new System.ArgumentOutOfRangeException("sortCol", "The column to sort on must be contained within the array bounds.");

            DataTable dt = new DataTable();
            // Name the columns with the second dimension index values, e.g., "0", "1", etc.
            for (int col = 0; col < colCount; col++)
            {
                DataColumn dc = new DataColumn(col.ToString(), typeof(string));
                dt.Columns.Add(dc);
            }
            // Load data into the data table:
            for (int rowindex = 0; rowindex < rowCount; rowindex++)
            {
                DataRow rowData = dt.NewRow();
                for (int col = 0; col < colCount; col++)
                    rowData[col] = array[rowindex, col];
                dt.Rows.Add(rowData);
            }
            // Sort by using the column index = name + an optional order:
            DataRow[] rows = dt.Select("", sortCol.ToString());

            for (int row = 0; row <= rows.GetUpperBound(0); row++)
            {
                DataRow dr = rows[row];
                for (int col = 0; col < colCount; col++)
                {
                    array[row, col] = (string)dr[col];
                }
            }

            dt.Dispose();
        }
    }
}