﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility
{
	/// <summary>
	/// Class to be used to get the file path - changes based on whether we're running
	/// on the webserver or within a test.
	/// </summary>
	public static class FilePathSystem
	{
		/// <summary>
		/// Get the file path to use
		/// </summary>
		/// <param name="folder">Folder Name</param>
		/// <param name="fileName">File Name</param>
		/// <returns>A string (path) to the file system</returns>
		public static string GetFilePath(string folder, string fileName, string fileType = ".json")
		{
			// Set the base file path.
			string pathToUse = @"~\JSON\" + folder + @"\" + fileName + fileType;

			// Check if we're running inside a unit test or not.
			if (!UnitTestDetector.IsInUnitTest)
			{
				// Get the server folder structure.
				pathToUse = HttpContext.Current.Server.MapPath("~/JSON/" + folder + "/" + fileName + fileType);
			}

			// Return the file path string.
			return pathToUse;
		}

		/// <summary>
		/// Get the file path to use
		/// </summary>
		/// <param name="folder">Folder Name</param>
		/// <returns>A string (path) to the file system</returns>
		public static string GetFilePathFromFolder(string folder)
		{
			// Set the base file path.
			string pathToUse = @"~\JSON\" + folder;

			// Check if we're running inside a unit test or not.
			if (!UnitTestDetector.IsInUnitTest)
			{
				// Get the server folder structure.
				pathToUse = HttpContext.Current.Server.MapPath("~/JSON/" + folder);
			}

			// Return the file path string.
			return pathToUse;
		}

		/// <summary>
		/// Get the file path to use
		/// </summary>
		/// <param name="folder">Folder Name</param>
		/// <param name="fileName">File Name</param>
		/// <returns>A string (path) to the file system</returns>
		public static string GetRootFilePath(string folder, string fileName, string fileType = ".json")
		{
			// Set the base file path.
			string pathToUse = @"~\" + folder + @"\" + fileName + fileType;

			// Check if we're running inside a unit test or not.
			if (!UnitTestDetector.IsInUnitTest)
			{
				// Get the server folder structure.
				pathToUse = HttpContext.Current.Server.MapPath("~/" + folder + "/" + fileName + fileType);
			}

			// Return the file path string.
			return pathToUse;
		}
	}
}