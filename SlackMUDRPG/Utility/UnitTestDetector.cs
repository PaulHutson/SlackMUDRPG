using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility
{
	/// <summary>
	/// Detects if we are running inside a unit test.
	/// </summary>
	public static class UnitTestDetector
	{
		/// <summary>
		/// This is in a sub method so that it only needs to be run once on first
		/// run of the IsInUnitTest getter/setter method.
		/// </summary>
		static UnitTestDetector()
		{
			string testAssemblyName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";
			UnitTestDetector.IsInUnitTest = AppDomain.CurrentDomain.GetAssemblies()
				.Any(a => a.FullName.StartsWith(testAssemblyName));
		}

		/// <summary>
		/// Is used to detect whether we're using a unit test.  This is only used
		/// for the location checks within the path finding class.
		/// </summary>
		public static bool IsInUnitTest { get; private set; }
	}
}