using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;
using System.Threading;

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

	public class SPCommandUtility
	{
		public string UserID { get; set; }

		public SPCommandUtility(string userID)
		{
			this.UserID = userID;
		}

		public string InitateCommand(string commandText)
		{
			// Return string information
			string returnString = "";

			// TODO Validate user command before threading the command
			if (this.GetCommandByName(this.GetCommandNameFromString(commandText)) != null)
			{
				HttpContext ctx = HttpContext.Current;

				Thread commandThread = new Thread(new ThreadStart(() =>
				{
					HttpContext.Current = ctx;
					this.ProcessUserCommand(commandText);
				}));

				commandThread.Start();
			}
			else
			{
				SMCharacter smc = new SlackMud().GetCharacter(this.UserID);
				returnString = "Command \"" + commandText + "\" not recognised, please check and try again";
			}

			return returnString;
		}

		/// <summary>
		/// Processes the user command, running the action if the command is vaid.
		/// </summary>
		/// <param name="cmd">User entered command string.</param>
		public void ProcessUserCommand(string cmd)
		{
			SMParsedCommand parsedCmd = ParseCommandString(cmd);

			if (parsedCmd != null)
			{
				RunAction(parsedCmd);
			}
		}

		/// <summary>
		/// Parses the user enterd command string to produced a SMParsedCommand object to run.
		/// </summary>
		/// <returns>SMParsedCommand object to run.</returns>
		/// <param name="cmdString">User entered command string.</param>
		/// <thorws>NullReferenceException if command not found.
		public SMParsedCommand ParseCommandString(string cmdString)
		{
			if (cmdString == null)
			{
				return null;
			}

			SMParsedCommand cmd = new SMParsedCommand();

			// Trim white space and remove leading /
			char[] trimFromStart = { '/' };
			cmdString = Utils.CleanString(cmdString, trimFromStart);

			// Get the and of the command e.g. "shout"
			string commandName = GetCommandNameFromString(cmdString);

			// Get the SMCommmand object for the given command name
			SMCommand command = GetCommandByName(commandName);

			// If the command if found extra the params and return the parsed command object.
			if (command != null)
			{
				object[] parameters = GetParamsFromCommandString(command, cmdString);

				cmd.CommandName = commandName;
				cmd.Command = command;
				cmd.Parameters = parameters;

				return cmd;
			}

			// Throw and exception if unable to find the command
			throw new System.NullReferenceException("Command specified command not found.");
		}

		/// <summary>
		/// Gets the command name from the user command string.
		/// </summary>
		/// <returns>The command name..</returns>
		/// <param name="cmdString">User entered command string..</param>
		public string GetCommandNameFromString(string cmdString)
		{
			int spacePos = cmdString.IndexOf(" ", StringComparison.CurrentCulture);

			if (spacePos < 0)
			{
				spacePos = cmdString.Length;
			}

			return cmdString.Substring(0, spacePos);
		}

		/// <summary>
		/// Gets an SMCommand object for the command by name.
		/// </summary>
		/// <returns>SMCommand object or null</returns>
		/// <param name="name">Name of the command.</param>
		public SMCommand GetCommandByName(string name)
		{
			List<SMCommand> commands = (List<SMCommand>)HttpContext.Current.Application["SMCommands"];

			SMCommand command = commands.FirstOrDefault(cmd => cmd.CommandName == name);

			return command;
		}

		/// <summary>
		/// Gets the parameters from command string based on the expression in the corresponding SMCommand object
		/// </summary>
		/// <returns>List of extrated parameters to use when running the user command</returns>
		/// <param name="command">SMCommand object.</param>
		/// <param name="cmdString">User entered command string.</param>
		public object[] GetParamsFromCommandString(SMCommand command, string cmdString)
		{
			// Create a new generic object list to hold the commands parameters
			List<object> parameters = new List<object>();

			// Handle passing the CommandName as the first arg if set in the SMCommand object
			if (command.PassCommandAsFirstArg)
			{
				parameters.Add(command.CommandName);
			}

			// Handle passing a defined Form/QueryString param as the first arg if set in the SMCommand object
			if (command.PassQueryParamAsFirstArg != null)
			{
				parameters.Add(Utils.GetQueryParam(command.PassQueryParamAsFirstArg));
			}

			// Get the Regex form the Command object to extract params from the user command string
			string extractPattern = command.ParseExpression();

			// Get the Match from runnong the extraction Regex
			Match match = Regex.Match(cmdString, extractPattern);

			// Check we have a match and process the matches capturing groups
			if (match.Success)
			{
				foreach (Group group in match.Groups)
				{
					// Onlt process groups that aren't empty or the full command string
					// the first matching group is always the full command string
					if (group.Value != cmdString && group.Value != "")
					{
						parameters.Add(group.Value);
					}
				}
			}

			return parameters.ToArray();
		}

		/// <summary>
		/// Runs the parsed command.
		/// </summary>
		/// <param name="command">Parsed Command.</param>
		public void RunAction(SMParsedCommand command)
		{
			// Get the Class name of the command class for use in the ClassBuilder
			string commandClassName = command.Command.CommandClass.Split('.').Last();

			// Get class instance from ClassBuilder to run the command with
			ClassBuilder cb = new ClassBuilder(commandClassName, this.UserID);

			object commandClass = cb.GetClassInstance();

			// if null returned by ClassBuilder
			if (commandClass == null)
			{
				Utils.CallUserFuncArray(
					command.Command.CommandClass,
					command.Command.CommandMethod,
					command.Parameters
				);
			}
			// Using instance returned by the ClassBuilder
			else
			{
				Utils.CallUserFuncArray(
					commandClass,
					command.Command.CommandMethod,
					command.Parameters
				);
			}
		}
	}

	public class SMParsedCommand
	{
		public string CommandName;

		public SMCommand Command;

		public object[] Parameters;
	}
}