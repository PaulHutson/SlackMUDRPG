using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandClasses
{
	public class SMBodyPart
	{
		[JsonProperty("Name")]
		public string Name { get; set; }
	}
}