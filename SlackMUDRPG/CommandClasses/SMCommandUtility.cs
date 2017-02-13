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
			if (commandText.ToLower() != "login")
			{
				// Replay the last used command if the command text is a !
				if (commandText == "!")
				{
					return this.HandleRepeats();
				}

				// Process the commandText to check short codes for NPC responses or movement
				commandText = this.ProcessShortCodes(commandText);
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
				return "Unable to replay the last command, yout command history is empty!";
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
		/// Updates the character running to the command with the details of the command run and when.
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
					smc.LastInteractionDate = DateTime.Now;
					smc.SaveToApplication();
				}
			}
		}

		/// <summary>
		/// Repeats to last command used or returns an error message.
		/// </summary>
		/// <returns>Empty string if the previous command is successfully handed of to a new thread otherwise an error message.</returns>
		private string HandleRepeats()
		{
			SMCharacter smc = new SlackMud().GetCharacter(this.UserID);

			if (smc != null)
			{
				string previousCommand = smc.GetLastUsedCommand();
				if (previousCommand != null)
				{
					return new SMCommandUtility(this.UserID).InitateCommand(previousCommand);
				}
			}

			return this.GetCommandNotFoundMsg("!");
		}

		/// <summary>
		/// Processes the commandText checking if a short code has been given and either returning the original or the formated shortcode for processing.
		/// </summary>
		/// <param name="commandText">User enteted command text.</param>
		/// <returns>A string representing to command for standard command processing.</returns>
		private string ProcessShortCodes(string commandText)
		{
			string command;

			// First check if the command is an NPC Response
			command = this.CheckNPCResponses(commandText);

			if (command != null)
			{
				return command;
			}

			// Second check in the command is an exit shortcut
			command = this.CheckRoomExitCodes(commandText);

			if (command != null)
			{
				return command;
			}

			// Otherwise return orignal command for standard processing
			return commandText;
		}

		/// <summary>
		/// Checks if the given commandText matched a NPC interaction awaiting response.
		/// If there is a match the correctly formatted command is returned, else null is.
		/// </summary>
		/// <param name="commandText">User entered command.</param>
		/// <returns>Formatted command for procesing or null.</returns>
		private string CheckNPCResponses(string commandText)
		{
			SMCharacter smc = new SlackMud().GetCharacter(this.UserID);

			if (smc == null)
			{
				return null;
			}

			// Get a list of awaiting responses for the characters current room
			List<AwaitingResponseFromCharacter> npcResponses = smc.GetAwaitingResponsesForRoom();

			if (npcResponses != null)
			{
				foreach (AwaitingResponseFromCharacter response in npcResponses)
				{
					// Check the resonse has not expored
					if (Utils.GetUnixTime() <= response.TimeOut)
					{
						// Loop around the shortcut tokens looking for a match
						foreach (ShortcutToken token in response.ShortCutTokens)
						{
							if (token.ShortCutToken.ToLower() == commandText.ToLower())
							{
								return $"resp {token.ShortCutToken}";
							}
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Checks if the given commandText matched an exit in the current room.
		/// If there is a match the correctly formatted command is returned, else null is.
		/// </summary>
		/// <param name="commandText">User entered command.</param>
		/// <returns>Formatted command for procesing or null.</returns>
		private string CheckRoomExitCodes(string commandText)
		{
			SMCharacter smc = new SlackMud().GetCharacter(this.UserID);

			if (smc == null)
			{
				return null;
			}

			// Get a list of all exits.
			List<SMExit> exits = smc.GetRoom().RoomExits;

			if (exits == null || !exits.Any())
			{
				return null;
			}

			// Loop through the exits comparing the commandText to the shortcut.
			foreach (SMExit exit in exits)
			{
				if (exit.Shortcut.ToLower() == commandText.ToLower())
				{
					return $"move {exit.Shortcut}";
				}
			}

			return null;
		}
	}
}