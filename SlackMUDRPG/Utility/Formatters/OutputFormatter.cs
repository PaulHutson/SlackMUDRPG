using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace SlackMUDRPG.Utility.Formatters
{
	public abstract class OutputFormatter
	{
		/// <summary>
		/// Processes a given string for output by replacing tags and formatting new lines correctly.
		/// </summary>
		/// <param name="output">The output string to format.</param>
		/// <returns>The formatted output string.</returns>
		public string ProcessOutput(string output)
		{
			output = this.ReplaceNewLines(output);

			output = this.ProcessSimpleTags(output);

			output = this.ProcessAdvancedTags(output);

			return output;
		}

		/// <summary>
		/// Replaces new lines (\n) with the new line sequence for the target platform.
		/// </summary>
		/// <param name="str">The string to replace new lines in.</param>
		/// <returns>The string with new lines replaced.</returns>
		private string ReplaceNewLines(string str)
		{
			return str.Replace("\n", this.GetNewLineSequence()).Replace("[n]", this.GetNewLineSequence());
		}

		/// <summary>
		/// Processes simple tags that do not have params such as [b]text[/b] for output.
		/// </summary>
		/// <param name="str">The string to process simple tags in.</param>
		/// <returns>Prces string with simple tags replaced for output.</returns>
		private string ProcessSimpleTags(string str)
		{
			Regex tagMatcher = new Regex(@"\[(b|code|i|li|s|u)\](.+?)?\[\/\1\]");

			Match match = tagMatcher.Match(str);

			String replacePattern;

			while (tagMatcher.IsMatch(str))
			{
				switch (match.Groups[1].Value)
				{
					case "b":
						replacePattern = this.GetBoldReplacePattern();
						break;
					case "code":
						replacePattern = this.GetCodeBlockReplacePattern();
						break;
					case "i":
						replacePattern = this.GetItalicReplacePattern();
						break;
					case "li":
						replacePattern = this.GetListItemReplacePattern();
						break;
					case "s":
						replacePattern = this.GetStrikeReplacePattern();
						break;
					case "u":
						replacePattern = this.GetUnderlineReplacePattern();
						break;
					default:
						replacePattern = "$2";
						break;
				}

				str = tagMatcher.Replace(str, replacePattern, 1);

				match = tagMatcher.Match(str);
			}

			return str;
		}

		/// <summary>
		/// Processes advanced tags that have params such as [color=#fff]text[/color] for output.
		/// </summary>
		/// <param name="str">The string to process advanced tags in.</param>
		/// <returns>Prces string with advanced tags replaced for output.</returns>
		private string ProcessAdvancedTags(string str)
		{
			Regex tagMatcher = new Regex(@"\[(color|size)=(.+)\](.+?)?\[\/\1\]");

			Match match = tagMatcher.Match(str);

			String replacePattern;

			while (tagMatcher.IsMatch(str))
			{
				switch (match.Groups[1].Value)
				{
					case "color":
						replacePattern = this.GetColorReplacePattern();
						break;
					case "size":
						replacePattern = this.GetSizeReplacePattern();
						break;
					default:
						replacePattern = "$3";
						break;
				}

				str = tagMatcher.Replace(str, replacePattern);

				match = tagMatcher.Match(str);

			}

			return str;
		}

		/// <summary>
		/// Get the regex replacement pattern for bold output, [b][/b] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetBoldReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for code block output, [code][/code] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetCodeBlockReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for colored output, [color=#ffffff][/color] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetColorReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for italic output, [i][/i] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetItalicReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for list item output, [li][/li] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetListItemReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for sized output, [size=100][/size] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetSizeReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for stuck through output, [s][/s] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetStrikeReplacePattern();

		/// <summary>
		/// Get the regex replacement pattern for underlined output, [s][/s] tags.
		/// </summary>
		/// <returns>The replacement pattern string.</returns>
		protected abstract string GetUnderlineReplacePattern();

		/// <summary>
		/// Gets the new line sequence used to replace \n in the output.
		/// </summary>
		/// <returns>The new line sequence.</returns>
		protected abstract string GetNewLineSequence();
	}
}