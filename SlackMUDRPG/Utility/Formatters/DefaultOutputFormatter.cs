using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class DefaultOutputFormatter : OutputFormatter
	{
		public override string Announcement(string text)
		{
			return text;
		}

		public override string General(string text)
		{
			return text;
		}

		public override string ListItem(string text)
		{
			return text;
		}

		public override string Title(string text)
		{
			return text;
		}
	}
}