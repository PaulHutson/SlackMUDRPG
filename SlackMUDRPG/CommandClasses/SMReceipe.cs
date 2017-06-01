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

		/// <summary>
		/// Gets the name of the item the recipe produces by parsing the Produces property.
		/// </summary>
		/// <returns>The name of the item produced.</returns>
		public string GetProducedItemName()
		{
			string[] parts = this.Produces.Split('|');

			return parts[0];
		}

		/// <summary>
		/// Gets the quantity of items the recipe produces by parsing the Produces property.
		/// </summary>
		/// <returns>The quantity of items produced.</returns>
		public Int32 GetProducedItemQty()
		{
			string[] parts = this.Produces.Split('|');

			if (parts.Length == 1)
			{
				return 1;
			}

			return Int32.Parse(parts[1]);
		}

		public SMItem GetProducedItem()
		{
			SMItem smi = null;

			// Get the right path, and work out if the file exists.
			string path = FilePathSystem.GetFilePath("Objects", this.GetProducedItemName());

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

		/// <summary>
		/// Gets a comma seperated string of the skills (including level) required for this recipe.
		/// </summary>
		/// <returns>String detailing the required skills.</returns>
		public string GetRequiredSkillsString()
		{
			List<string> required = new List<string>();

			foreach (SMSkillHeld skill in this.RequiredSkills)
			{
				required.Add($"{skill.SkillName} ({skill.SkillLevel})");
			}

			return required.Count > 0 ? String.Join(", ", required.ToArray()) : null;
		}

		/// <summary>
		/// Gets a comma seperated string of the materials (including qty) required for this recipe.
		/// </summary>
		/// <returns>String detailing the required materials.</returns>
		public string GetRequiredMaterialsString()
		{
			List<string> required = new List<string>();

			foreach (SMReceipeMaterial material in this.Materials)
			{
				required.Add($"{material.MaterialType.Split('.')[1]} x {material.MaterialQuantity}");
			}

			return required.Count > 0 ? String.Join(", ", required.ToArray()) : null;
		}

		/// <summary>
		/// Gets a string used to display the output of the recipe.
		/// </summary>
		/// <returns>String represeting the recipes output.</returns>
		public string GetProducedOutputString()
		{
			Int32 qty = this.GetProducedItemQty();

			if (qty > 1)
			{
				return $"{this.GetProducedItem().PluralName} x {qty}";
			}

			return $"{this.GetProducedItem().ItemName} x {qty}";
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