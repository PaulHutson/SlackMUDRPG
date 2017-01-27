using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG
{
	public class Global : System.Web.HttpApplication
	{

		protected void Application_Start(object sender, EventArgs e)
		{
			Application["TestOutput"] = 0;

			// Character List
			List<SMCharacter> smc = new List<SMCharacter>();
			Application["SMCharacters"] = smc;

			// Room List
			List<SMRoom> smr = new List<SMRoom>();
			Application["SMRooms"] = smr;

			// Load the Commands into memory for usage later
			// Used for both parsing commands sent in and also for help output
			SMCommands lsmc = new SMCommands();
			string commandsPath = FilePathSystem.GetFilePath("Commands", "Help");
			// Check if the character exists..
			if (File.Exists(commandsPath))
			{
				// Use a stream reader to read the file in (based on the path)
				using (StreamReader r = new StreamReader(commandsPath))
				{
					// Create a new JSON string to be used...
					string json = r.ReadToEnd();

					// ... get the information from the help file
					lsmc = JsonConvert.DeserializeObject<SMCommands>(json);
				}
			}
			Application["SMCommands"] = lsmc;

			// Load class builder specs into memory for usage later
			// Used for creating objects that are used to call user commands on
			ClassBuilderSpecs cbs = new ClassBuilderSpecs();
			string specsPath = FilePathSystem.GetFilePath("Commands", "ClassBuilder");

			if (File.Exists(specsPath))
			{
				using (StreamReader r = new StreamReader(specsPath))
				{
					string json = r.ReadToEnd();

					cbs = JsonConvert.DeserializeObject<ClassBuilderSpecs>(json);
				}
			}

			Application["ClassBuilderSpecs"] = cbs;

			// Load the Skills into memory for usage later
			// Used for both parsing commands sent in and also for help output
			List<SMSkill> lsk = new List<SMSkill>();

			// Get all filenames from path
			string skillFolderFilePath = FilePathSystem.GetFilePathFromFolder("Skills");
			DirectoryInfo d = new DirectoryInfo(skillFolderFilePath);//Assuming Test is your Folder
			FileInfo[] Files = d.GetFiles();
			foreach (FileInfo file in Files)
			{
				string skillFilePath = FilePathSystem.GetFilePath("Skills", file.Name, "");
				// Use a stream reader to read the file in (based on the path)
				using (StreamReader r = new StreamReader(skillFilePath))
				{
					// Create a new JSON string to be used...
					string json = r.ReadToEnd();

					// ... get the information from the help file
					lsk.Add(JsonConvert.DeserializeObject<SMSkill>(json));
				}
			}

			Application["SMSkills"] = lsk;

			// Load the Receipes into memory for usage later
			// Used for both parsing commands sent in and also for help output
			List<SMReceipe> smrl = new List<SMReceipe>();

			// Get all filenames from path
			string receipeFolderFilePath = FilePathSystem.GetFilePathFromFolder("Receipe");
			d = new DirectoryInfo(receipeFolderFilePath);//Assuming Test is your Folder
			Files = d.GetFiles();
			foreach (FileInfo file in Files)
			{
				string receipeFilePath = FilePathSystem.GetFilePath("Receipe", file.Name, "");
				// Use a stream reader to read the file in (based on the path)
				using (StreamReader r = new StreamReader(receipeFilePath))
				{
					// Create a new JSON string to be used...
					string json = r.ReadToEnd();

					// ... get the information from the help file
					smrl.Add(JsonConvert.DeserializeObject<SMReceipe>(json));
				}
			}

			Application["SMReceipes"] = smrl;
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{
			
		}

        protected void Application_Disposed(object sender, EventArgs e)
        {
            // If the application closes flush everything to disk.
            //List<SMCharacter> smcl = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];

            //foreach (SMCharacter smc in smcl)
            //{
            //    smc.SaveToFile();
            //}

            //// TODO Save Rooms
            //List<SMRoom> smrl = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];

            //foreach (SMRoom smr in smrl)
            //{
            //    smr.SaveToFile();
            //}
        }

    }
}