using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.CommandClasses
{
	public class SMCommandHelper
	{
		/// <summary>
		/// Stores the instantiating characters UserId
		/// </summary>
		private string UserID;

		/// <summary>
		/// Stores the trimmed user emter command string.
		/// </summary>
		public string userCmd;

		/// <summary>
		/// Stores the determined command name.
		/// </summary>
		public string cmdName;

		/// <summary>
		/// Stores an instance of the SMCommand object defined by cmdName.
		/// </summary>
		private SMCommand cmd;

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="userCmd">User entered command string.</param>
		public SMCommandHelper(string UserID, string userCmd)
		{
			this.UserID = UserID;

			// trims whitespace or leading /
			char[] trimFromStart = { '/' };
			this.userCmd = Utils.CleanString(userCmd, trimFromStart);

			this.cmdName = this.GetCommandName(this.userCmd);

			this.cmd = this.GetCommandByName(this.cmdName);
		}

		/// <summary>
		///  Determines if a command exists by checking the cmd is not null;
		/// </summary>
		/// <returns>True if the command exists otherwise false.</returns>
		public bool CommandExists()
		{
			if (this.cmd != null)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Check if the character has the appropriate access level to execute the command.
		/// </summary>
		/// <returns>True if the character is allowed to execute the command otherwise false.</returns>
		public bool CharacterCanExecuteCommand()
		{
			// TODO check characters access level against the commands required access level
			return true;
		}

		/// <summary>
		/// Builds and return the SMParsedCommand object based on the users input.
		/// </summary>
		/// <returns>SMParsedCommand object.</returns>
		public SMParsedCommand GetParsedCommand()
		{
			SMParsedCommand parsedCmd = new SMParsedCommand();

			parsedCmd.CommandName = this.cmdName;
			parsedCmd.Command = this.cmd;
			parsedCmd.Parameters = this.GetCommandParameters();

			return parsedCmd;
		}

		/// <summary>
		/// Gets the command name form the cmdString by getting all characters upto the first space in the string.
		/// </summary>
		/// <param name="cmdString">The clened cmdString.</param>
		/// <returns>Name of the command to run.</returns>
		private string GetCommandName(string cmdString)
		{
			int firstSpacePos = cmdString.IndexOf(" ", StringComparison.CurrentCulture);

			if (firstSpacePos < 0)
			{
				firstSpacePos = cmdString.Length;
			}

			return cmdString.Substring(0, firstSpacePos).ToLower();
		}

		/// <summary>
		/// Gets the SMCommand object for a given comand name (checking command aliases).
		/// </summary>
		/// <param name="cmdName">The command name to search for.</param>
		/// <returns>SMCommand or null.</returns>
		private SMCommand GetCommandByName(string cmdName)
		{
			List<SMCommand> commands = (List<SMCommand>)HttpContext.Current.Application["SMCommands"];

			SMCommand command = commands.FirstOrDefault(cmd =>
			{
				List<string> names = cmd.CommandName.Split(',').ToList();

				if (names.FirstOrDefault(name => name.Trim().ToLower() == cmdName.ToLower()) != null)
				{
					return true;
				}

				return false;
			});

			return command;
		}

		/// <summary>
		/// Extracts parameters from the userCmd, taking into account the command spec.
		/// </summary>
		/// <returns>Array of objects rpresenting the commands parameters.</returns>
		private object[] GetCommandParameters()
		{
			// Create a new generic object list to hold the commands parameters
			List<object> parameters = new List<object>();

			// Handle passing the CommandName as the first arg if set in the SMCommand object
			if (this.cmd.PassCommandAsFirstArg)
			{
				parameters.Add(this.cmdName);
			}

			// Handle passing the UserID as the first arg if set in the SMCommand object
			if (this.cmd.PassUserIDAsFirstArg)
			{
				parameters.Add(this.UserID);
			}

			// Get the Regex form the Command object to extract params from the user command string
			string extractPattern = this.cmd.ParseExpression();

			// Get the Match from runnong the extraction Regex
			Match match = Regex.Match(this.userCmd, extractPattern);

			// Check we have a match and process the matches capturing groups
			if (match.Success)
			{
				foreach (Group group in match.Groups)
				{
					// Onlt process groups that aren't empty or the full command string
					// the first matching group is always the full command string
					if (group.Value != this.userCmd && group.Value != "")
					{
						parameters.Add(group.Value);
					}
				}
			}

			// return the List og param as an array
			return parameters.ToArray();
		}
	}
}