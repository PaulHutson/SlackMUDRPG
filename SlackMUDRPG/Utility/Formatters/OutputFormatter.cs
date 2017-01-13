using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public abstract class OutputFormatter
	{
		/// <summary>
		/// New line character
		/// </summary>
		public abstract string NewLine { get; }

		/// <summary>
		/// /// Formats a title text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string Bold(string text, int newlines = 1);

		/// <summary>
		/// Formats a text string as a code block.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string CodeBlock(string text, int newlines = 1);

		/// <summary>
		/// Formats a general text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string General(string text, int newlines = 1);

		/// <summary>
		/// Formats an announcement text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string Italic(string text, int newlines = 1);

		/// <summary>
		/// Formats a list item text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string ListItem(string text, int newlines = 1);

		/// <summary>
		/// Gets a string by repeating a number of new line characters
		/// </summary>
		/// <param name="repeat"></param>
		/// <returns></returns>
		protected string GetNewLines(int repeat)
		{
			if (repeat == 0)
			{
				return "";
			}

			return String.Concat(Enumerable.Repeat(this.NewLine, repeat));
		}
	}
}