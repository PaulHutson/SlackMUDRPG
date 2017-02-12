using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;
using System.Threading;

namespace SlackMUDRPG.CommandClasses
{
	public class SMCommandUtility
	{
		/// <summary>
		/// Stores the characters UserID.
		/// </summary>
		private string UserID { get; set; }
		
		/// <summary>
		/// Stores an instance of SMCommandHelper to process the command.
		/// </summary>
		private SMCommandHelper CmdHelper;

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="userID"></param>
		public SMCommandUtility(string userID)
		{
			this.UserID = userID;
		}

		/// <summary>
		/// Handles initialisation of the users command on a new thread if the command is valid.
		/// </summary>
		/// <param name="commandText"></param>
		/// <returns>Empyt string if the command was passed off to a new thread, otherwise an error meesage.</returns>
		public string InitateCommand(string commandText)
		{
			// Replay the last used command if the command text is a !
			if (commandText == "!")
			{
				SMCharacter smc = new SlackMud().GetCharacter(this.UserID);

				if (smc != null)
				{
					string previousCommand = smc.GetLastUsedCommand();
					if (previousCommand != null)
					{
						return new SMCommandUtility(this.UserID).InitateCommand(previousCommand);
					}

					return this.GetCommandNotFoundMsg(commandText);
				}
			}

			// Instantiate a new command helper.
			this.CmdHelper = new SMCommandHelper(this.UserID, commandText);

			// Validate command exists and the character has the appropriate access level to execute it before threading.
			if (this.CmdHelper.CommandExists() && this.CmdHelper.CharacterCanExecuteCommand())
			{
				HttpContext ctx = HttpContext.Current;

				Thread commandThread = new Thread(new ThreadStart(() =>
				{
					HttpContext.Current = ctx;
					this.RunCommand(this.CmdHelper.GetParsedCommand());
				}));

				commandThread.Start();

				return String.Empty;
			}

			// Return a message if the command does not exist or insufficient access level to execute.
			return this.GetCommandNotFoundMsg(commandText);
		}

		/// <summary>
		/// Gets a string to send back to the player if the command they types cannto be run.
		/// </summary>
		/// <param name="command">The user entered command.</param>
		/// <returns>Command failure string.</returns>
		private string GetCommandNotFoundMsg(string command = null)
		{
			if (command == "!")
			{
				return "Unable to replay the last command, no command history found!";
			}

			if (command != null)
			{
				return $"Command \"{Utils.SanitiseString(command)}\" not recognised, please check and try again";
			}

			return "Command not recognised, please check and try again";
		}

		/// <summary>
		/// Runs the parsed command.
		/// </summary>
		/// <param name="command">Parsed Command.</param>
		private void RunCommand(SMParsedCommand command)
		{
			this.UpdateLastRunCommand();

			// Get the Class name of the command class for use in the ClassBuilder
			string commandClassName = command.Command.CommandClass.Split('.').Last();

			// Get class instance from ClassBuilder to run the command with
			ClassBuilder cb = new ClassBuilder(commandClassName, this.UserID);
			object commandClass = cb.GetClassInstance();

			// If null returned by ClassBuilder, run the command with the CommandClass name
			// this lets UserCallFuncArray handle object instantiation.
			if (commandClass == null)
			{
				Utils.CallUserFuncArray(
					command.Command.CommandClass,
					command.Command.CommandMethod,
					command.Parameters
				);
			}
			// Rm the command using the object instance returned by the ClassBuilder.
			else
			{
				Utils.CallUserFuncArray(
					commandClass,
					command.Command.CommandMethod,
					command.Parameters
				);
			}
		}

		/// <summary>
		/// Updates the character running to the command with the details of the command run.
		/// </summary>
		private void UpdateLastRunCommand()
		{
			// Exclude login commands as a player must be logged in before its character can be accessed.
			if (this.CmdHelper.cmdName != "login")
			{
				SMCharacter smc = new SlackMud().GetCharacter(this.UserID);

				if (smc != null)
				{
					smc.LastUsedCommand = this.CmdHelper.userCmd;
					smc.SaveToApplication();
				}
			}
		}
	}
}