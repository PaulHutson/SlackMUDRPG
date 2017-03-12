using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.Utility.Formatters
{
	public class OutputFormatterHTML : OutputFormatter
	{
		/// <inheritdoc />
		protected override string GetBoldReplacePattern()
		{
			return "<strong>$2</strong>";
		}

		/// <inheritdoc />
		protected override string GetCodeBlockReplacePattern()
		{
			return "<pre>$2</pre>";
		}

		/// <inheritdoc />
		protected override string GetColorReplacePattern()
		{
			return "<span style=\"color: $2;\">$3</span>";
		}

		/// <inheritdoc />
		protected override string GetItalicReplacePattern()
		{
			return "<span style=\"font-style: italic;\">$2</span>";
		}

		/// <inheritdoc />
		protected override string GetListItemReplacePattern()
		{
			return "<li>$2</li>";
		}

		/// <inheritdoc />
		protected override string GetSizeReplacePattern()
		{
			return "<span data-size=\"$3\">$2</span>";
		}

		/// <inheritdoc />
		protected override string GetStrikeReplacePattern()
		{
			return "<span style=\"text-decoration: line-through;\">$2</span>";
		}

		/// <inheritdoc />
		protected override string GetUnderlineReplacePattern()
		{
			return "<span style=\"text-decoration: underline;\">$2</span>";
		}

		/// <inheritdoc />
		protected override string GetNewLineSequence()
		{
			return "<br/>";
		}
	}
}