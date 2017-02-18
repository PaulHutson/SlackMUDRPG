using Newtonsoft.Json;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMQuest
	{
		[JsonProperty("QuestName")]
		public string QuestName { get; set; }

		[JsonProperty("Prerequisites")]
		public List<SMQuestPrequisite> Prerequisites { get; set; }

		[JsonProperty("Daily")]
		public bool Daily { get; set; }

		[JsonProperty("QuestSteps")]
		public List<SMQuestStep> QuestSteps { get; set; }

		[JsonProperty("Rewards")]
		public List<SMQuestReward> Rewards { get; set; }
	}

	public class SMQuestPrequisite
	{
		[JsonProperty("Prerequisites")]
		public string Prerequisites { get; set; } // Quest, Faction, Etc

		[JsonProperty("Datapoint")]
		public string Datapoint { get; set; } // Questname, Attribute Name, etc

		[JsonProperty("Value")]
		public string Value { get; set; } // The amount (1 for a quest completed, x for the attribute value)
	}

	public class SMQuestStep
	{
		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("Type")]
		public string Type { get; set; }

		[JsonProperty("CharacterName")]
		public string CharacterName { get; set; }

		[JsonProperty("LocationID")]
		public string LocationID { get; set; }

		[JsonProperty("AdditionalData")]
		public string AdditionalData { get; set; }

		[JsonProperty("Instructions")]
		public string Instructions { get; set; }
	}

	public class SMQuestReward
	{
		[JsonProperty("Type")]
		public string Type { get; set; }

		[JsonProperty("AdditionalData")]
		public string AdditionalData { get; set; }
	}

	public class SMQuestStatus
	{
		[JsonProperty("QuestName")]
		public string QuestName { get; set; }

		[JsonProperty("QuestStep")]
		public string QuestStep { get; set; }

		[JsonProperty("LastDateUpdated")]
		public int LastDateUpdated { get; set; }

		[JsonProperty("Expires")]
		public int Expires { get; set; }

		[JsonProperty("Completed")]
		public bool Completed { get; set; }
	}

	public static class SMQuestFactory {

		/// <summary>
		/// Gets a quest of the specific type
		/// </summary>
		/// <param name="questName">The name of the quest</param>
		/// <returns>A new item of null.</returns>
		public static SMQuest Get(string questName)
		{
			string questSpec = GetQuestSpecJson(questName);
			SMQuest smq = null;

			if (questSpec != "")
			{
				smq = JsonConvert.DeserializeObject<SMQuest>(questSpec);
			}

			return smq;
		}

		/// <summary>
		/// Gets a string representing the json spec for a quest.
		/// </summary>
		/// <param name="questName">The quest name.</param>
		/// <returns>Json spec for the quest or an empty string.</returns>
		private static string GetQuestSpecJson(string questName)
		{
			string filename = questName;
			string filePath = FilePathSystem.GetFilePath("Quests", filename);

			string objectSpecJson = "";

			if (File.Exists(filePath))
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					objectSpecJson = reader.ReadToEnd();
				}
			}

			return objectSpecJson;
		}
	}

}