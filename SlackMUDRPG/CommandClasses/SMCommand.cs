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

		[JsonProperty("ParamsExpression")]
		public string ParamsExpression { get; set; }

		[JsonProperty("CommandClass")]
		public string CommandClass { get; set; }

		[JsonProperty("CommandMethod")]
		public string CommandMethod { get; set; }

		[JsonProperty("PassCommandAsFirstArg")]
		public bool PassCommandAsFirstArg { get; set; }

		[JsonProperty("ExampleUsage")]
		public string ExampleUsage { get; set; }

		[JsonProperty("RequiredSkill")]
		public string RequiredSkill { get; set; }

		/// <summary>
		/// Parses the ParamsExpression into a Regex pattern to extra params from the users command accounting for aliases, e.g.
		/// Command Name = equip; "{.+} from {.+}?" becomes "^(?:equip )(?:(.+?)(?: from |$)(.+)?)$"
		/// Command Name = attack; "{.+}" becomes "^(?:attack |hit )(?:(.+?))$"
		/// </summary>
		/// <returns>The parsed regex patthern string</returns>
		public string ParseExpression()
		{
			string pattern = @"(?:\{([^}]+)\}(\?)?)(?:$|)";

			string commandExpression = $"{this.CommandName.Split(',')[0]} {this.ParamsExpression}";

			MatchCollection matches = Regex.Matches(commandExpression, pattern);

			string ret = @"";

			for (int i = 0; i < matches.Count; i++)
			{
				// handles adding the line start, command (inc aliases) and opens a non-capturing group
				if (i == 0)
				{
					ret += "^(?:" + String.Join(" |", this.CommandName.Split(',').Select(s => s.Trim())) + " )";
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
					ret += "(?:" + commandExpression.Substring(start, length);

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

	public class SMParsedCommand
	{
		public string CommandName;

		public SMCommand Command;

		public object[] Parameters;
	}
}