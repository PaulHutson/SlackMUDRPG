using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMQuest
	{
		[JsonProperty("QuestName")]
		public string QuestName { get; set; }

		[JsonProperty("Prerequisites")]
		public string Prerequisites { get; set; }

		[JsonProperty("QuestName")]
		public List<SMQuestStep> QuestSteps { get; set; }
	}

	public class SMQuestPrequisite
	{
		[JsonProperty("Prerequisites")]
		public string Type { get; set; } // Quest, Faction, Etc

		[JsonProperty("Datapoint")]
		public string Datapoint { get; set; } // Questname, Attribute Name, etc

		[JsonProperty("Value")]
		public string Value { get; set; } // The amount (1 for a quest completed, x for the attribute value)
	}

	public class SMQuestStep
	{

	}

	public class SMQuestStatus
	{
		[JsonProperty("QuestName")]
		public string QuestName { get; set; }

		[JsonProperty("QuestStep")]
		public string QuestStep { get; set; }

		[JsonProperty("LastDateUpdated")]
		public int LastDateUpdated { get; set; }

		[JsonProperty("Completed")]
		public bool Completed { get; set; }
	}
}