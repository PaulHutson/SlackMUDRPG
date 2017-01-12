using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class SlackOutputFormatter : OutputFormatter
	{
		public override string Announcement(string text)
		{
			return $"_{text}_\n";
		}

		public override string General(string text)
		{
			return $"{text}\n";
		}

		public override string ListItem(string text)
		{
			return $"> {text}\n";
		}

		public override string Title(string text)
		{
			return $"*{text}*\n\n";
		}
	}
}