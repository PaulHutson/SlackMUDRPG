﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class DefaultOutputFormatter : OutputFormatter
	{
		public override string NewLine { get { return "\n"; } }


		public override string Bold(string text, int newlines = 2)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}

		public override string CodeBlock(string text, int newlines = 1)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}

		public override string General(string text, int newlines = 1)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}

		public override string Italic(string text, int newlines = 1)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}

		public override string ListItem(string text, int newlines = 1)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}
	}
}