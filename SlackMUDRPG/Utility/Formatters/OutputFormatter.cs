using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public abstract class OutputFormatter
	{
		/// <summary>
		/// Formats an announcement text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string Announcement(string text);

		/// <summary>
		/// Formats a general text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string General(string text);

		/// <summary>
		/// Formats a list item text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string ListItem(string text);

		/// <summary>
		/// /// Formats a title text string.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>Formatted text string.</returns>
		public abstract string Title(string text);
	}
}