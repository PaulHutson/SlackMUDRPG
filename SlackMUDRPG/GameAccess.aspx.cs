using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;

namespace SlackMUDRPG
{
	public partial class GameAccess : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Command text (this works for both form submissions and also query strings)
			string commandText = Utils.GetQueryParam("text");

            // TODO Validate user command before threading the command
            if (this.GetCommandByName(this.GetCommandNameFromString(commandText))!=null)
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
                SMCharacter smc = new SlackMud().GetCharacter(Utils.GetQueryParam("user_id"));
                string commandNotRecognisedMessage = "Command \"" + commandText + "\" not recognised, please check and try again";
                lit_output.Text = commandNotRecognisedMessage;
            }
		}

		/// <summary>
		/// Processes the user command, running the action if the command is vaid.
		/// </summary>
		/// <param name="cmd">User entered command string.</param>
		private void ProcessUserCommand(string cmd)
		{
			SMParsedCommand parsedCmd = this.ParseCommandString(cmd);

			if (parsedCmd != null)
			{
				this.RunAction(parsedCmd);
			}
        }

		/// <summary>
		/// Parses the user enterd command string to produced a SMParsedCommand object to run.
		/// </summary>
		/// <returns>SMParsedCommand object to run.</returns>
		/// <param name="cmdString">User entered command string.</param>
		/// <thorws>NullReferenceException if command not found.
		private SMParsedCommand ParseCommandString(string cmdString)
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
			string commandName = this.GetCommandNameFromString(cmdString);

			// Get the SMCommmand object for the given command name
			SMCommand command = this.GetCommandByName(commandName);

			// If the command if found extra the params and return the parsed command object.
			if (command != null)
			{
				object[] parameters = this.GetParamsFromCommandString(command, cmdString);

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
		private string GetCommandNameFromString(string cmdString)
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
		private SMCommand GetCommandByName(string name)
		{
			SMCommands commands = (SMCommands)HttpContext.Current.Application["SMCommands"];

			SMCommand command = commands.SMCommandList.FirstOrDefault(cmd => cmd.CommandName == name);

			return command;
		}

		/// <summary>
		/// Gets the parameters from command string based on the expression in the corresponding SMCommand object
		/// </summary>
		/// <returns>List of extrated parameters to use when running the user command</returns>
		/// <param name="command">SMCommand object.</param>
		/// <param name="cmdString">User entered command string.</param>
		private object[] GetParamsFromCommandString(SMCommand command, string cmdString)
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
		private void RunAction(SMParsedCommand command)
		{
			// Get the Class name of the command class for use in the ClassBuilder
			string commandClassName = command.Command.CommandClass.Split('.').Last();

			// Get class instance from ClassBuilder to run the command with
			ClassBuilder cb = new ClassBuilder(commandClassName);

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
}