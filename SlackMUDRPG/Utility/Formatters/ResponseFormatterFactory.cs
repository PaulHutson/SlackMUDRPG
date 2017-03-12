using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.Utility.Formatters
{
	public static class ResponseFormatterFactory
	{
		/// <summary>
		/// Returns aRutputFormatter instance
		/// </summary>
		/// <returns>OutputFormatter Instance</returns>
		public static ResponseFormatter Get()
		{
			return new ResponseFormatterDefault();
		}
	}
}