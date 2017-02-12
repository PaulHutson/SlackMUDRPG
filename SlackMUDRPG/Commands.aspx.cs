using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SlackMUDRPG
{
	public partial class Commands : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Prepare a string for output later
			string outputString = "";

			// Get all the loaded receipes
			List<SMCommand> smrl = (List<SMCommand>)Application["SMCommands"];

			// Get the receipe template html
			string template = "";

			string templatesPath = FilePathSystem.GetRootFilePath("HTMLTemplates", "CommandTemplate", ".html");

			if (File.Exists(templatesPath))
			{
				using (StreamReader r = new StreamReader(templatesPath))
				{
					template = r.ReadToEnd();
				}
			}

			// Sort the list alphabetically
			smrl = smrl.OrderBy(x => x.CommandFamily).ToList();

			// Scroll around those
			foreach (SMCommand command in smrl)
			{

				string baseCommandName = command.CommandName;
				if (baseCommandName.Contains(','))
				{
					string[] splitCommandName = baseCommandName.Split(',');
					baseCommandName = splitCommandName[0];
				}

				string commandItem = template;
				// replace all {name|lower} items with the base name
				commandItem = commandItem.Replace("{name|lower}", "<i>" + baseCommandName + "</i>");

				// Get each item and build it into the relevant string
				commandItem = commandItem.Replace("{Family}", command.CommandFamily);
				commandItem = commandItem.Replace("{Title}", command.CommandName);
				commandItem = commandItem.Replace("{Description}", command.CommandDescription);
				if (command.RequiredSkill != null)
				{
					commandItem = commandItem.Replace("{RequiredSkills}", command.RequiredSkill);
				}
				else
				{
					commandItem = commandItem.Replace("{RequiredSkills}", "None");
				}
				commandItem = commandItem.Replace("{ExampleUsage}", command.ExampleUsage);
				commandItem = commandItem.Replace("{CommandSyntax}", command.CommandSyntax);

				outputString += commandItem;
			}

			// Add all the receipes to the page
			lit_CommandList.Text = outputString;
		}
	}
}