using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SlackMUDRPG.Utility;
using System.IO;
using SlackMUDRPG.CommandClasses;
using Newtonsoft.Json;

namespace SlackMUDRPG
{
    public partial class RebuildCharNameList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string commandsFolderPath = FilePathSystem.GetFilePathFromFolder("Characters");
            DirectoryInfo dirInfo = new DirectoryInfo(commandsFolderPath);
            FileInfo[] CommandFiles = dirInfo.GetFiles("*.json");

            List<SMCharacter> smcl = new List<SMCharacter>();

            foreach (FileInfo file in CommandFiles)
            {
                if (file.Name != "CharNamesList.json")
                {
                    string path = FilePathSystem.GetFilePath("Characters", file.Name, "");
                    // Use a stream reader to read the file in (based on the path)
                    using (StreamReader r = new StreamReader(path))
                    {
                        // Create a new JSON string to be used...
                        string json = r.ReadToEnd();

                        // Get all the commands from the commands file
                        smcl.Add(JsonConvert.DeserializeObject<SMCharacter>(json));
                    }
                }
            }

            List<SMAccount> smal = new List<SMAccount>();

            foreach (SMCharacter smc in smcl)
            {
                SMAccount sma = new SMAccount();
                sma.AccountReference = smc.UserID;
                sma.CharacterName = smc.GetFullName();
                sma.UserName = smc.Username;
                sma.HashedPassword = smc.Password;

                smal.Add(sma);
            }

            string listJSON = JsonConvert.SerializeObject(smal, Formatting.Indented);
            string path2 = FilePathSystem.GetFilePath("Characters", "CharNamesList");
            using (StreamWriter w = new StreamWriter(path2))
            {
                w.WriteLine(listJSON);
            }

            //string specsPath = Utils.GetFilePath("Characters", "ClassBuilder");

            //if (File.Exists(specsPath))
            //{
            //    using (StreamReader r = new StreamReader(specsPath))
            //    {
            //        string json = r.ReadToEnd();

            //        cbs = JsonConvert.DeserializeObject<ClassBuilderSpecs>(json);
            //    }
            //}
        }
    }
}