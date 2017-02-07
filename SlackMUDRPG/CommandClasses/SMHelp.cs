using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;

namespace SlackMUDRPG.CommandClasses
{
	/// <summary>
	/// SMHelp class, takes care of formatting and displaying help commands to the user
	/// </summary>
	public class SMHelp
	{
		/// <summary>
		/// The list of commands available to users
		/// </summary>
		private List<SMCommand> commandList;

		/// <summary>
		/// The character requesting help (needed to sent responces)
		/// </summary>
		private SMCharacter character;

		/// <summary>
		/// OutputFormatter instance to use when displaying help to the user
		/// </summary>
		private OutputFormatter outputFormatter;

		/// <summary>
		/// SMHelp class constructor, populate commandsList
		/// </summary>
		public SMHelp()
		{
			this.commandList = this.GetCommandsList();

			this.character = new SlackMud().GetCharacter(Utils.GetQueryParam("user_id"));

			this.outputFormatter = OutputFormatterFactory.Get();
		}

		/// <summary>
		/// Get a list of commands (SMCommands) available to users
		/// </summary>
		/// <returns></returns>
		private List<SMCommand> GetCommandsList()
		{
			return (List<SMCommand>)HttpContext.Current.Application["SMCommands"];
		}

		/// <summary>
		/// Checks if a given string is the name of a family of commands (case insensitive).
		/// </summary>
		/// <param name="family">String represeting the command family name to check.</param>
		/// <returns>True if string is a command family name, otherwise false.</returns>
		private bool IsCommandFamily(string family)
		{
			if (this.commandList.FirstOrDefault(cmd => cmd.CommandFamily.ToLower() == family.ToLower()) != null)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if a given string is the name of a command (case insensitive).
		/// </summary>
		/// <param name="command">String represeting the command name to check.</param>
		/// <returns>True if string is a command name, otherwise false.</returns>
		private bool IsCommand(string command)
		{
			if (this.commandList.FirstOrDefault(cmd =>
				cmd.CommandName.Split(',').Select(s => s.Trim().ToLower()).Contains(command.ToLower())) != null)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Shows a list of command families to the user.
		/// </summary>
		private void ShowGeneralHelp()
		{
			string output = "";

			output += this.outputFormatter.General("The following types of command are available, type \"", 0);
			output += this.outputFormatter.Italic("help <command_type>", 0);
			output += this.outputFormatter.General("\" for more details:");

			var commandFamilies = from cmd in this.commandList
								  group cmd by cmd.CommandFamily into family
								  orderby family.Key ascending
								  select new
								  {
									  FamilyName = family.Key
								  };

			foreach (var family in commandFamilies)
			{
				output += this.outputFormatter.ListItem(family.FamilyName);
			}

			this.OutputHelp(output);
		}

		/// <summary>
		/// Shows a list of commands in a given CommandFamily to the user.
		/// </summary>
		/// <param name="family"></param>
		private void ShowCommandFamilyHelp(string family)
		{
			string output = "";

			output += this.outputFormatter.General($"The following commands are available \"", 0);
			output += this.outputFormatter.Italic(Utils.ToTitleCase(family), 0);
			output += this.outputFormatter.General("\", type \"", 0);
			output += this.outputFormatter.Italic("help <command>", 0);
			output += this.outputFormatter.General("\" for details of how to use a command:");

			List<SMCommand> commands = this.GetCommandsForFamily(family);

			foreach (SMCommand cmd in commands)
			{
				List<string> aliases = this.GetCmdAliasList(cmd);

				foreach (string alias in aliases)
				{
					Dictionary<string, string> data = new Dictionary<string, string>();
					data.Add("name", alias);

					output += this.outputFormatter.ListItem(new TagReplacer(cmd.CommandSyntax).Replace(data));
				}
			}

			this.OutputHelp(output);
		}

		/// <summary>
		/// Gets a list of aliases for a given SMCommand.
		/// </summary>
		/// <param name="cmd">The SMCommand.</param>
		/// <returns>The list of alias names.</returns>
		private List<string> GetCmdAliasList(SMCommand cmd)
		{
			return cmd.CommandName.Split(',').Select(name => name.Trim()).ToList();
		}

		/// <summary>
		/// Shows help for a given command.
		/// </summary>
		/// <param name="command"></param>
		private void ShowCommandHelp(string command)
		{
			string output = "";

			SMCommand cmd = this.GetCommand(command);

			if (cmd == null)
			{
				this.CommandNotFound();
				return;
			}

			output += this.GetHelpStringForCommand(cmd, command);

			this.OutputHelp(output);
		}

		/// <summary>
		/// Shows a message to the user if no help is found
		/// </summary>
		private void CommandNotFound()
		{
			string output = this.outputFormatter.Italic("Sorry, unable to find help in that!");

			this.OutputHelp(output);
		}

		/// <summary>
		/// Outputs a given help string to the user
		/// </summary>
		/// <param name="helpString"></param>
		private void OutputHelp(string helpString)
		{
			this.character.sendMessageToPlayer(helpString);
		}

		/// <summary>
		/// Gets a filtered command list containing only commands in a given CommandFamily, order by CommandName
		/// </summary>
		/// <param name="family">The CommandFamily name to get commands for.</param>
		/// <returns>List of commands.</returns>
		private List<SMCommand> GetCommandsForFamily(string family)
		{
			return this.commandList.Where(cmd => cmd.CommandFamily.ToLower() == family.ToLower())
								   .OrderBy(cmd => cmd.CommandName)
								   .ToList();
		}

		/// <summary>
		/// Gets a command from the commandsList by its name.
		/// </summary>
		/// <param name="commandName">CommandName to search for.</param>
		/// <returns>SMCommand or null.</returns>
		private SMCommand GetCommand(string commandName)
		{
			return this.commandList.FirstOrDefault(cmd =>
				cmd.CommandName.Split(',').Select(s => s.Trim().ToLower()).Contains(commandName.ToLower())
			);
		}

		/// <summary>
		/// Builds the help string for a given command.
		/// </summary>
		/// <param name="command">SMCOmmand to get help string for.</param>
		/// <returns>The help string to display to the user.</returns>
		private string GetHelpStringForCommand(SMCommand command, string commandName)
		{
			string helpString = "";

			List<string> aliases = this.GetCmdAliasList(command);

			foreach (string alias in aliases)
			{
				if (alias.ToLower() == commandName.ToLower())
				{
					Dictionary<string, string> data = new Dictionary<string, string>();
					data.Add("name", alias);

					helpString += this.outputFormatter.Bold(alias, 0);
					helpString += this.outputFormatter.General($": {new TagReplacer(command.CommandDescription).Replace(data)}");

					helpString += this.outputFormatter.General("");

					helpString += this.outputFormatter.ListItem(
						this.outputFormatter.General("Command Syntax: ", 0) +
						this.outputFormatter.Italic(new TagReplacer(command.CommandSyntax).Replace(data), 0)
					);

					helpString += this.outputFormatter.ListItem(
						this.outputFormatter.General("Example: ", 0) +
						this.outputFormatter.Italic(new TagReplacer(command.ExampleUsage).Replace(data), 0)
					);

					if (command.RequiredSkill != null)
					{
						helpString += this.outputFormatter.ListItem(
							this.outputFormatter.General("Required Skill: ", 0) +
							this.outputFormatter.Italic(command.RequiredSkill.Replace("Skill.", ""), 0)
						);
					}
				}
			}

			return helpString;
		}

		/// <summary>
		/// Main help function called in response to user request to help.
		/// </summary>
		/// <param name="input">Options string indicating what help to give.</param>
		public void Help(string input = null)
		{
			if (input == null)
			{
				this.ShowGeneralHelp();
				return;
			}

			string cleanInput = Utils.SanitiseString(input);

			if (this.IsCommandFamily(cleanInput))
			{
				this.ShowCommandFamilyHelp(cleanInput);
				return;
			}

			if (this.IsCommand(cleanInput))
			{
				this.ShowCommandHelp(cleanInput);
				return;
			}

			this.CommandNotFound();
			return;
		}
	}
}