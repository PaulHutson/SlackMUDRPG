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
	public partial class Skills : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			skillListings.Text = this.GetSkillListingsHtml();
		}

		/// <summary>
		/// Gets the HTML for all skill listings
		/// </summary>
		/// <returns>String of HTML skill entrires</returns>
		private string GetSkillListingsHtml()
		{
			string html = String.Empty;

			foreach (SMSkill skill in this.GetSkills())
			{
				html += this.GetSkillListingHtml(skill);
			}

			return html;
		}

		/// <summary>
		/// Get the HTML listing for a given skill
		/// </summary>
		/// <param name="skill">The skill to get the listing for</param>
		/// <returns>String of HTML represting the skill listing</returns>
		private string GetSkillListingHtml(SMSkill skill)
		{
			string guid = Guid.NewGuid().ToString();
			string template = Utils.GetHtmlTemplate("SkillTemplate");

			template = template.Replace("{panelId}", guid);
			template = template.Replace("{SkillName}", skill.SkillName);
			template = template.Replace("{Description}", skill.SkillDescription);
			template = template.Replace("{BaseStat}", Utils.noneIfNull(this.GetStatName(skill.BaseStat)));
			template = template.Replace("{SkillPrerequisites}", Utils.noneIfNull(this.GetSkillPrerequisitesHtml(skill)));
			template = template.Replace("{SkillNotes}", Utils.noneIfNull(this.GetSkillNotesHtml(skill)));

			return template;
		}

		/// <summary>
		/// Gets a list of all skills loaded in the current application context
		/// </summary>
		/// <returns>List of SMSKill objects</returns>
		private List<SMSkill> GetSkills()
		{
			List<SMSkill> smsl = (List<SMSkill>)Application["SMSkills"];

			return smsl.OrderBy(skill => skill.SkillName).ToList();
		}

		/// <summary>
		/// Gets the fullname of a stat/attribute based on its abbrviated short code (case insensitive)
		/// </summary>
		/// <param name="statAbbrv">Stat/Attribute abbrviated short code</param>
		/// <returns>Fullname of the stat/attribute</returns>
		private string GetStatName(string statAbbrv)
		{
			switch (statAbbrv.ToLower())
			{
				case "str":
					return "Strength";
				case "int":
					return "Intelligence";
				case "chr":
					return "Charisma";
				case "dex":
					return "Dexterity";
				case "wp":
					return "Will Power";
				case "ft":
					return "Fortitude";
				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the HTML for a given skills prerequisites
		/// </summary>
		/// <param name="skill">The skill to get prerequisites for</param>
		/// <returns>String of HTML represting the skill prerequisites</returns>
		private string GetSkillPrerequisitesHtml(SMSkill skill)
		{
			if (skill.Prerequisites == null || !skill.Prerequisites.Any())
			{
				return null;
			}

			List<string> prereqs = new List<string>();

			foreach (SMSkillPrerequisite prereq in skill.Prerequisites)
			{
				if (prereq.IsSkill)
				{
					prereqs.Add($"Skill: {prereq.SkillStatName}, Lvl {prereq.PreReqLevel}");
				}
				else
				{
					prereqs.Add($"Attribute: {this.GetStatName(prereq.SkillStatName)}, Lvl {prereq.PreReqLevel}");
				}
			}

			return String.Join("<br/>", prereqs.OrderBy(p => p).ToArray());
		}

		/// <summary>
		/// Gets the HTML for any notes relating to a given skill
		/// </summary>
		/// <param name="skill">The skill to get notes for</param>
		/// <returns>String of HTML notes or null</returns>
		private string GetSkillNotesHtml(SMSkill skill)
		{
			List<string> notes = new List<string>();

			if (skill.ActivityType.ToLower() == "passive")
			{
				notes.Add("This skill is passive, you cannot use it directly.");
			}

			if (skill.CanUseWithoutLearning == false)
			{
				notes.Add("You must learn this skill before you can use it.");
			}

			if (!notes.Any())
			{
				return null;
			}

			return String.Join("<br/>", notes.ToArray());
		}
	}
}