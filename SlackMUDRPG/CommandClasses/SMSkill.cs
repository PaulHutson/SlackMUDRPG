using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMSkill
	{
		[JsonProperty("SkillType")]
		public SMSkillType SkillType { get; set; }

		[JsonProperty("SkillName")]
		public string SkillName { get; set; }

		[JsonProperty("SkillDescription")]
		public string SkillDescription { get; set; }

		[JsonProperty("SkillLearnText")]
		public string SkillLearnText { get; set; }

		[JsonProperty("LevelIncreaseText")]
		public string LevelIncreaseText { get; set; }

		[JsonProperty("ActivityType")]
		public string ActivityType { get; set; }

		[JsonProperty("BaseStat")]
		public string BaseStat { get; set; }

		[JsonProperty("LevelMultiplier")]
		public int LevelMultiplier { get; set; }

		[JsonProperty("ImprovementSpeed")]
		public int ImprovementSpeed { get; set; }

		[JsonProperty("Prerequisites")]
		public List<SMSkillPrerequisite> Prerequisites { get; set; }

		[JsonProperty("SkillSteps")]
		public List<SMSkillStep> SkillSteps { get; set; }

		public void UseSkill(SMCharacter smc, out string messageOut, out float floatOut, bool beginSkillUse = true, string targetType = null, string targetID = null, bool isPassive = false)
		{
			// Output variables for passive skills that need output (like "dodge")
			messageOut = "";
			floatOut = 0;

			// Set the character activity
			if (beginSkillUse)
			{
				smc.CurrentActivity = this.ActivityType;
			}

			// Loop around the steps
			foreach (SMSkillStep smss in this.SkillSteps)
			{
				if (smc.CurrentActivity == this.ActivityType)
				{
					switch (smss.StepType)
					{
						case "Object":
							if ((!StepRequiredObject(smc, smss.StepRequiredObject, smss.RequiredObjectAmount)) && (!isPassive))
								smc.sendMessageToPlayer(smss.FailureOutput);
							break;
                        case "EquippedObject":
                            if ((!StepRequiredObject(smc, smss.StepRequiredObject, smss.RequiredObjectAmount, true)) && (!isPassive))
                                smc.sendMessageToPlayer(smss.FailureOutput);
                            break;
                        case "Target":
							if ((!StepRequiredTarget(smc, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID)) && (!isPassive))
								smc.sendMessageToPlayer(smss.FailureOutput);
							break;
						case "Hit":
							if (!StepHit(smss, smc, this.BaseStat, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
							{
                                if (!isPassive)
                                {
                                    smc.sendMessageToPlayer(smss.FailureOutput);
                                }
                                SkillIncrease(smc, false);
							}
							else
							{
								SkillIncrease(smc, true);
							}
							break;
						case "Information":
							if (targetType == "Character")
							{
								// Get the character
								var targetChar = smc.GetRoom().GetPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);
								smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, targetChar.GetFullName(), ""));
							}
							else
							{
								var targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
								smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, targetItem.SingularPronoun + " " + targetItem.ItemName, ""));
							}
							break;
						case "Pause":
							System.Threading.Thread.Sleep(smss.RequiredObjectAmount * 1000);
							break;
						case "Repeat":
							this.UseSkill(smc, out messageOut, out floatOut, false, targetType, targetID, isPassive);
							break;
					}
				}
			}

		}

		#region "Skill Step Methods"

		private bool StepRequiredObject(SMCharacter smc, string requiredObjectType, int requiredObjectAmount, bool equippedItem = false)
		{
            bool hasItem = false;
            string[] splitRequiredObjectType = requiredObjectType.Split('.');

            if (equippedItem)
            {
                // Check if the player has the type of object equipped.
                if (splitRequiredObjectType[0] == "Family")
                {
                    hasItem = smc.HasItemFamilyTypeEquipped(splitRequiredObjectType[1]);
                }
                else
                {
                    hasItem = smc.HasItemTypeEquipped(splitRequiredObjectType[1]);
                }
            }

            bool hasEnoughOfItem = true;
            if (hasItem)
            {
                // Check if, in the list, they have the required item / amount
                hasEnoughOfItem = (smc.CountOwnedItemsByName(splitRequiredObjectType[1]) >= requiredObjectAmount);
            }
            
            return hasItem && hasEnoughOfItem;
		}

		private bool StepRequiredTarget(SMCharacter smc, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
		{
			if (targetType != "Character")
			{
				// Check the type of target they've specified is correct...
				// Get the target item
				SMItem targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

				// Look through the items in the room to see if there are any items with the target id
				if (targetItem != null)
				{
					if (requiredTargetObjectType != null)
					{
						if (targetItem.ItemFamily != requiredTargetObjectType)
						{
							return false;
						}
					}
					else
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		private bool StepHit(SMSkillStep smss, SMCharacter smc, string baseStat, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
		{
			// Get the object to hit the target with.
			string itemName = smss.StepRequiredObject;
			float charItembaseDamage = smc.Attributes.Strength/10;
			if (itemName != null)
			{ 
				string[] splitItemName = itemName.Split('.');
				SMItem charItemToUse = smc.GetEquippedItem(splitItemName[1]);
				charItembaseDamage = charItemToUse.BaseDamage;

				// Check that the person has the skill to use the item (if there are any required skills)
				// Check the player has the required skills
				bool hasAllRequiredSkills = true;
				foreach (SMRequiredSkill smrs in charItemToUse.RequiredSkills)
				{
					hasAllRequiredSkills = smc.HasRequiredSkill(smrs.SkillName, smrs.SkillLevel);
				}

				// If they don't have have all the required skills, set the damage to be 10% of what it should be.
				if (!hasAllRequiredSkills)
				{
					charItembaseDamage = charItembaseDamage * (float)0.1;
				}
			}
			
			// Get the base attribute from the character
			int baseStatValue = smc.Attributes.GetBaseStatValue(baseStat);

			// Work out the damage multiplier based on attribute level (+/-)
			int baseStatRequiredAmount = this.Prerequisites.First(pr => pr.SkillStatName == baseStat).PreReqLevel;
			float positiveNegativeBaseStat = baseStatValue - baseStatRequiredAmount;
            SMSkillHeld theCharacterSkill = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
			int charLevelOfSkill = 0;
			if (theCharacterSkill != null)
			{
				charLevelOfSkill = theCharacterSkill.SkillLevel;
			}
			float damageMultiplier = (positiveNegativeBaseStat + charLevelOfSkill) / 100;

			// Get the target toughness and HP
			int targetToughness, targetHP;
			SMItem targetItem; // for use when it's a target item
			SMCharacter targetChar; // for use when it's a target character
			bool objectAvoidedHit = false; // Need to check whether the character has any defensive type skills that are going to help here.
			string targetName, destroyedObjectType;
			if (targetType == "Character")
			{
				// Get the character
				targetChar = smc.GetRoom().GetPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);

				// Get the toughness and the hitpoints
				targetToughness = targetChar.Attributes.GetToughness();
				targetHP = targetChar.Attributes.HitPoints;
				targetName = targetChar.GetFullName();
				destroyedObjectType = "Corpse of " + targetName;

                // See if they dodge or parry the hit.
                objectAvoidedHit = targetChar.CheckDodgeParry();
            }
			else // Assume it's an item
			{
				// Get the target item
				targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

				// Get the toughness and the hitpoints
				targetToughness = targetItem.Toughness;
				targetHP = targetItem.HitPoints;
				targetName = targetItem.ItemName;
				destroyedObjectType = targetItem.DestroyedOutput;
			}

            if (!objectAvoidedHit)
            {

			    // Hit the target
			    // calculate the actual damage amount
			    float actualDamageAmount = (charItembaseDamage * (1 + damageMultiplier)) - targetToughness;
			    if (actualDamageAmount < 0)
			    {
				    actualDamageAmount = 0;
			    }

			    // Reduce the targets HP
			    int newTargetHP = targetHP - (int)actualDamageAmount;
			
			    // if the targets HP reaches 0 it has "died" or been "destroyed"
			    if (newTargetHP <= 0)
			    {
				    string newItemName = "", oldItemName = "";

				    // Replace the object with the alterobject type
				    if (targetType == "Character")
				    {
                        // TODO Add "Die" method to the character
                        targetChar = smc.GetRoom().GetPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);
                        targetChar.Die();
				    }
				    else // Assume it's an item
				    {
					    // Todo add the new item to the room.
					    // Get the target item
					    targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
					    string[] destroyedObjectInfo = targetItem.DestroyedOutput.Split(',');
					    int numberOfObjectsToCreate = int.Parse(destroyedObjectInfo[1]);
					    oldItemName = targetItem.SingularPronoun + " " + targetItem.ItemName;
					    SMItem destroyedItemType = targetItem.GetDestroyedItem();
					    if (numberOfObjectsToCreate > 1)
					    {
						    newItemName = destroyedItemType.PluralPronoun + " " + destroyedItemType.PluralName + "(" + numberOfObjectsToCreate + ")";
					    }
					    else
					    {
						    newItemName = destroyedItemType.SingularPronoun + " " + destroyedItemType.ItemName;
					    }

					    // Loop around and create the required number of objects
					    while (numberOfObjectsToCreate > 0)
					    {
						    // Reduce the number of items waiting to be created
						    numberOfObjectsToCreate--;

						    // Add the item to the room
						    smc.GetRoom().AddItem(targetItem.GetDestroyedItem());

						    // Remove the destroyed item from the room.
						    smc.GetRoom().RemoveItem(targetItem);
					    }
				    }
				
				    smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, oldItemName, newItemName));
				    SkillIncrease(smc);
				    smc.CurrentActivity = null;
			    }
			    else
			    {
				    if (targetType == "Character")
				    {
					    // TODO Update a character HP
					    targetChar = smc.GetRoom().GetPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);
                        targetChar.Attributes.HitPoints = targetChar.Attributes.HitPoints - (int)actualDamageAmount;
                        if ((int)actualDamageAmount > 0)
					    {
						    smc.sendMessageToPlayer("_Hit " + targetChar.GetFullName() + " for " + (int)actualDamageAmount + " damage_");
                            targetChar.SaveToApplication();
                        }
					    else
					    {
						    smc.sendMessageToPlayer("_You're unable to damage " + targetChar.GetFullName() + "_");
					    }
				    }
				    else
				    {
					    targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
					    smc.GetRoom().UpdateItem(targetItem.ItemID, "HP", newTargetHP);
					    if ((int)actualDamageAmount > 0)
					    {
						    smc.sendMessageToPlayer("_Hit " + targetItem.ItemName + " for " + (int)actualDamageAmount + " damage_");
					    }
					    else
					    {
						    smc.sendMessageToPlayer("_You're unable to damage " + targetItem.ItemName + "_");
					    }
				    }
			    }
            }
            else
            {
                return false;
            }

            // Check to see if we should reduce the objects HP (wear and tear)
            // TODO - Add Method to the object method for wear and tear
            // reduce the objects HP
            //      if the objects HP reaches 0 then the item is "broken" and the player needs to be told via a message.
            //      Stop any repeat actions against the character happening.

            return true;
		}

		private string SuccessOutputParse(string successOutput, SMCharacter smc, string targetName, string objectDestroyedName)
		{
			// replace the elements as needed
			successOutput = successOutput.Replace("{TARGETNAME}", targetName);
			successOutput = successOutput.Replace("{CHARNAME}", smc.GetFullName());
			successOutput = successOutput.Replace("{Object.DestroyedOutput}", objectDestroyedName);

			return successOutput;
		}

		private void SkillIncrease(SMCharacter smc, bool skillSuccess = true)
		{
			// Skill Increase
			// Max skill level
			int maxSkillLevel = 100;
			int failureMultipler = 2;
			if (skillSuccess)
			{
				failureMultipler = 1;
			}

            // Get characters current skill level
            SMSkillHeld theCharacterSkill = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
			int currentSkillLevel = 0;
			if (theCharacterSkill!=null)
			{
				currentSkillLevel = theCharacterSkill.SkillLevel;
			}

			// Chance of the skill increasing in level
			double chanceOfSkillIncrease = ((maxSkillLevel - (currentSkillLevel * failureMultipler)) / 100) / 4;

			// Random chance to see if someone achieves the skill increase change
			Random r = new Random();
			double rDouble = r.NextDouble();
			if (rDouble < chanceOfSkillIncrease)
			{
				// Increase the skill lebel by one.
				smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName).SkillLevel++;

				// Send message to the player.
				smc.sendMessageToPlayer(this.SkillName + " increased in level to " + (currentSkillLevel + 1));
			}

			// Attribute Increase
			// TODO add an attribute increase method check to SMAttributes, very very low chance!
		}

		#endregion

	}

	#region "Other Class Structures for use"

	/// <summary>
	/// A character skill that is associated with the character, also storing their level.
	/// </summary>
	public class SMSkillHeld
	{
		[JsonProperty("SkillName")]
		public string SkillName { get; set; }

		[JsonProperty("SkillLevel")]
		public int SkillLevel { get; set; }
	}

	/// <summary>
	/// A skill prerequisite (i.e. something the need before they can train the skill).
	/// </summary>
	public class SMSkillPrerequisite
	{
		[JsonProperty("IsSkill")]
		public bool IsSkill { get; set; }

		[JsonProperty("SkillStatName")]
		public string SkillStatName { get; set; }

		[JsonProperty("PreReqLevel")]
		public int PreReqLevel { get; set; }
	}

	/// <summary>
	/// A skill type, because different types group together.
	/// </summary>
	public class SMSkillType
	{
		[JsonProperty("SkillTypeName")]
		public string SkillTypeName { get; set; }

		[JsonProperty("SkillTypeDescription")]
		public string SkillTypeDescription { get; set; }
	}

    /// <summary>
    /// Skill steps represent the steps that something uses to use the skill.
    /// 
    /// Step types include:
    /// Object: Required Object, i.e. you must have an object of a type, like an axe to chop something.
    /// EquippedObject: Required Object is equipped, i.e. you must have an object of a type, like an axe to chop something.
    /// Target: Required Target, i.e. you must target something to use the skill, like a tree if you want to chop it.
    /// CheckComplete: Check whether the task is completed, i.e. if you're forging a sword you need to check whether it's completed yet.
    /// Hit: Hits something with the object that they've used.
    /// Pause: Pause timing so the system doesn't just loop like a crazy! - required amount denotes the pause time in seconds.
    /// Repeat: Repeats everything until stopped (i.e. will continue to do something over and over).
    /// Information: Information output.
    /// </summary>
    public class SMSkillStep
	{
		// Different types of steps have different code that runs them.
		[JsonProperty("StepType")]
		public string StepType { get; set; }

		// If an object is required it should be shown in here.
		[JsonProperty("StepRequiredObject")]
		public string StepRequiredObject { get; set; }

		// Required Object Amount
		[JsonProperty("RequiredObjectAmount")]
		public int RequiredObjectAmount { get; set; }

		// Failure output text
		[JsonProperty("FailureOutput")]
		public string FailureOutput { get; set; }

		// Success output text
		[JsonProperty("SuccessOutput")]
		public string SuccessOutput { get; set; }
	}

	#endregion

}