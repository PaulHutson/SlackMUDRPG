using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using SlackMUDRPG.CommandClasses;

namespace SlackMUDRPG.Utility
{
	public static class Utils
	{
		#region "HTTP"

		/// <summary>
		/// Gets a parameter from the Request Form or QueryString
		/// </summary>
		/// <returns>The value of the parameter.</returns>
		/// <param name="paramName">Parameter name.</param>
		public static string GetQueryParam(string paramName)
		{
			string formParam = null;
			if (HttpContext.Current.Request.Form[paramName] != null)
			{
				formParam = HttpContext.Current.Request.Form[paramName];
			}

			string queryParam = null;
			if (HttpContext.Current.Request.QueryString[paramName] != null)
			{
				queryParam = HttpContext.Current.Request.QueryString[paramName];
			}


			return formParam ?? queryParam ?? "";
		}

		#endregion

		#region "String Processing"

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
			string nonAlphaNumeric = @"[^\w\.@ -]";
			return Regex.Replace(input, nonAlphaNumeric, "");
		}

		/// <summary>
		/// Converts a string to TitleCase
		/// </summary>
		/// <param name="input">The string to convert.</param>
		/// <returns>The title case string.</returns>
		public static string ToTitleCase(string input)
		{
			return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
		}

		/// <summary>
		/// Splits a given camel case string into sepeate words with sapces
		/// </summary>
		/// <param name="value">The string value to split</param>
		/// <returns>The split string</returns>
		public static string SplitCamelCase(string value)
		{
			return Regex.Replace(value, "(?!^)([A-Z])", " $1");
		}

		#endregion

		#region "Reflection"

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

			try
			{
				// Builds params array accounting for defaults
				object[] parameters = GetParamsArrayForMethod(method, args);

				// Calls the method
				return method.Invoke(method.IsStatic ? null : obj, parameters);
			}
			catch (Exception ex)
			{
				return new SMCommandException(ex.Message);
			}
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

			try
			{
				// Builds params array accounting for defaults
				object[] parameters = GetParamsArrayForMethod(method, args).ToArray();

				// Calls the method
				return method.Invoke(method.IsStatic ? null : obj, parameters);
			}
			catch (Exception ex)
			{
				return new SMCommandException(ex.Message);
			}
		}

		/// <summary>
		/// Returns an new instanace of a give className by calling its constructor with the supplied args.
		/// </summary>
		/// <param name="className">Name of the class (fully qualified) to instantiate.</param>
		/// <param name="args">Object array of args required for instantiation.</param>
		/// <returns></returns>
		public static object ConstructWithArgs(string className, params object[] args)
		{
			// Build at array of constructor arg types for GetContructorCall.
			List<Type> types = new List<Type>();

			foreach (var type in args)
			{
				Type t = type.GetType();
				types.Add(t);
			}

			Type[] typesArray = types.ToArray();

			// Instantiate and return new object
			return Type.GetType(className).GetConstructor(typesArray).Invoke(args);
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
					// throw new ArgumentException("Not enough arguments provided");
					return null;
				}
			}

			return calulatedArgs;
		}

		#endregion

		#region "Time Functions"

		public static int GetUnixTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static int GetUnixTimeOffset(int additionalSeconds)
        {
            return GetUnixTime() + additionalSeconds;
        }

		public static int GetDifferenceBetweenUnixTimestamps(int initialTimeStamp, int secondaryTimeStamp)
		{
			return secondaryTimeStamp - initialTimeStamp;
		}

		public static int GetDifferenceBetweenUnixTimestampsInMinutes(int initialTimeStamp, int secondaryTimeStamp)
		{
			return (GetDifferenceBetweenUnixTimestamps(initialTimeStamp, secondaryTimeStamp) / 60);
		}

		#endregion

		#region "Load File"

		public static string GetFileJSON(string filePath, string fileName)
		{
			string specsPath = FilePathSystem.GetFilePath(filePath, fileName);
			string json = null;

			if (File.Exists(specsPath))
			{
				using (StreamReader r = new StreamReader(specsPath))
				{
					json = r.ReadToEnd();
				}
			}

			return json;
		}

		#endregion

		#region "Misc Utility Methods"

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		#endregion

		#region "Templates"

		/// <summary>
		/// Gets an HTML template file as a string, based on the template name provided
		/// </summary>
		/// <param name="templateName">Name of the temaplte to loead</param>
		/// <returns>String representing the HTML template file contents</returns>
		public static string GetHtmlTemplate(string templateName)
		{
			string template = String.Empty;

			string templatesPath = FilePathSystem.GetRootFilePath("HTMLTemplates", templateName, ".html");

			if (File.Exists(templatesPath))
			{
				using (StreamReader r = new StreamReader(templatesPath))
				{
					template = r.ReadToEnd();
				}
			}

			return template;
		}

		/// <summary>
		/// Returns the string None if value is null otherwise the value
		/// </summary>
		/// <param name="value">Some string value to check</param>
		/// <returns>None or value</returns>
		public static string noneIfNull(string value)
		{
			if (value == null)
			{
				return "None";
			}

			return value;
		}

        #endregion

        #region "Error Logging"

        /// <summary>
        /// Writes an error to the error log
        /// </summary>
        /// <param name="errorToLog">The error to write</param>
        public static void LogError(string errorToLog)
        {
            string path = FilePathSystem.GetFilePath("Logs", "Error");
            using (StreamWriter w = new StreamWriter(path, true))
            {
                w.WriteLine(errorToLog);
            }
        }

        #endregion
    }

    public static class ThreadSafeRandom
	{
		[ThreadStatic]
		private static Random Local;

		public static Random ThisThreadsRandom
		{
			get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
		}
	}
}