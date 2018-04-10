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
        static string[,] sortedValidDataTable = new string[NUMBER_OF_DATA, 8];
        static string[,] validDataSortedBySession = new string[NUMBER_OF_DATA, 8];
        static string[,] validDataTable = new string[NUMBER_OF_DATA, 8];
        static string[,] sessions = new string[49, 2];
        static string[,] popularSites;
        static List<string> mostPopularSites = new List<string>();

        static List<string> validData = new List<string>();
        static List<string> dataWithGetMethod = new List<string>();
        static List<string> dataWithOkStatus = new List<string>();
        static List<string> dataWithoutImages = new List<string>();
        static List<string> dataWithAllConditions = new List<string>();
        static List<int> dataWithAllConditionsIndexes = new List<int>();
        static string[,] tableWithAllConditions;
        static string[,] sortedTableWithALlConditions;
        static List<string> sites = new List<string>();
        static List<string> users = new List<string>();

        static void Main(string[] args)
        {
            FillData();
            ChoseMostPopularSites();

            Console.WriteLine("Number of valid data: " + validData.Count);
            Console.WriteLine("Number of data with GET method: " + dataWithGetMethod.Count);
            Console.WriteLine("Number of data with OK (200) status: " + dataWithOkStatus.Count);
            Console.WriteLine("Number of data without images: " + dataWithoutImages.Count);
            Console.WriteLine("Number of data with all above conditions: " + dataWithAllConditions.Count);

            for (int i = 0; i < 48; i++)
            {
                Console.WriteLine($"nr: {sessions[i,0]}  time: {sessions[i,1]}");
            }

            Console.WriteLine("Saving File...");

            //SaveToCSVFile(dataWithAllConditions, "dataWithAllConditions");
            SaveToCSVSessionFile();

            SetSessions();

            Console.WriteLine("Click to EXIT");
            Console.ReadLine();
        }

        private static void SetSessions()
        {
            for(int i=0;i<48;i++)
            {
                for(int j=0;i<NUMBER_OF_DATA;j++)
                {

                }
            }
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

        private static void FillSessions()
        {
            DateTime sessionTime = new DateTime(1995, 8, 1, 0, 0, 0);
            for (int i = 0; i < 48; i++)
            {
                sessions[i, 0] = (i+1).ToString();
                sessions[i, 1] = sessionTime.ToString("HH:mm:ss");

                sessionTime = sessionTime.AddMinutes(30);
            }

            sessions[48, 0] = "49";
            sessions[48, 1] = "00:00:00";
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
                csvFile.WriteLine("@ATTRIBUTE session STRING");
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

        private static void SaveToCSVSessionFile()
        {
            var list = new List<string>();
            var validSessionLines = new List<string>();

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

                if(operations!=0)
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
                    
                    foreach(var u in usrs)
                    {
                        foreach(var t in times)
                        {
                            if(u.Time >= t && u.Time<=t.AddMinutes(30))
                            {
                                u.Session = times.IndexOf(t)+1;
                            }
                        }
                    }

                    var validSessions = new List<Session>();

                    for (int s = 0; s < countOfSessions; s++)
                    {
                        if(usrs.Where(q => q.Session == s + 1).Any())
                        {
                            var diff = (usrs.Where(q => q.Session == s + 1).Last().Time - usrs.Where(q => q.Session == s + 1).First().Time).TotalSeconds;

                            var avgtime = diff / usrs.Where(q => q.Session == s + 1).Count();

                            var vs = new Session { User = user, AverageTime = avgtime, NumberOfOperations = usrs.Where(q => q.Session == s + 1).Count(), SessionTime = diff };

                            if (vs.SessionTime != 0)
                            {
                                vs.Number = s + 1;
                                validSessions.Add(vs);
                            }
                        }
                    }

                    foreach(var s in validSessions)
                    {
                        string line = $"{s.User},{s.Number},{Math.Round(s.SessionTime,2)},{s.NumberOfOperations},{Math.Round(s.AverageTime,2)}";

                        foreach(var site in mostPopularSites)
                        {
                            int counter = 0;
                            foreach (var u in usrs.Where(q => q.Session == s.Number))
                            {
                                if (u.Site == site)
                                {
                                    counter++;
                                }
                            }

                            if(counter>0)
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
                }
            }

            string txtfileName = csvFilePath + "sessions.txt";
            string csvFileName = csvFilePath + "sessions.csv";
            string wekaFileName = csvFilePath + "sessions.arff";

            if (File.Exists(txtfileName))
            {
                File.Delete(txtfileName);
            }

            using (StreamWriter csvFile = new StreamWriter(txtfileName, true))
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
                    csvFile.WriteLine("@ATTRIBUTE " + site + " STRING");
                }
                csvFile.WriteLine(String.Empty);
                csvFile.WriteLine("@DATA");

                foreach (var line in validSessionLines)
                {
                    csvFile.WriteLine(line);
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

            FillSessions();

            StreamReader file = new StreamReader(FilePath);
            while ((line = file.ReadLine()) != null)
            {
                AddDataToTable(line, row);
                row++;
            }


            for(int i = 0; i<NUMBER_OF_DATA;i++)
            {
                for(int j=0;j<8;j++)
                {
                    validDataTable[i, j] = sortedValidDataTable[i, j];
                    validDataSortedBySession[i,j] = sortedValidDataTable[i, j];
                }
            }

            Sort(sortedValidDataTable, 0);
            Sort(validDataSortedBySession, 7);

            file.Close();

            for (int i = 0;i< NUMBER_OF_DATA; i++)
            {
                string validLine = "";

                for (int j = 0; j < 8; j++)
                {
                    if (sortedValidDataTable[i, j] != String.Empty)
                    {
                        validLine += sortedValidDataTable[i, j] + " ";
                    }
                }

                validLine = validLine.Remove(validLine.Length - 1, 1);

                validData.Add(validLine);

                if (sortedValidDataTable[i, 3].Contains("GET"))
                {
                    dataWithGetMethod.Add(validLine);
                }

                if (sortedValidDataTable[i, 5] == "200")
                {
                    dataWithOkStatus.Add(validLine);
                }

                string fileExtension = Path.GetExtension(sortedValidDataTable[i, 6]);

                if (!ImageExtensions.Contains(fileExtension.ToUpper()) && sortedValidDataTable[i, 6] != "/")
                {
                    dataWithoutImages.Add(validLine);
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

            tableWithAllConditions = new string[dataWithAllConditions.Count, 8];

            int r1 = 0;
            for (int i = 0; i < NUMBER_OF_DATA; i++)
            {
                if(dataWithAllConditionsIndexes.Contains(i))
                {
                    for(int j=0;j<8;j++)
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
            sortedValidDataTable[row, 7] = GetSession(row);
        }

        private static string GetSession(int row)
        {
            for(int i = 0; i < 48; i++)
            {
                var x = DateTime.ParseExact(sessions[i, 1], "HH:mm:ss", CultureInfo.InvariantCulture);
                var y = DateTime.ParseExact(sessions[i+1, 1], "HH:mm:ss", CultureInfo.InvariantCulture);
                var z = DateTime.ParseExact(sortedValidDataTable[row, 2], "HH:mm:ss", CultureInfo.InvariantCulture);

                if(z.TimeOfDay >= x.TimeOfDay && z.TimeOfDay <= y.TimeOfDay)
                {
                    return sessions[i, 0];
                }
            }

            return "cannot determine session";
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
