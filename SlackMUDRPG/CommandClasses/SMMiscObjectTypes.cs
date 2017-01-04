using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMMiscObjectTypes
	{
	}

	public class SMStartLocation
	{
		[JsonProperty("StartLocation")]
		public string StartLocation { get; set; }
	}
}