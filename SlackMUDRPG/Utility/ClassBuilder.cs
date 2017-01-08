using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using SlackMUDRPG.CommandClasses;

namespace SlackMUDRPG.Utility
{
	/// <summary>
	/// Class builder spec, holds details of how the get a CommandClass instance to use when running
	/// user inputted commands.
	/// </summary>
	public class ClassBuilderSpec
	{
		/// <summary>
		/// Gets or sets the name of the command class, that should be returned.
		/// </summary>
		/// <value>The name of the command class.</value>
		[JsonProperty("CommandClassName")]
		public string CommandClassName { get; set; }

		/// <summary>
		/// Gets or sets the name of the build class, that has a method returning an instance of the CommandClass.
		/// </summary>
		/// <value>The name of the build class.</value>
		[JsonProperty("BuildClassName")]
		public string BuildClassName { get; set; }

		/// <summary>
		/// Gets or sets the name of the build method, that returns an instance of the CommandClass.
		/// </summary>
		/// <value>The name of the build method.</value>
		[JsonProperty("BuildMethodName")]
		public string BuildMethodName { get; set; }

		/// <summary>
		/// Gets or sets the build arguments list, that defines the arguments required by the BuildMethod and
		/// how to derive them.
		/// </summary>
		/// <value>The build arguments.</value>
		[JsonProperty("BuildArgs")]
		public List<BuildArg> BuildArgs { get; set; }
	}

	/// <summary>
	/// Build argument object, containng details of a single argument and its source for
	/// building an CommandClass instance.
	/// </summary>
	public class BuildArg
	{
		/// <summary>
		/// Gets or sets the name of the argument.
		/// </summary>
		/// <value>The name of the argument.</value>
		[JsonProperty("ArgName")]
		public string ArgName { get; set; }

		/// <summary>
		/// Gets or sets the arguments source.
		/// </summary>
		/// <value>The argument source.</value>
		[JsonProperty("ArgSource")]
		public string ArgSource { get; set; }
	}

	/// <summary>
	/// Class builder specs, hold a list of CLassBuilderSpec objects
	/// </summary>
	public class ClassBuilderSpecs
	{
		/// <summary>
		/// Gets or sets the class builder spec list.
		/// </summary>
		/// <value>The class builder spec list.</value>
		[JsonProperty("ClassBuilderSpecList")]
		public List<ClassBuilderSpec> ClassBuilderSpecList { get; set; }
	}

	/// <summary>
	/// Class builder.
	/// </summary>
	public class ClassBuilder
	{
		/// <summary>
		/// The ClassBuilderSpec object for the instance.
		/// </summary>
		private ClassBuilderSpec Spec;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:SlackMUDRPG.Utility.ClassBuilder"/> class.
		/// </summary>
		/// <param name="CommandClassName">The name of the CommandClass the being built.</param>
		public ClassBuilder(string CommandClassName)
		{
			this.Spec = this.GetSpecFromCommandClassName(CommandClassName);
		}

		/// <summary>
		/// Gets an instance of the CommandClass detailed in this.Spec.
		/// </summary>
		/// <returns>The CommandClass instance.</returns>
		public object GetClassInstance()
		{
			// return null if the ClassBuilderSpec does not exist
			if (this.Spec == null)
			{
				return null;
			}

			SMCharacter smc = new SlackMud().GetCharacter(Utils.GetQueryParam("user_id"));
			smc.sendMessageToPlayer("here");

			// Get the args array for the BuildMethod
			object[] parameters = this.GetArgs();

			smc.sendMessageToPlayer("here1");

			// Call the BuildMethod and return the new class instance
			object CommandClass = Utils.CallUserFuncArray(
				this.Spec.BuildClassName,
				this.Spec.BuildMethodName,
				parameters
			);

			smc.sendMessageToPlayer("here2");

			return CommandClass;
		}

		/// <summary>
		/// Gets the ClassBuilderSpec for the named CommandClass from application memory.
		/// </summary>
		/// <returns>ClassBuilderSpec object or null</returns>
		/// <param name="CommandClassName">Command class name.</param>
		private ClassBuilderSpec GetSpecFromCommandClassName(string CommandClassName)
		{
			ClassBuilderSpecs specs = (ClassBuilderSpecs)HttpContext.Current.Application["ClassBuilderSpecs"];

			ClassBuilderSpec spec = specs.ClassBuilderSpecList.FirstOrDefault(s => s.CommandClassName == CommandClassName);

			return spec;
		}

		/// <summary>
		/// Gets the arguments array for the BuildMethod based on this.Spec.
		/// </summary>
		/// <returns>The arguments.</returns>
		private object[] GetArgs()
		{
			List<object> args = new List<object>();

			foreach (BuildArg buildArg in this.Spec.BuildArgs)
			{
				args.Add(this.GetBuildArgValue(buildArg));
			}

			return args.ToArray();
		}

		/// <summary>
		/// Gets the build argument value, based on it ArgSource.
		/// </summary>
		/// <returns>The build argument value.</returns>
		/// <param name="arg">BuildArg.</param>
		private object GetBuildArgValue(BuildArg arg)
		{
			if (arg.ArgSource == "query_param")
			{
				return Utils.GetQueryParam(arg.ArgName);
			}

			return null;
		}
	}
}
