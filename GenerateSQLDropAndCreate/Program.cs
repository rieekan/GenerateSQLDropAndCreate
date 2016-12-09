using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Diagnostics;
using USG.SQL.SMO;

namespace GenerateSQLDropAndCreate
{
    public class Program
    {
        static void Main(string[] args)
        {
            var util = new USG.SQL.SMO.Utility();
            string localPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            string serverName = @"USILG01-DWD005";
            string databaseName = "UnderwritingReview";
            string fileName = localPath + @"\" + databaseName + ".sql";

            util.ScriptDatabase(serverName, databaseName, fileName, Progress);

            Console.WriteLine("Press any key.");
            Console.ReadKey();

            Process.Start("notepad.exe", fileName);
        }

        static void Progress(string Message)
        {
            Console.WriteLine(Message);
        }
    }
}
