using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SlackMUDRPG.Utility
{
	public static class Utils
	{
		/// <summary>
		/// Gets a parameter from the Request Form or QueryString
		/// </summary>
		/// <returns>The value of the parameter.</returns>
		/// <param name="paramName">Parameter name.</param>
		public static string GetQueryParam(string paramName)
		{
			return HttpContext.Current.Request.Form[paramName] ?? HttpContext.Current.Request.QueryString[paramName];
		}

		/// <summary>
		/// Cleans a string, trimming whitespace and optionally removing specifed leading chars
		/// </summary>
		/// <returns>The cleaned string.</returns>
		/// <param name="str">The String to clean.</param>
		/// <param name="trimFromStart">Optional chars to Trim from the start.</param>
		public static string CleanString(string str, char[] trimFromStart = null)
		{
			// trim extra white space
			str = str.Trim();

			//trim leading characters
			if (trimFromStart != null)
			{
				str = str.TrimStart(trimFromStart);
			}

			return str;
		}

		/// <summary>
		/// Sanitises a string by removing any characters that are not alpha numberic, spaces, -, _ @ or .
		/// </summary>
		/// <param name="input">The string to sanitise.</param>
		/// <returns>The sanitised string.</returns>
		public static string SanitiseString(string input)
		{
			string nonAlphaNumeric = @"[^\w\.@- ]";
			return Regex.Replace(input, nonAlphaNumeric, string.Empty);
		}

		/// <summary>
		/// Calls methodName on className with the args supplied.
		/// </summary>
		/// <param name="className">Class name.</param>
		/// <param name="methodName">Method name.</param>
		/// <param name="args">Arguments.</param>
		public static object CallUserFuncArray(string className, string methodName, params object[] args)
		{
			// Gets class object instance
			object obj = Type.GetType(className).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });

			// Gets method details
			MethodInfo method = Type.GetType(className).GetMethod(methodName);

			// Builds params array accounting for defaults
			object[] parameters = GetParamsArrayForMethod(method, args);

			// Calls the method
			return method.Invoke(method.IsStatic ? null : obj, parameters);
		}

		/// <summary>
		/// Calls methodName on the obj with the args supplied.
		/// </summary>
		/// <param name="obj">Object instance to call methodName on.</param>
		/// <param name="methodName">Method name.</param>
		/// <param name="args">Arguments.</param>
		public static object CallUserFuncArray(object obj, string methodName, params object[] args)
		{
			// Gets the class name of the object
			string className = obj.GetType().ToString();

			// Gets method details
			MethodInfo method = Type.GetType(className).GetMethod(methodName);

			// Builds params array accounting for defaults
			object[] parameters = GetParamsArrayForMethod(method, args).ToArray();

			// Calls the method
			return method.Invoke(method.IsStatic ? null : obj, parameters);
		}

		/// <summary>
		/// Builds an array of parametes for a method based on the supplied args the methods defaults
		/// </summary>
		/// <returns>The parameters array for method.</returns>
		/// <param name="method">Method.</param>
		/// <param name="args">Arguments.</param>
		private static object[] GetParamsArrayForMethod(MethodInfo method, object[] args)
		{
			// Gets details of the methods parameters
			var parameters = method.GetParameters();

			// creates a new object array to match the number of parameters
			object[] calulatedArgs = new object[parameters.Length];

			// Assigns a value to each index of the array
			for (int i = 0; i < calulatedArgs.Length; i++)
			{
				if (i < args.Length)
				{
					calulatedArgs[i] = args[i];
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

			return calulatedArgs;
		}
	}
}