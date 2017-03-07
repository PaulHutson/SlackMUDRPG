using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class ResponseFormatterDefault : ResponseFormatter
	{
		public override string NewLine { get { return "\n"; } }

		public override string Bold(string text, int newlines = 2)
		{
			return $"[b]{text}[/b]" + this.GetNewLines(newlines);
		}

		public override string CodeBlock(string text, int newlines = 1)
		{
			return $"[code]{text}[/code]" + this.GetNewLines(newlines);
		}

		public override string Color(string text, string color, int newlines = 1)
		{
			return $"[color={color}]{text}[/color]" + this.GetNewLines(newlines);
		}

		public override string General(string text, int newlines = 1)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}

		public override string Italic(string text, int newlines = 1)
		{
			return $"[i]{text}[/i]" + this.GetNewLines(newlines);
		}

		public override string ListItem(string text, int newlines = 1)
		{
			return $"[li]{text}[/li]" + this.GetNewLines(newlines);
		}

		public override string Size(string text, int size, int newlines = 1)
		{
			return $"[size={size}]{text}[/size]" + this.GetNewLines(newlines);
		}

		public override string Strike(string text, int newlines = 1)
		{
			return $"[s]{text}[/s]" + this.GetNewLines(newlines);
		}

		public override string Underline(string text, int newlines = 1)
		{
			return $"[u]{text}[/u]" + this.GetNewLines(newlines);
		}
	}
}