using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public abstract class ResponseFormatter
	{
		/// <summary>
		/// New line sequence.
		/// </summary>
		public abstract string NewLine { get; }

		/// <summary>
		/// /// Adds bold formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string Bold(string text, int newlines = 0);

		/// <summary>
		/// Adds code block formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string CodeBlock(string text, int newlines = 0);

		/// <summary>
		/// Adds color formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string Color(string text, string color, int newlines = 0);

		/// <summary>
		/// Adds general formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string General(string text, int newlines = 0);

		/// <summary>
		/// Adds itallic formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string Italic(string text, int newlines = 0);

		/// <summary>
		/// Adds list item formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string ListItem(string text, int newlines = 0);

		/// <summary>
		/// Adds size formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string Size(string text, int size, int newlines = 0);

		/// <summary>
		/// Adds strikthrough formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string Strike(string text, int newlines = 0);

		/// <summary>
		/// Adds underline formatting tags around a given string, and a given number of new line sequences after it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Tagged text string.</returns>
		public abstract string Underline(string text, int newlines = 0);

		/// <summary>
		/// Gets a string representing a given number of new line sequences.
		/// </summary>
		/// <param name="repeat"></param>
		/// <returns>String of new line sequences.</returns>
		public string GetNewLines(int repeat)
		{
			if (repeat == 0)
			{
				return "";
			}

			return String.Concat(Enumerable.Repeat(this.NewLine, repeat));
		}
	}
}