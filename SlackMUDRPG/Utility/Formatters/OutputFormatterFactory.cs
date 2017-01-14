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
		/// Returns OutputFormatter instance bases in the output target name (e.g. slack)
		/// </summary>
		/// <param name="targetPlatform">Case insensitive name of the out target platform</param>
		/// <returns>OutputFormatter Instance</returns>
		public static OutputFormatter Get(string targetPlatform = null)
		{
			if (targetPlatform == null)
			{
				targetPlatform = Utils.GetQueryParam("st");
			}

			targetPlatform = targetPlatform == null ? targetPlatform : targetPlatform.ToString().ToLower();

			switch (targetPlatform)
			{
				case "slack":
					return new SlackOutputFormatter();
				default:
					return new DefaultOutputFormatter();
			}
		}
	}
}