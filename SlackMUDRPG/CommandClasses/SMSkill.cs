using Newtonsoft.Json;
using SlackMUDRPG.Utility.Formatters;
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

		[JsonProperty("CanUseWithoutLearning")]
		public bool CanUseWithoutLearning { get; set; }

		[JsonProperty("Prerequisites")]
		public List<SMSkillPrerequisite> Prerequisites { get; set; }

		[JsonProperty("SkillSteps")]
		public List<SMSkillStep> SkillSteps { get; set; }

		/// <summary>
		/// Holds the class instance of the response formater.
		/// </summary>
		private ResponseFormatter Formatter = null;

		/// <summary>
		/// Class constructor
		/// </summary>
		public SMSkill()
		{
			this.Formatter = ResponseFormatterFactory.Get();
		}

		public void UseSkill(SMCharacter smc, out string messageOut, out float floatOut, string extraData, int skillLoop, bool beginSkillUse = true, string targetType = null, string targetID = null, bool isPassive = false)
		{
			// Output variables for passive skills that need output (like "dodge")
			messageOut = "";
			floatOut = 0;

            if (skillLoop < 5)
            {
                // Increase the loop number
                var newSkillLoop = skillLoop + 1;
				string originCharUserID = smc.UserID;

			    // Get the actual instance of the character!
			    smc = new SlackMud().GetAllCharacters(originCharUserID); // TODO make this work with an NPC!

			    // Set the character activity
			    if (beginSkillUse)
			    {
				    smc.CurrentActivity = this.ActivityType;
			    }

				var continueCycle = true;

				// Loop around the steps
				foreach (SMSkillStep smss in this.SkillSteps)
				{
					if (continueCycle)
					{
						// Get the character again each time we go around the loop
						smc = new SlackMud().GetAllCharacters(originCharUserID);

						if (smc.CurrentActivity == this.ActivityType)
						{
							switch (smss.StepType)
							{
								case "Object":
									if ((!StepRequiredObject(smc, smss.StepRequiredObject, smss.RequiredObjectAmount)) && (!isPassive))
									{
										smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
										continueCycle = false;
										smc.StopActivity();
									}
									break;
								case "EquippedObject":
									if ((!StepRequiredObject(smc, smss.StepRequiredObject, smss.RequiredObjectAmount, true)) && (!isPassive))
									{
										smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
										continueCycle = false;
										smc.StopActivity();
									}
									break;
								case "Target":
									if ((!StepRequiredTarget(smc, smss, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID)) && (!isPassive))
									{
										string getTargetName = GetTargetName(smc, targetType, targetID);
										if (getTargetName != null)
										{
											smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput, smc, getTargetName, null)));
										}
										continueCycle = false;
										smc.StopActivity();
									}
									break;
								case "Hit":
									if (!StepHit(smss, smc, this.BaseStat, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
									{
										if (!isPassive)
										{
											string getTargetName = GetTargetName(smc, targetType, targetID);
											if (getTargetName != null)
											{
												smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput, smc, getTargetName, null)));
											}
										}
										SkillIncrease(smc, false);
									}
									else
									{
										SkillIncrease(smc, true);
									}
									break;
								case "HitMulti":
									if (!StepHitMulti(smss, smc, this.BaseStat, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
									{
										if (!isPassive)
										{
											string getTargetName = GetTargetName(smc, targetType, targetID);
											if (getTargetName != null)
											{
												smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput, smc, getTargetName, null)));
											}
										}
										SkillIncrease(smc, false);
									}
									else
									{
										SkillIncrease(smc, true);
									}
									break;
								case "CheckReceipe":
									if (!CheckReceipe(smss, smc, this.BaseStat, extraData, null, 0, null))
									{
                                        smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
                                        continueCycle = false;
										smc.StopActivity();
									}
									break;
								case "UseReceipe":
									if (!UseReceipe(smss, smc, this.BaseStat, extraData, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
									{
										string targetItem = String.Empty;
										SMReceipe smr = this.GetRecipeByName(extraData);

										if (smr != null)
										{
											SMItem producedItem = smr.GetProducedItem();
											targetItem = $"{producedItem.SingularPronoun} {producedItem.ItemName}";
										}

										smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput, smc, targetItem, null)));
										continueCycle = false;
									}
									smc.StopActivity();
									break;
                                case "Cast":
                                    string targetName = "";
                                    if (targetType != null)
                                    {
                                        if (targetType == "Character")
                                        {
                                            var targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);
                                            targetName = targetChar.GetFullName();
                                        }
                                        else
                                        {
                                            var targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
                                            targetName = targetItem.ItemName;
                                        }
                                    }
                                    
                                    if (!Cast(smss, smc, this.BaseStat, extraData, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
                                    {
                                        smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput, smc, targetName, "")));
                                    }
                                    else
                                    {
                                        smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.SuccessOutput, smc, targetName, "")));
                                        SkillIncrease(smc, true);
                                    }
                                    break;
                                case "Information":
                                    string targetNameInformation = "";
                                    if (targetType != null)
                                    {
                                        if (targetType == "Character")
                                        {
                                            // Get the character
                                            var targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);
                                            targetNameInformation = targetChar.GetFullName();
                                        }
                                        else
                                        {
                                            var targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
                                            targetNameInformation = targetItem.SingularPronoun + " " + targetItem.ItemName;
                                        }
                                    }

                                    smc.GetRoom().Announce(this.Formatter.Italic(SuccessOutputParse(smss.SuccessOutput, smc, targetNameInformation, "")));
                                    break;
								case "OwnedObject":
									if (!CheckHasItem(smss, smc))
									{
										smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
										continueCycle = false;
										smc.StopActivity();
									}
									break;
								case "ConsumeObject":
									if (!ConsumeItem(smss, smc, targetType, targetID))
									{
										smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
										continueCycle = false;
										smc.StopActivity();
									}
									break;
								case "CreateDestroyedObject":
									if (!CreateDestroyedObject(smss, smc, targetID))
									{
										smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
									};
									break;
                                case "CheckObjectInLocation":
                                    if (!CheckObjectinLocation(smss, smc))
                                    {
                                        smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
										smc.StopActivity();
									};
                                    break;
                                case "CheckRoomProperty":
                                    if (!CheckRoomProperty(smss, smc))
                                    {
                                        smc.sendMessageToPlayer(this.Formatter.Italic(smss.FailureOutput));
										smc.StopActivity();
									};
                                    break;
                                case "SkillCheck":
                                    if (!SkillCheck(smss, smc))
                                    {
                                        // Failed
                                        smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput,smc,null,null)));
										smc.StopActivity();
									}
                                    else
                                    {
                                        // Passed
                                        // Create any objects that are associated with this.
                                        if (smss.ExtraData != null)
                                        {
                                            // Get the various parts for the object creation...
                                            string[] objectCreationInfo = smss.ExtraData.Split('|');
                                            string[] objectNameInfo = objectCreationInfo[0].Split('.');

                                            // Check if an object is created this time
                                            Random rSkillCheck = new Random();
                                            double rDoubleSkillCheck = rSkillCheck.NextDouble();

                                            // Check if an object is created...
                                            if (rDoubleSkillCheck * 100 <= int.Parse(objectCreationInfo[2]))
                                            {
                                                // ... loop around the number
                                                int loopNumber = int.Parse(objectCreationInfo[1]);

                                                // Loop around and create some items.
                                                while (loopNumber > 0)
                                                {
                                                    loopNumber--;
                                                    smc.GetRoom().AddItem(SMItemFactory.Get(objectNameInfo[0], objectNameInfo[1]));
                                                    smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.SuccessOutput, smc, null, null)));
                                                }
                                                SkillIncrease(smc, true);
                                            }
                                            else // Failure anyway.. skill passed, but they failed :/
                                            {
                                                smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(smss.FailureOutput, smc, null, null)));
                                            }
                                        }
                                    };
                                    break;
                                case "Pause":
									System.Threading.Thread.Sleep(smss.RequiredObjectAmount * 1000);
									break;
								case "Repeat":
									this.UseSkill(smc, out messageOut, out floatOut, extraData, newSkillLoop, false, targetType, targetID, isPassive);
									break;
							}
						}
					}
				}
			}
            else
            {
                smc.StopActivity();
            }
        }

		private string GetTargetName(SMCharacter smc, string targetType, string targetID)
		{
			if (targetType == "Character")
			{
				// Get the character
				SMCharacter targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);
				if (targetChar != null)
				{
					return targetChar.GetFullName();
				}
			}
			else
			{
				// Get the target item
				SMItem targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
				if (targetItem != null)
				{
					return targetItem.SingularPronoun + " " + targetItem.ItemName;
				}
			}

			return null;
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
                    hasItem = smc.HasItemEquipped(splitRequiredObjectType[1]);
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
                hasEnoughOfItem = (smc.CountOwnedItems(splitRequiredObjectType[1]) >= requiredObjectAmount);
            }
            
            return hasItem && hasEnoughOfItem;
		}

		private bool StepRequiredTarget(SMCharacter smc, SMSkillStep smss, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
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

						if (smss.ExtraData != null)
						{
							string[] ExtraDataInfo = smss.ExtraData.Split(',');
							foreach (string extraDataItem in ExtraDataInfo)
							{
								string[] extraDataItemSplit = extraDataItem.Split('.');

								if (extraDataItemSplit[0] == "Trait")
								{
									if (targetItem.ObjectTrait != extraDataItemSplit[1])
									{
										smc.sendMessageToPlayer(this.Formatter.Italic(smss.SuccessOutput));
										return false;
									}
								}
							}
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

			if (smss.ExtraData != null)
			{
				if ((targetType == "Character") && (smss.ExtraData.ToLower() == "object"))
				{
					return false;
				}
			}

			return true;
		}

		private bool StepHitMulti(SMSkillStep smss, SMCharacter smc, string baseStat, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
		{
			// Get the object to hit the target with.
			string itemName = smss.StepRequiredObject;
			float charItembaseDamage = smc.Attributes.Strength/10;
			if (itemName != null)
			{ 
				string[] splitItemName = itemName.Split('.');
				SMItem charItemToUse = smc.GetEquippedItem(splitItemName[1]);
				if (charItemToUse != null)
				{
					charItembaseDamage = charItemToUse.BaseDamage;

					// Check that the person has the skill to use the item (if there are any required skills)
					// Check the player has the required skills
					bool hasAllRequiredSkills = true;
					if (charItemToUse.RequiredSkills != null)
					{
						foreach (SMRequiredSkill smrs in charItemToUse.RequiredSkills)
						{
							hasAllRequiredSkills = smc.HasRequiredSkill(smrs.SkillName, smrs.SkillLevel);
						}
					}

					// If they don't have have all the required skills, set the damage to be 10% of what it should be.
					if (!hasAllRequiredSkills)
					{
						charItembaseDamage = charItembaseDamage * (float)0.1;
					}
				}
				else
				{
					smc.sendMessageToPlayer(this.Formatter.General("You do not have the required item equipped"));
					return false;
				}
			}
			
			// Get the base attribute from the character
			int baseStatValue = smc.Attributes.GetBaseStatValue(baseStat);

			// Work out the damage multiplier based on attribute level (+/-)
			int baseStatRequiredAmount = this.Prerequisites.First(pr => pr.SkillStatName == baseStat).PreReqLevel;
			float positiveNegativeBaseStat = baseStatValue - baseStatRequiredAmount;

			SMSkillHeld theCharacterSkill = null;
			if (smc.Skills != null)
			{
                theCharacterSkill = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
			}
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
				targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);

				if (targetChar != null)
				{
					// Get the toughness and the hitpoints
					targetToughness = targetChar.Attributes.GetToughness();
					targetHP = targetChar.Attributes.HitPoints;
					targetName = targetChar.GetFullName();
					destroyedObjectType = "the corpse of " + targetName;

					// See if they dodge or parry the hit.
					objectAvoidedHit = targetChar.CheckDodgeParry();
				}
				else
				{
					smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
					smc.StopActivity();
					return false;
				}
			}
			else // Assume it's an item
			{
				// Get the target item
				targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

				if (targetItem != null)
				{
					// Get the toughness and the hitpoints
					targetToughness = targetItem.Toughness;
					targetHP = targetItem.HitPoints;
					targetName = targetItem.ItemName;
					destroyedObjectType = targetItem.DestroyedOutput;
				}
				else
				{
					smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
					smc.StopActivity();
					return false;
				}
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

					// Todo add the new item to the room.
					// Get the target item
					targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

					if (targetItem != null)
					{
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

						smc.GetRoom().Announce(this.Formatter.Italic(SuccessOutputParse(smss.SuccessOutput, smc, oldItemName, newItemName)));
						SkillIncrease(smc);
						smc.CurrentActivity = null;
					}
					else
					{
						smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
						smc.StopActivity();
						return false;
					}
				}
				else
				{
					targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

					if (targetItem != null)
					{
						smc.GetRoom().UpdateItem(targetItem.ItemID, "HP", newTargetHP);
						if ((int)actualDamageAmount > 0)
						{
							double chanceOfObjectCreation = double.Parse(smss.ExtraData);
							// Random chance to see if someone achieves the skill increase change
							Random r = new Random();
							double rDouble = r.NextDouble();
						
							// Check if the object is created.
							if ((rDouble * 100) < chanceOfObjectCreation+charLevelOfSkill)
							{
								SMItem destroyedItemType = targetItem.GetDestroyedItem();
							
								string newItemName = destroyedItemType.SingularPronoun + " " + destroyedItemType.ItemName;
								string oldItemName = targetItem.SingularPronoun + " " + targetItem.ItemName;

								targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);
								string[] destroyedObjectInfo = targetItem.DestroyedOutput.Split(',');
								int numberOfObjectsToCreate = int.Parse(destroyedObjectInfo[1]);
								oldItemName = targetItem.SingularPronoun + " " + targetItem.ItemName;

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
								}

								smc.GetRoom().Announce(this.Formatter.Italic(SuccessOutputParse(smss.SuccessOutput, smc, oldItemName, newItemName)));
								SkillIncrease(smc);
							}
						}
						else
						{
							smc.sendMessageToPlayer(this.Formatter.Italic($"You're unable to damage {targetItem.ItemName}"));
						}
					}
					else
					{
						smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
						smc.StopActivity();
						return false;
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

        private bool Cast(SMSkillStep smss, SMCharacter smc, string baseStat, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
        {
            // Check whether the casting is completed properly or not
            // Get the base attribute from the character
            int baseStatValue = smc.Attributes.GetBaseStatValue(baseStat);

            // Work out the damage multiplier based on attribute level (+/-)
            int baseStatRequiredAmount = 0;
            if (this.Prerequisites != null)
            {
                baseStatRequiredAmount = this.Prerequisites.First(pr => pr.SkillStatName == baseStat).PreReqLevel;
            }

            float positiveNegativeBaseStat = baseStatValue - baseStatRequiredAmount;

            SMSkillHeld theCharacterSkill = null;
            if (smc.Skills != null)
            {
                theCharacterSkill = smc.Skills.FirstOrDefault(skill => skill.SkillName.ToLower() == this.SkillName.ToLower());
            }
            int charLevelOfSkill = 0;
            if (theCharacterSkill != null)
            {
                charLevelOfSkill = theCharacterSkill.SkillLevel;
            }
            float successMultiplier = (positiveNegativeBaseStat + charLevelOfSkill) / 100;

            Random r = new Random();
            double rDouble = r.NextDouble() * 100;
            double checkAmount = ((charLevelOfSkill * successMultiplier) * 100) + 20;
            if (rDouble < checkAmount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		private bool StepHit(SMSkillStep smss, SMCharacter smc, string baseStat, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
		{
			// Get the object to hit the target with.
			string itemName = smss.StepRequiredObject;
			float charItembaseDamage = smc.Attributes.Strength / 10;
			bool isRanged = false;
			if (itemName != null)
			{
				string[] splitItemName = itemName.Split('.');
				SMItem charItemToUse = smc.GetEquippedItem(splitItemName[1]);
				if (charItemToUse != null)
				{
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
						if (charItembaseDamage < 1)
						{
							charItembaseDamage = 1;
						}
					}

					// Set whether the item is ranged or not.
					if (charItemToUse.ItemType == "RangedWeapon")
					{
						isRanged = true;
					}
				}
				else
				{
					return false;
				}
			}
            else // lets just use whatever is equipped in their hands.
            {
				SMItem smid = smc.GetEquippedItem();

				if (smid != null)
				{
					charItembaseDamage = smc.GetEquippedItem().BaseDamage * (float)0.1;
				}
            }

			// Get the base attribute from the character
			int baseStatValue = smc.Attributes.GetBaseStatValue(baseStat);

			// Work out the damage multiplier based on attribute level (+/-)
			int baseStatRequiredAmount = 0;
			if (this.Prerequisites != null) {
				baseStatRequiredAmount = this.Prerequisites.First(pr => pr.SkillStatName == baseStat).PreReqLevel;
			}
			
			float positiveNegativeBaseStat = baseStatValue - baseStatRequiredAmount;

			SMSkillHeld theCharacterSkill = null;
			if (smc.Skills != null)
			{
				smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
			}
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
				targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);

				if (targetChar != null)
				{
					// Get the toughness and the hitpoints
					targetToughness = targetChar.Attributes.GetToughness();
					targetHP = targetChar.Attributes.HitPoints;
					targetName = targetChar.GetFullName();
					destroyedObjectType = "the corpse of " + targetName;

					// See if they dodge or parry the hit.
					if (!isRanged)
					{
						objectAvoidedHit = targetChar.CheckDodgeParry();
					}
				}
				else
				{
					smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
					smc.StopActivity();
					return false;
				}
			}
			else // Assume it's an item
			{
				// Get the target item
				targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

				if (targetItem != null)
				{
					// Get the toughness and the hitpoints
					targetToughness = targetItem.Toughness;
					targetHP = targetItem.HitPoints;
					targetName = targetItem.ItemName;
					destroyedObjectType = targetItem.DestroyedOutput;
				}
				else
				{
					smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
					smc.StopActivity();
					return false;
				}
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
						targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);

						if (targetChar != null)
						{
							targetChar.Die();
							oldItemName = targetChar.GetFullName();
							newItemName = destroyedObjectType;
						}
						else
						{
							smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
							smc.StopActivity();
							return false;
						}
					}
					else // Assume it's an item
					{
						// Todo add the new item to the room.
						// Get the target item
						targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

						if (targetItem != null)
						{
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
						else
						{
							smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
							smc.StopActivity();
							return false;
						}
                    }
                    // Announce the creation of the item.
                    smc.GetRoom().Announce(this.Formatter.Italic(SuccessOutputParse(smss.SuccessOutput, smc, oldItemName, newItemName)));

                    // Skill Increase
                    SkillIncrease(smc);
					smc.CurrentActivity = null;
				}
				else
				{
					if (targetType == "Character")
					{
						// TODO Update a character HP
						targetChar = smc.GetRoom().GetAllPeople().FirstOrDefault(roomCharacters => roomCharacters.UserID == targetID);

						if (targetChar != null)
						{
							targetChar.Attributes.HitPoints = targetChar.Attributes.HitPoints - (int)actualDamageAmount;
							if ((int)actualDamageAmount > 0)
							{
								smc.sendMessageToPlayer(this.Formatter.Italic($"Hit {targetChar.GetFullName()} for {(int)actualDamageAmount} damage"));
								targetChar.sendMessageToPlayer(this.Formatter.Italic($"You were hit by {smc.GetFullName()} for {(int)actualDamageAmount} damage (HP {targetChar.Attributes.HitPoints} / {targetChar.Attributes.MaxHitPoints} remaining)"));
								targetChar.SaveToApplication();
							}
							else
							{
								smc.sendMessageToPlayer(this.Formatter.Italic($"You're unable to damage {targetChar.GetFullName()}"));
							}
						}
						else
						{
							smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
							smc.StopActivity();
							return false;
						}
					}
					else
					{
						targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemID == targetID);

						if (targetItem != null)
						{
							smc.GetRoom().UpdateItem(targetItem.ItemID, "HP", newTargetHP);
							if ((int)actualDamageAmount > 0)
							{
								smc.sendMessageToPlayer(this.Formatter.Italic($"Hit {targetItem.ItemName} for {(int)actualDamageAmount} damage"));
							}
							else
							{
								smc.sendMessageToPlayer(this.Formatter.Italic($"You're unable to damage {targetItem.ItemName}"));
							}
						}
						else
						{
							smc.sendMessageToPlayer(this.Formatter.Italic("Cannot find the target"));
							smc.StopActivity();
							return false;
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

		/// <summary>
		/// Gets a recipe by its name
		/// </summary>
		/// <param name="recipeName">Name of the recipe to search for</param>
		/// <returns>SMReceipe or null</returns>
		private SMReceipe GetRecipeByName(string recipeName)
		{
			List<SMReceipe> smrl = (List<SMReceipe>)HttpContext.Current.Application["SMReceipes"];

			return smrl.FirstOrDefault(receipe => receipe.Name.ToLower() == recipeName.ToLower());
		}
		
		/// <summary>
		/// Check the player has an item (i.e. an arrow) and enough of them for the task.
		/// </summary>
		/// <param name="smss">The skill step</param>
		/// <param name="smc">The character</param>
		/// <returns>A true or false to say if they have the item or not</returns>
		private bool CheckHasItem(SMSkillStep smss, SMCharacter smc)
		{
			// Get the item and check that there is enough of it...
			string[] itemTypeSplit = smss.StepRequiredObject.Split('.');
			if (smc.CountOwnedItems(itemTypeSplit[1]) >= smss.RequiredObjectAmount)
			{
				// Return true if they have enough of it.
				return true;
			};

			// .. or return false if they don't.
			return false;
		}

		/// <summary>
		/// Check the player has an item (i.e. an arrow), enough of them for the task and then consume it.
		/// </summary>
		/// <param name="smss">The skill step</param>
		/// <param name="smc">The character</param>
		/// <returns>A true or false to say if they had the item or not</returns>
		private bool ConsumeItem(SMSkillStep smss, SMCharacter smc, string targetType, string targetID)
		{
			// Check the player has the item and enough of it...
			if (smss.StepRequiredObject != "{TARGET}")
			{
				string[] itemTypeSplit = smss.StepRequiredObject.Split('.');
				if (smc.CountOwnedItems(itemTypeSplit[1]) > smss.RequiredObjectAmount)
				{
					// if they have enough remove them (i.e. consume them)
					int numberToConsume = smss.RequiredObjectAmount;

					// Loop around the number
					while (numberToConsume > 0)
					{
						// reduce the number
						numberToConsume--;

						// consume an item
						smc.RemoveOwnedItem(itemTypeSplit[1]);
					}

					// Now return true
					return true;
				};
			}
			else
			{
				// See if the item is in the room
				SMItem itemToRemove = smc.FindItemInRoom(targetID);
				if (itemToRemove != null)
				{
					// Remove the item from the room
					smc.GetRoom().RemoveItem(itemToRemove);
					return true;
				}
				else
				{
					// is the player holding it or has it in their bag?
					itemToRemove = smc.GetOwnedItem(targetID);
					if (itemToRemove != null)
					{
						smc.RemoveOwnedItem(targetID);
						return true;
					}
				}
			}
			
			// else return false because they don't have enough of the item!
			return false;
		}

		/// <summary>
		/// Create a destroyed object
		/// </summary>
		private bool CreateDestroyedObject(SMSkillStep smss, SMCharacter smc, string targetID)
		{
			// Get the target item, so we can find what is going to be destroyed
			SMItem targetItem = smc.FindItemInRoom(targetID);
			if (targetItem == null)
			{
				targetItem = smc.FindItemInRoom(targetID);
			}
			
			if (targetItem != null)
			{
				List<SMItem> smil = targetItem.GetDestroyedItems();

				SMRoom smr = smc.GetRoom();
				foreach (SMItem smi in smil)
				{
					smi.ItemName = ReplaceTags(smi.ItemName, targetItem);
					smr.AddItem(smi);
					smr.Announce(this.Formatter.Italic($"{smc.GetFullName()} creates {smi.SingularPronoun} {smi.ItemName}"));
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Replace tag in the string
		/// </summary>
		/// <param name="inString">The string to replace things within.</param>
		/// <param name="targetItem">The item to be used for replacements.</param>
		/// <returns>A string with the tags replaced</returns>
		private string ReplaceTags(string inString, SMItem targetItem)
		{
			string returnString = inString;

			if (targetItem.PreviousItemFamily != null)
			{
				returnString = returnString.Replace("{Family}", targetItem.PreviousItemFamily);
			}

			return returnString;
		}

		/// <summary>
		/// Checks that there is an object of a type / family in a location
		/// </summary>
		/// <returns></returns>
		private bool CheckObjectinLocation(SMSkillStep smss, SMCharacter smc)
		{
			// Check if the character has it equipped.
			if (CheckHasItem(smss, smc))
			{
				return true;
			}

			// If not check whether the item is in the location
			string[] splitObjectType = smss.StepRequiredObject.Split('.');
			SMItem smi = smc.FindItemInRoom(splitObjectType[1]);
			if (smi != null)
			{
				if (smss.ExtraData != null)
				{
					splitObjectType = smss.ExtraData.Split('.');
					if (SMItemHelper.CountItemsInContainer(smi, splitObjectType[1].ToLower()) > 0)
					{
						return true;
					}
				}

				return true;
			}

			return false;
		}

        /// <summary>
        /// Check that the room ha sa specific property
        /// </summary>
        /// <param name="smss">The skill step</param>
        /// <param name="smc">The character</param>
        /// <returns></returns>
        private bool CheckRoomProperty(SMSkillStep smss, SMCharacter smc)
        {
            // Find the property name
            string propertyName = smss.StepRequiredObject;

            // Check the room has the property - returned from the HasProperty command.

            return smc.GetRoom().HasProperty(propertyName);
        }

        private bool SkillCheck(SMSkillStep smss, SMCharacter smc)
        {
            // Random
            Random rSkillCheck = new Random();
            double rDoubleSkillCheck = (rSkillCheck.NextDouble() * 100);

            // Work out bonuses
            // Get the base attribute from the character
            int baseStatValue = smc.Attributes.GetBaseStatValue(this.BaseStat);

            // Work out the damage multiplier based on attribute level (+/-)
            int baseStatRequiredAmount = this.Prerequisites.First(pr => pr.SkillStatName == this.BaseStat).PreReqLevel;
            float positiveNegativeBaseStat = baseStatValue - baseStatRequiredAmount;

            SMSkillHeld theCharacterSkill = null;
            if (smc.Skills != null)
            {
                theCharacterSkill = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
            }
            int charLevelOfSkill = 0;
            if (theCharacterSkill != null)
            {
                charLevelOfSkill = theCharacterSkill.SkillLevel;
            }
            float checkSkillUse = ((positiveNegativeBaseStat + charLevelOfSkill) / 100) * 100;

            // Return true or false
            if (rDoubleSkillCheck <= checkSkillUse)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks the receipe the character is trying to use.
        /// </summary>
        private bool CheckReceipe(SMSkillStep smss, SMCharacter smc, string baseStat, string nameOfReceipe, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
		{
			// Check that the character knows the receipe or it's a receipe that everyone knows how to make intuitively.
			SMReceipe smr = this.GetRecipeByName(nameOfReceipe);

			if (smr != null)
			{
				if (smr.NeedToLearn)
				{
					// Check the character knows the receipe
					// return false if they don't know it.
					if (smc.KnownRecipes == null || smc.KnownRecipes.FirstOrDefault(k => k.ToLower() == nameOfReceipe.ToLower()) == null)
					{
						return false;
					}
				}

				// If the extra data field is filled in there is a required trait.
				if (smss.ExtraData != null)
				{
					// Check that the item they're trying to make has the right trait.
					if (!smr.ProductionTrait.Contains(smss.ExtraData))
					{
						return false;
					}
				}
			} 
			else // The receipe they've specified doesn't exist.
			{
				return false;
			}
			
			return true;
		}

		private bool UseReceipe(SMSkillStep smss, SMCharacter smc, string baseStat, string nameOfReceipe, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
		{
			// Check that the character knows the receipe or it's a receipe that everyone knows how to make intuitively.
			SMReceipe smr = this.GetRecipeByName(nameOfReceipe);
			if (smr != null)
			{
				var continueCycle = true;

				// Loop around the steps
				foreach (SMSkillStep receipeStep in smr.SkillSteps)
				{
					if (continueCycle)
					{
						// Get the character again each time we go around the loop
						smc = new SlackMud().GetAllCharacters(smc.UserID);

						if (smc.CurrentActivity == this.ActivityType)
						{
							switch (receipeStep.StepType)
							{
								case "CheckConsumeItems":
									// Check that the items needed for this task are available.

									bool allFound = true;

									foreach (SMReceipeMaterial smrm in smr.Materials)
									{
										bool materialFound = false;
										string[] materialType = smrm.MaterialType.Split('.');
                                        // Check if the character is holding the item or has it equipped

                                        int numberFound = smc.CountOwnedItems(materialType[1]);

                                        if (numberFound > 0)
										{
											materialFound = true;
										}

                                        // Is the item in a container the character is wearing?

										// Check if the items are in the location with the character
										if (!materialFound)
										{
											SMItem checkForItem = smc.GetRoom().GetItemByFamilyName(materialType[1]);
											if (checkForItem != null)
											{
												materialFound = true;
											}
											else
											{
												smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(receipeStep.FailureOutput, smc, materialType[1], "")));
												allFound = false;
											}
										}
									}

									if (!allFound)
									{
										return false;
									}
									break;
								case "ConsumeItems":
									// Consume the items.
									foreach (SMReceipeMaterial smrm in smr.Materials)
									{
										string[] materialType = smrm.MaterialType.Split('.');
                                        int currentNumber = smrm.MaterialQuantity;
										
										// Check if the character is holding the item
                                        while (currentNumber>0)
                                        {
                                            bool materialFound = false;

                                            materialFound = smc.RemoveItem(materialType[1], true);
                                            
                                            if (!materialFound)
                                            {
                                                smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(receipeStep.FailureOutput, smc, materialType[1], "")));
                                                currentNumber = 0;
                                                return false;
                                            }
                                            else
                                            {
                                                currentNumber--;
                                            }
                                        }
									}
									break;
								case "CreateObject":
									// Create the object.
									// Create a new object of the type for the receipe.
									SMItem smi = smr.GetProducedItem();
                                    string originalItemName = smi.ItemName;
									smi.ItemID = Guid.NewGuid().ToString();

									// Check the threshold reached for this item...
									Random r = new Random();
									double rDouble = r.NextDouble();
									int baseItemWeight = smi.ItemWeight;
									int baseItemSize = smi.ItemSize;
									int baseHitPoints = smi.HitPoints;
									float baseDamage = smi.BaseDamage;
									int baseToughness = smi.Toughness;
                                    int baseItemCapacity = smi.ItemCapacity;

                                    // Get the character level
                                    SMSkillHeld smsh = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
                                    int characterSkillLevel = 0;
                                    if (smsh != null)
                                    {
                                        characterSkillLevel = (int)smsh.SkillLevel;
                                    }
                                    
									foreach (SMReceipeStepThreshold smrst in smr.StepThresholds)
									{
										if (((rDouble * 100) + characterSkillLevel) >= smrst.ThresholdLevel)
                                        {
                                            smi.ItemName = smrst.ThresholdName + " " + originalItemName;

											if (smrst.ThresholdBonus!= null)
											{
												foreach (SMReceipeStepThresholdBonus smrstb in smrst.ThresholdBonus)
												{
													switch (smrstb.ThresholdBonusName)
													{
														case "BaseDamage":
															smi.BaseDamage = baseDamage + smrstb.ThresholdBonusValue;
															break;
														case "Toughness":
															smi.Toughness = baseToughness + smrstb.ThresholdBonusValue;
															break;
														case "HitPoints":
															smi.HitPoints = baseHitPoints + smrstb.ThresholdBonusValue;
															break;
														case "ItemSize":
															smi.ItemSize = baseItemSize + smrstb.ThresholdBonusValue;
															break;
														case "ItemWeight":
															smi.ItemWeight = baseItemWeight + smrstb.ThresholdBonusValue;
															break;
                                                        case "ItemCapacity":
                                                            smi.ItemCapacity = baseItemCapacity + smrstb.ThresholdBonusValue;
                                                            break;
                                                    }
												}
											}
										}
									}

									// Place it in the location where the character is.
									smc.GetRoom().AddItem(smi);
                                    smc.sendMessageToPlayer(this.Formatter.Italic(SuccessOutputParse(receipeStep.SuccessOutput, smc, smi.SingularPronoun + " " + smi.ItemName, "")));

                                    break;
                                case "Information":
                                    SMItem producedItem = smr.GetProducedItem();
                                    smc.GetRoom().Announce(this.Formatter.Italic(SuccessOutputParse(receipeStep.SuccessOutput, smc, producedItem.SingularPronoun + " " + producedItem.ItemName, "")));
                                    break;
								case "Pause":
									System.Threading.Thread.Sleep(receipeStep.RequiredObjectAmount * 1000);
									break;
							}
						}
					}
				}
			}

			// Return true from this
			return true;
		}

		private string SuccessOutputParse(string successOutput, SMCharacter smc, string targetName, string objectDestroyedName)
		{
            // replace the elements as needed
            successOutput = successOutput.Replace("{TARGETNAME}", targetName);
            successOutput = successOutput.Replace("{ITEMNAME}", targetName);
            successOutput = successOutput.Replace("{CHARNAME}", smc.GetFullName());
			successOutput = successOutput.Replace("{Object.DestroyedOutput}", objectDestroyedName);

			return successOutput;
		}

		public void SkillIncrease(SMCharacter smc, bool skillSuccess = true)
		{
			// Skill Increase
			// Max skill level
			int maxSkillLevel = 20;

			// Get characters current skill level
			SMSkillHeld theCharacterSkill = null;
			if (smc.Skills != null)
			{
                theCharacterSkill = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);
			}
			int currentSkillLevel = 0;
			if (theCharacterSkill!=null)
			{
				currentSkillLevel = theCharacterSkill.SkillLevel;
			}

            if (currentSkillLevel < maxSkillLevel)
            {
                // Chance of the skill increasing in level
                double chanceOfSkillIncrease = 20 - currentSkillLevel;
                if ((chanceOfSkillIncrease < 0) || (!skillSuccess))
                {
                    chanceOfSkillIncrease = 1;
                }

                // Random chance to see if someone achieves the skill increase change
				int randomChance = (new Random().Next(1, 100));
				if (randomChance <= chanceOfSkillIncrease)
                {
                    // Increase the skill lebel by one.
                    if (smc.Skills != null)
                    {
                        SMSkillHeld smshincrease = smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName);

                        if (smshincrease != null)
                        {
                            smc.Skills.FirstOrDefault(skill => skill.SkillName == this.SkillName).SkillLevel++;

                            // Send message to the player.
                            smc.sendMessageToPlayer(this.Formatter.Italic(this.LevelIncreaseText + " (" + this.SkillName + " increased in level to " + (currentSkillLevel + 1) + ")"));
                        }
                        else
                        {
                            LearnNewSkill(smc); 
                        }
                    }
                    else
                    {
                        LearnNewSkill(smc);
                    }
                }

                // Attribute Increase
                // TODO add an attribute increase method check to SMAttributes, very very low chance!

                smc.SaveToApplication();
                smc.SaveToFile();
            }
		}

		public void LearnNewSkill(SMCharacter smc)
		{
			SMSkillHeld newSkill = new SMSkillHeld();
			newSkill.SkillName = this.SkillName;
			newSkill.SkillLevel = 1;
            if (smc.Skills!=null)
            {
                smc.Skills.Add(newSkill);
            }
            else
            {
                List<SMSkillHeld> newSkillGroup = new List<SMSkillHeld>();
                newSkillGroup.Add(newSkill);
                smc.Skills = newSkillGroup;
            }
			
			smc.sendMessageToPlayer(this.Formatter.Italic(this.SkillLearnText));
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

		// Required Object Amount
		[JsonProperty("ExtraData")]
		public string ExtraData { get; set; }

		// Failure output text
		[JsonProperty("FailureOutput")]
		public string FailureOutput { get; set; }

		// Success output text
		[JsonProperty("SuccessOutput")]
		public string SuccessOutput { get; set; }
	}

	#endregion

}