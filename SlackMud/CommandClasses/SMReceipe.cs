using Newtonsoft.Json;
using SlackMUDRPG.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMReceipe
	{
		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("Produces")]
		public string Produces { get; set; }

		[JsonProperty("PauseTime")]
		public int PauseTime { get; set; }

		[JsonProperty("NeedToLearn")]
		public bool NeedToLearn { get; set; }

		[JsonProperty("ProductionTrait")]
		public string ProductionTrait { get; set; }

		[JsonProperty("Materials")]
		public List<SMReceipeMaterial> Materials { get; set; }

		[JsonProperty("RequiredSkills")]
		public List<SMSkillHeld> RequiredSkills { get; set; }

		[JsonProperty("StepThresholds")]
		public List<SMReceipeStepThreshold> StepThresholds { get; set; }

		[JsonProperty("SkillSteps")]
		public List<SMSkillStep> SkillSteps { get; set; }

		public SMItem GetProducedItem()
		{
			SMItem smi = null;

			// Get the right path, and work out if the file exists.
			string path = FilePathSystem.GetFilePath("Objects", this.Produces);

			// Check if the character exists..
			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					// Get the character from the json string
					string json = r.ReadToEnd();
					smi = JsonConvert.DeserializeObject<SMItem>(json);
				}
			}

			return smi;
		}
	}

	public class SMReceipeMaterial
	{
		[JsonProperty("MaterialType")]
		public string MaterialType { get; set; }

		[JsonProperty("MaterialQuantity")]
		public int MaterialQuantity { get; set; }
	}

	public class SMReceipeStepThreshold
	{
		[JsonProperty("ThresholdName")]
		public string ThresholdName { get; set; }

		[JsonProperty("ThresholdLevel")]
		public int ThresholdLevel { get; set; }

		[JsonProperty("ThresholdBonus")]
		public List<SMReceipeStepThresholdBonus> ThresholdBonus { get; set; }
	}

	public class SMReceipeStepThresholdBonus
	{
		[JsonProperty("ThresholdBonusName")]
		public string ThresholdBonusName { get; set; }

		[JsonProperty("ThresholdBonusValue")]
		public int ThresholdBonusValue { get; set; }
	}
}