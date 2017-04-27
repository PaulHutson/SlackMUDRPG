using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class ResponseFormatterDefault : ResponseFormatter
	{
		/// <inheritdoc />
		public override string NewLine { get { return "\n"; } }

		/// <inheritdoc />
		public override string Bold(string text, int newlines = 0)
		{
			return $"[b]{text}[/b]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string CodeBlock(string text, int newlines = 0)
		{
			return $"[code]{text}[/code]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string Color(string text, string color, int newlines = 0)
		{
			return $"[color={color}]{text}[/color]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string General(string text, int newlines = 0)
		{
			return $"{text}" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string Italic(string text, int newlines = 0)
		{
			return $"[i]{text}[/i]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string ListItem(string text, int newlines = 0)
		{
			return $"[li]{text}[/li]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string Size(string text, int size, int newlines = 0)
		{
			return $"[size={size}]{text}[/size]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string Strike(string text, int newlines = 0)
		{
			return $"[s]{text}[/s]" + this.GetNewLines(newlines);
		}

		/// <inheritdoc />
		public override string Underline(string text, int newlines = 0)
		{
			return $"[u]{text}[/u]" + this.GetNewLines(newlines);
		}
	}
}