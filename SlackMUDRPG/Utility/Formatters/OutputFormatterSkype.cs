using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class OutputFormatterSkype : OutputFormatter
	{
		/// <inheritdoc />
		protected override string GetBoldReplacePattern()
		{
			return "**$2**";
		}

		/// <inheritdoc />
		protected override string GetCodeBlockReplacePattern()
		{
			return "`$2`";
		}

		/// <inheritdoc />
		protected override string GetColorReplacePattern()
		{
			return "$3";
		}

		/// <inheritdoc />
		protected override string GetItalicReplacePattern()
		{
			return "*$2*";
		}

		/// <inheritdoc />
		protected override string GetListItemReplacePattern()
		{
			return "    $2";
		}

		/// <inheritdoc />
		protected override string GetSizeReplacePattern()
		{
			return "$3";
		}

		/// <inheritdoc />
		protected override string GetStrikeReplacePattern()
		{
			return "~$2~";
		}

		/// <inheritdoc />
		protected override string GetUnderlineReplacePattern()
		{
			return "$2";
		}

		/// <inheritdoc />
		protected override string GetNewLineSequence()
		{
			return "\n\n";
		}
	}
}