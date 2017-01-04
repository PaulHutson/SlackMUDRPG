using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMCommand
	{
		[JsonProperty("CommandFamily")]
		public string CommandFamily { get; set; }

		[JsonProperty("CommandBaseName")]
		public string CommandBaseName { get; set; }

		[JsonProperty("CommandName")]
		public string CommandName { get; set; }

		[JsonProperty("CommandDescription")]
		public string CommandDescription { get; set; }

		[JsonProperty("CommandSyntax")]
		public string CommandSyntax { get; set; }

		[JsonProperty("ExampleUsage")]
		public string ExampleUsage { get; set; }

		[JsonProperty("RequiredSkill")]
		public string RequiredSkill { get; set; }
	}

	public class SMCommands
	{
		[JsonProperty("SMCommandList")]
		List<SMCommand> SMCommandList { get; set; }
	}
}