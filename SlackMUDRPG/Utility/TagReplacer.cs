using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using SlackMUDRPG.Utility;

namespace SlackMUDRPG.Utility
{
	public class TagReplacer
	{
		/// <summary>
		/// Stores the base string that contains tags to be replaced.
		/// </summary>
		private string source;

		/// <summary>
		/// Regex to match variable replacement tag patters such as "{name}" or "{name|lower}"
		/// </summary>
		private string variableReplacementPattern = @"(?:{([^\|]+)\|?(.+?)?})";

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="source"></param>
		public TagReplacer(string source)
		{
			this.source = source;
		}

		/// <summary>
		/// Performs all replacements in the source string returning a new string.
		/// </summary>
		/// <param name="data">Dictionary of datam, key value pairs to use in variable replacements.</param>
		/// <returns></returns>
		public string Replace(Dictionary<string, string> data)
		{
			string output = String.Empty;

			// Perform variable replacements
			output = Regex.Replace(this.source, this.variableReplacementPattern, match => this.DoReplace(match, data));

			return output;
		}

		/// <summary>
		/// MatchEvaluator delagate method for variable replacements ({group1|group2}). Repalces group1 with the corresponding value
		/// from the data dictionary. If the option group2 is matched this is used to transform the group1 value before returing it. 
		/// e.g. for the match variable tag {name|upper}, with the dictionary value of "name" = "example", "EXAMPLE" would be returned.
		/// If there is no key in the dictionary matching group1 and empty string will be reurned.
		/// </summary>
		/// <param name="match">Regex match object.</param>
		/// <param name="data">Data dictionary of key value pairs for replacements.</param>
		/// <returns></returns>
		private string DoReplace(Match match, Dictionary<string, string> data)
		{
			string key = match.Groups[1].Value;

			if (data.ContainsKey(key))
			{
				string val = data[key];

				if (match.Groups[2].Success)
				{
					val = this.GetTransformedValue(val, match.Groups[2].Value);
				}

				return val;
			}

			return String.Empty;
		}

		/// <summary>
		/// Processes a given vaule based on the supplied operator name to get the transformed value.
		/// </summary>
		/// <param name="value">The value to transform.</param>
		/// <param name="operatorName">Operator name defining the transformation.</param>
		/// <returns></returns>
		private string GetTransformedValue(string value, string operatorName)
		{
			switch (operatorName.ToLower())
			{
				case "lower":
					return value.ToLower();
				case "upper":
					return value.ToUpper();
				case "title":
					return Utils.ToTitleCase(value);
				default:
					return value;
			}
		}
	}
}