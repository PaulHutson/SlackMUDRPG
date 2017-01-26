using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

		[JsonProperty("CommandExpression")]
		public string CommandExpression { get; set; }

		[JsonProperty("CommandClass")]
		public string CommandClass { get; set; }

		[JsonProperty("CommandMethod")]
		public string CommandMethod { get; set; }

		[JsonProperty("PassCommandAsFirstArg")]
		public bool PassCommandAsFirstArg { get; set; }

		[JsonProperty("PassQueryParamAsFirstArg")]
		public string PassQueryParamAsFirstArg { get; set; }

		[JsonProperty("ExampleUsage")]
		public string ExampleUsage { get; set; }

		[JsonProperty("RequiredSkill")]
		public string RequiredSkill { get; set; }

		/// <summary>
		/// Parses the CommandExpression into a Regex pattern to extra params from the users command, e.g.
		/// equip {.+} from {.+}? becomes ^equip (?:equip (.+?)(?: from |$)(.+)?)$
		/// </summary>
		/// <returns>The parsed regex patthern string</returns>
		public string ParseExpression()
		{
			string pattern = @"(?:\{([^}]+)\}(\?)?)(?:$|)";

			MatchCollection matches = Regex.Matches(this.CommandExpression, pattern);

			string ret = @"";

			for (int i = 0; i < matches.Count; i++)
			{
				// handles adding the line start, command and opens a non-capturing group
				if (i == 0)
				{
					ret += "^" + this.CommandExpression.Substring(0, matches[i].Index);
					ret += "(?:";
				}

				Match current = matches[i];
				Match next = null;

				if (i < matches.Count - 1)
				{
					next = matches[i].NextMatch();
				}

				if (i < matches.Count - 1)
				{
					next = matches[i].NextMatch();
				}

				// handles the last group in the expression
				if (next == null)
				{
					ret += "(" + current.Groups[1].Value + ")";

					if (current.Groups[2].Value == "?")
					{
						ret += "?";
					}
				}
				// handles all other matches and sections between params like ' from '
				else
				{
					ret += "(" + current.Groups[1].Value + "?)";

					if (current.Groups[2].Value == "?")
					{
						ret += "?";
					}

					int start = current.Index + current.Length;
					int length = next.Index - (current.Index + current.Length);
					ret += "(?:" + this.CommandExpression.Substring(start, length);

					// if next group is optional
					if (next.Groups[2].Value == "?")
					{
						ret += "|$";
					}

					ret += ")";
				}

				// handles closing the initial non-capturing group and adding the line end
				if (next == null)
				{
					ret += ")$";
				}
			}

			return ret;
		}
	}

	public class SMCommands
	{
		[JsonProperty("SMCommandList")]
		public List<SMCommand> SMCommandList { get; set; }
	}

	public class SMParsedCommand
	{
		public string CommandName;

		public SMCommand Command;

		public object[] Parameters;
	}
}