using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMItemContainer : SMItem
	{
		[JsonProperty("ItemCapacity")]
		public int ItemCapacity { get; set; }

		[JsonProperty("HeldItems")]
		public List<SMItem> HeldItems { get; set; }

		/// <inheritdoc />
		public override bool canHoldOtherItems()
		{
			return true;
		}
	}
}