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
	public partial class Receipes : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Prepare a string for output later
			string outputString = "";

			// Get all the loaded receipes
			List<SMReceipe> smrl = (List<SMReceipe>)Application["SMReceipes"];

			// Get the receipe template html
			string template = "";

			string templatesPath = FilePathSystem.GetRootFilePath("HTMLTemplates", "ReceipeTemplate", ".html");

			if (File.Exists(templatesPath))
			{
				using (StreamReader r = new StreamReader(templatesPath))
				{
					template = r.ReadToEnd();
				}
			}

			// Sort the list alphabetically
			smrl = smrl.OrderBy(x => x.Name).ToList();

			// Scroll around those
			foreach (SMReceipe receipe in smrl)
			{
				string receipeItem = template;
				// Get each item and build it into the relevant string
				receipeItem = receipeItem.Replace("{Title}", receipe.Name);
				receipeItem = receipeItem.Replace("{Description}", receipe.Description);

				string skillsRequired = "";
				bool isFirst = true;
				foreach(SMSkillHeld requiredSkill in receipe.RequiredSkills)
				{
					if (isFirst)
					{
						isFirst = false;
					}
					else
					{
						skillsRequired += ", ";
					}
					skillsRequired += requiredSkill.SkillName + "(" + requiredSkill.SkillLevel + ")";
				}
				receipeItem = receipeItem.Replace("{RequiredSkills}", skillsRequired);

				string materialsRequired = "";
				isFirst = true;
				foreach (SMReceipeMaterial requiredMaterial in receipe.Materials)
				{
					if (isFirst)
					{
						isFirst = false;
					}
					else
					{
						materialsRequired += ", ";
					}
					materialsRequired += requiredMaterial.MaterialType + "(" + requiredMaterial.MaterialQuantity + ")";
				}
				receipeItem = receipeItem.Replace("{RequiredMaterials}", materialsRequired);

				receipeItem = receipeItem.Replace("{NeedToLearn}", receipe.NeedToLearn.ToString());

				outputString += receipeItem;
			}
			
			// Add all the receipes to the page
			lit_ReceipeList.Text = outputString;
		}
	}
}