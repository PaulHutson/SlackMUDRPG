using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SlackMUDRPG.CommandClasses;
using SlackMUDRPG.Utility;
using System.Reflection;

namespace SlackMUDRPG
{
	public partial class GameAccess : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Default text to output if everything goes Pete-Tong
			string outputText = "Command not found... try '/sdcommand help' to return a list of possible commands";

			// Other variables needed
			bool error = false;
			string botName = "SlackMud";                	// Error bot name
			string replyToChannel = "@error";               // Default ReplyToChannel
			string replyToError = outputText;               // Default reply text

			// Get Slack / Other webhook locations
			string serviceType = Request.Form["st"] ?? Request.QueryString["st"];   // This will only be Slack for now, but could use something else like Discord

			// Get Slack Group Name
			string serviceName = Request.Form["sn"] ?? Request.QueryString["sn"];   // This will only be Slack for now, but could use something else like Discord

			// Additional text (this works for both form submissions and also query strings depending on how we're accessing the code)
			string additionalText = Request.Form["text"] ?? Request.QueryString["text"];   // Additional text that might be needed from the form..

			string responseURL = Request.Form["response_url"] ?? Request.QueryString["response_url"];

            try
            {
                PerformCommand(additionalText);
            }
            catch
            {
                error = true;
            }
			
			if (error && (replyToChannel != "@error"))
			{
				Commands.SendMessage(serviceType, serviceName, outputText, botName, replyToChannel, responseURL);
			}

		}

		private void PerformCommand(string cmd)
		{
			SMParsedCommand parsedCmd = this.ParseCommandString(cmd);
            
            this.CallUserFuncArray(parsedCmd.Command.CommandClass, parsedCmd.Command.CommandMethod, parsedCmd.Parameters.ToArray());

        }

		private SMParsedCommand ParseCommandString(string cmdString)
		{
			SMParsedCommand cmd = new SMParsedCommand();

			cmdString = this.CleanCommandString(cmdString);

			string commandName = this.GetCommandNameFromString(cmdString);

			SMCommand command = this.GetCommandByName(commandName);

			List<object> parameters = this.GetParamsFromCommandString(command, cmdString);

			cmd.CommandName = commandName;
			cmd.Command = command;
			cmd.Parameters = parameters;

			return cmd;
		}

		private string CleanCommandString(string cmdString)
		{
			// trim extra white space
			cmdString = cmdString.Trim();

			//trim leading "/" id present
			char[] toTrim = { '/' };
			cmdString = cmdString.TrimStart(toTrim);

			return cmdString;
		}

		private string GetCommandNameFromString(string cmdString)
		{
			int spacePos = cmdString.IndexOf(" ");

			if (spacePos < 0)
			{
				spacePos = cmdString.Length;
			}

			return cmdString.Substring(0, spacePos);
		}

		private SMCommand GetCommandByName(string name)
		{
			SMCommands commands = (SMCommands)HttpContext.Current.Application["SMCommands"];

			SMCommand command = commands.SMCommandList.FirstOrDefault(cmd => cmd.CommandName == name);

			return command;
		}

		private List<object> GetParamsFromCommandString(SMCommand command, string cmdString)
		{
			List<object> parameters = new List<object>();
            string userId = HttpContext.Current.Request.Form["user_id"] ?? HttpContext.Current.Request.QueryString["user_id"];

            if (!command.CommandNoChar)
            {
                parameters.Add(new SlackMud().GetCharacter(userId));
            } else
            {
                parameters.Add(userId);
            }
            
			string commandExpression = command.CommandExpression;

			string pattern = command.ParseExpression();

			Match match = Regex.Match(cmdString, pattern);

			if (match.Success)
			{
				foreach (Group group in match.Groups)
				{
					if ((group.Value != cmdString) && (group.Value != ""))
                    {
						parameters.Add(group.Value);
					}
				}
			}

			return parameters;
		}

		private void CallUserFuncArray(string className, string methodName, params object[] providedArgs)
		{
			object obj = Type.GetType(className).GetConstructor(Type.EmptyTypes).Invoke(new object[]{});
			MethodInfo method = Type.GetType(className).GetMethod(methodName);
            var parameters = method.GetParameters();
            object[] calulatedArgs = new object[parameters.Length];
            for (int i = 0; i < calulatedArgs.Length; i++)
            {
                if (i < providedArgs.Length)
                {
                    calulatedArgs[i] = providedArgs[i];
                }
                else if (parameters[i].HasDefaultValue)
                {
                    calulatedArgs[i] = parameters[i].DefaultValue;
                }
                else
                {
                    throw new ArgumentException("Not enough arguments provided");
                }
            }
            method.Invoke(method.IsStatic ? null : obj, calulatedArgs);

            //.Invoke(obj, args)

            // for static methods
            //Type.GetType(className).GetMethod(methodName).Invoke(null, args);
        }

        private void SetOutputText(string outputLit)
		{
			lit_output.Text = outputLit;
		}

		private void SendPrivateMessage(string serviceType, string nameOfHook, string toName)
		{
			//Commands.SendMessage(serviceType, nameOfHook, "This is a bit of text", "SlackMud", toName);
		}
	}
}