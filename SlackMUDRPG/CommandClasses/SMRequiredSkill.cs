using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMRequiredSkill
	{
		[JsonProperty("SkillName")]
		public string SkillName { get; set; }

		[JsonProperty("SkillLevel")]
		public string SkillLevel { get; set; }
	}
}