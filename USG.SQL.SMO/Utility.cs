using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SqlServer;
using Microsoft.SqlServer.Management.Utility;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Management;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Common;
using Microsoft.SqlServer.Management.Common;

namespace USG.SQL.SMO
{
    /// <summary>
    /// http://www.sqlteam.com/article/scripting-database-objects-using-smo-updated
    /// https://msdn.microsoft.com/en-us/library/ms162153.aspx
    /// https://social.msdn.microsoft.com/Forums/sqlserver/en-US/c91bd7d4-8ea8-43ce-97de-4ea4f14adebb/what-replaces-microsoftsqlserversmo-in-visual-studio-2013?forum=vbgeneral
    /// https://msdn.microsoft.com/en-us/library/ms162129(v=sql.110).aspx
    /// http://stackoverflow.com/questions/12140422/generating-sql-script-through-code-c-net
    /// </summary>
    public class Utility
    {
        public delegate void ProgressDelegate(string message);

        public void ScriptDatabase(string ServerName, string DatabaseName, string Filename, ProgressDelegate progress)
        {
            var script = "";

            try
            {
                script = ScriptDatabase(ServerName, DatabaseName, progress);

                string localPath = System.Reflection.Assembly.GetEntryAssembly().Location;

                System.IO.File.WriteAllText(Filename, script);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public string ScriptDatabase(string ServerName, string DatabaseName, ProgressDelegate progress)
        {
            var sb = new StringBuilder();
            string scrs = "";

            try
            {
                var server = new Server(ServerName);
                var database = server.Databases[DatabaseName];

                var scripter = new Scripter(server);
                scripter.Options.BatchSize = 1;
                scripter.Options.ScriptDrops = true;
                scripter.Options.ScriptBatchTerminator = true;
                scripter.Options.NoCommandTerminator = false;
                scripter.Options.ConvertUserDefinedDataTypesToBaseType = false;
                scripter.Options.Default = true;
                scripter.Options.ScriptSchema = true;
                scripter.Options.ScriptData = true;
                scripter.Options.WithDependencies = true;
                scripter.Options.IncludeHeaders = false;
                scripter.Options.ClusteredIndexes = true;
                scripter.Options.DriAllKeys = true;
                scripter.Options.DriDefaults = true;
                scripter.Options.DriIndexes = true;
                scripter.Options.DriNonClustered = true;
                scripter.Options.DriPrimaryKey = true;
                scripter.Options.DriUniqueKeys = true;
                scripter.Options.FullTextIndexes = true;
                scripter.Options.Triggers = false;
                scripter.ScriptingProgress += scripter_ScriptingProgress;
                scripter.Options.WithDependencies = false;

                //var smoObjects = new Urn[1];
                //foreach (Table t in database.Tables)
                //{
                //    smoObjects[0] = t.Urn;
                //    if (t.IsSystemObject == false)
                //    {
                //        StringCollection sc = scripter.Script(smoObjects);

                //        foreach (var st in sc)
                //        {
                //            sb.Append(st);
                //            sb.Append("\r\n\r\n");
                //        }
                //    }
                //}

                string GO = "GO\r\n";
                string crnl = "\r\n";

                scrs = String.Format("USE [{0}]", DatabaseName) + crnl;
                scrs += GO + crnl + crnl;

                foreach (Table myTable in database.Tables)
                {
                    if (myTable.IsSystemObject == false)
                    {
                        IEnumerable<string> tableScripts = scripter.EnumScript(new Urn[] { myTable.Urn });
                        string tmp = "";
                        foreach (string script in tableScripts)
                            tmp += script + crnl;
                        scrs += tmp + GO;
                        progress(tmp);
                    }
                }

                scrs += "\r\n\r\n";

                scripter.Options.ScriptDrops = false;

                foreach (Table myTable in database.Tables)
                {
                    if (myTable.IsSystemObject == false)
                    {
                        IEnumerable<string> tableScripts = scripter.EnumScript(new Urn[] { myTable.Urn });
                        string tmp = "";
                        foreach (string script in tableScripts)
                            tmp += script + crnl;
                        scrs += tmp + GO;
                        progress(tmp);
                    }
                }

                //progress("*******************************************************************************");
                //progress(" Filtering system stored procedures...");
                //progress("*******************************************************************************");

                ////List<StoredProcedure> filteredList = database.StoredProcedures.Where(x => x.IsSystemObject == false).ToList();
                //List<StoredProcedure> list = (List<StoredProcedure>)database.StoredProcedures.OfType<StoredProcedure>().Where(x => x.IsSystemObject == false).ToList<StoredProcedure>();

                //scripter.Options.ScriptDrops = true;
                //foreach (StoredProcedure sproc in list)
                //{
                //    IEnumerable<string> sprocScripts = scripter.EnumScript(new Urn[] { sproc.Urn });
                //    string tmp = "";
                //    foreach (string script in sprocScripts)
                //        tmp += script + crnl;
                //    scrs += tmp + GO;
                //    progress(tmp);
                //}

                //scripter.Options.ScriptDrops = false;
                //foreach (StoredProcedure sproc in list)
                //{
                //    IEnumerable<string> sprocScripts = scripter.EnumScript(new Urn[] { sproc.Urn });
                //    string tmp = "";
                //    foreach (string script in sprocScripts)
                //        tmp += script + crnl;
                //    scrs += tmp + GO;
                //    progress(tmp);
                //}


                progress("*******************************************************************************");
                progress(" Filtering system functions...");
                progress("*******************************************************************************");
                //List<StoredProcedure> filteredList = database.StoredProcedures.Where(x => x.IsSystemObject == false).ToList();
                List<UserDefinedFunction> listFunctions = (List<UserDefinedFunction>)database.StoredProcedures.OfType<UserDefinedFunction>().Where(x => x.IsSystemObject == false).ToList<UserDefinedFunction>();

                scripter.Options.ScriptDrops = true;
                foreach (UserDefinedFunction func in listFunctions)
                {
                    IEnumerable<string> sprocScripts = scripter.EnumScript(new Urn[] { func.Urn });
                    string tmp = "";
                    foreach (string script in sprocScripts)
                        tmp += script + crnl;
                    scrs += tmp + GO;
                    progress(tmp);
                }

                scripter.Options.ScriptDrops = false;
                foreach (UserDefinedFunction func in listFunctions)
                {
                    IEnumerable<string> sprocScripts = scripter.EnumScript(new Urn[] { func.Urn });
                    string tmp = "";
                    foreach (string script in sprocScripts)
                        tmp += script + crnl;
                    scrs += tmp + GO;
                    progress(tmp);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            //return sb.ToString();
            scrs = scrs.Replace("SET ANSI_NULLS ON", "SET ANSI_NULLS ON\r\nGO");
            scrs = scrs.Replace("SET QUOTED_IDENTIFIER ON", "SET QUOTED_IDENTIFIER ON\r\nGO");
            return scrs;
        }

        void scripter_ScriptingProgress(object sender, ProgressReportEventArgs e)
        {
            //throw new NotImplementedException();

            // TODO: create a delegate and pass in a progresss handler from calling environment
        }
    }
}
