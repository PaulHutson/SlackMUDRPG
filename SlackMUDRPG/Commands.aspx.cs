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
			commandTypesList.Text = this.GetCommandFamilyListHtml();

			commandListings.Text = this.GetCommandFamiliesListingHtml();
		}

		/// <summary>
		/// Gets the HTML for a list on command family navigation links
		/// </summary>
		/// <returns>String of HTML li elements</returns>
		private string GetCommandFamilyListHtml()
		{
			string html = String.Empty;

			foreach(string family in this.GetCommandFamilies())
			{
				html += $"<li><a href=\"#\" data-group=\"commands-{family.ToLower()}\">{family} Commands</a></li>";
			}

			return html;
		}

		/// <summary>
		/// Gets the HTML for the command listings for all command familes
		/// </summary>
		/// <returns>String of HTML command entrires</returns>
		private string GetCommandFamiliesListingHtml()
		{
			string html = String.Empty;

			List<string> families = this.GetCommandFamilies();

			foreach(string family in families)
			{
				html += $"<div class=\"info-group\" id=\"commands-{family.ToLower()}\">";
				html += this.GetCommandFamilyListingHtml(family);
				html += $"</div>";
			}

			return html;
		}

		/// <summary>
		/// Gets the HTML of command listings for a given command family
		/// </summary>
		/// <param name="family">The command family to get the listing for</param>
		/// <returns>String of HTML command entrires</returns>
		private string GetCommandFamilyListingHtml(string family)
		{
			string html = String.Empty;

			List<SMCommand> commands = this.GetCommandsForFamily(family);

			if (commands != null)
			{
				foreach(SMCommand command in commands)
				{
					string guid = Guid.NewGuid().ToString();
					string template = this.GetHtmlTemplate("CommandTemplate");

					template = template.Replace("{panelId}", guid);
					template = template.Replace("{Name}", command.CommandName);
					template = template.Replace("{Description}", command.CommandDescription);
					template = template.Replace("{RequiredSkills}", command.RequiredSkill);
					template = template.Replace("{ExampleUsage}", command.ExampleUsage);
					template = template.Replace("{CommandSyntax}", command.CommandSyntax);

					html += template;
				}
			}

			return html;
		}

		/// <summary>
		/// Gets a list of command family names based on those loaded in the current application
		/// </summary>
		/// <returns>Ordered list of command family name strings</returns>
		private List<string> GetCommandFamilies()
		{
			List<SMCommand> smcl = (List<SMCommand>)Application["SMCommands"];

			List<string> filtered = smcl.Select(cmd => cmd.CommandFamily).Distinct().ToList();

			return filtered.OrderBy(cmd => cmd).ToList();
		}

		/// <summary>
		/// Gets a list of commands in a given family based on those loadin in the current application
		/// </summary>
		/// <param name="commandFamily">Family name to get command for</param>
		/// <returns>Ordered list of SMCommand objects for the given family name</returns>
		private List<SMCommand> GetCommandsForFamily(string commandFamily)
		{
			List<SMCommand> smcl = (List<SMCommand>)Application["SMCommands"];

			List<SMCommand> filtered = smcl.Where(cmd => cmd.CommandFamily == commandFamily).ToList();

			return filtered.OrderBy(cmd => cmd.CommandName).ToList();
		}

		/// <summary>
		/// Gets an HTML template file as a string, based on the template name provided
		/// </summary>
		/// <param name="templateName">Name of the temaplte to loead</param>
		/// <returns>String representing the HTML template file contents</returns>
		private string GetHtmlTemplate(string templateName)
		{
			string template = String.Empty;

			string templatesPath = FilePathSystem.GetRootFilePath("HTMLTemplates", templateName, ".html");

			if (File.Exists(templatesPath))
			{
				using (StreamReader r = new StreamReader(templatesPath))
				{
					template = r.ReadToEnd();
				}
			}

			return template;
		}
	}
}