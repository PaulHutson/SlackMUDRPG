using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.Utility.Formatters
{
	public static class OutputFormatterFactory
	{
		/// <summary>
		/// Returns a new OutputFormatter instance based on the target platform e.g. Slack.
		/// </summary>
		/// <param name="targetPlatform">Case insensitive name of the target platform.</param>
		/// <returns>OutputFormatter instance.</returns>
		public static OutputFormatter Get(string targetPlatform)
		{
			switch (targetPlatform.ToLower())
			{
				case "slack":
					return new OutputFormatterSlack();
				case "html":
					return new OutputFormatterHTML();
				case "skype":
					return new OutputFormatterSkype();
				default:
					return new OutputFormatterDefault();
			}
		}
	}
}