using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
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
        
        public void UseSkill(SMCharacter smc, bool beginSkillUse = true, string targetType = null, string targetID = null)
        {
            // Set the character activity
            if (beginSkillUse)
            {
                smc.CurrentActivity = this.ActivityType;
            }

            // Loop around the steps
            foreach(SMSkillStep smss in this.SkillSteps)
            {
                if (smc.CurrentActivity == this.ActivityType)
                {
                    switch (smss.StepType)
                    {
                        case "Object":
                            if (!StepRequiredObject(smc, smss.StepRequiredObject, smss.RequiredObjectAmount))
                                smc.sendMessageToPlayer(smss.FailureOutput);
                            break;
                        case "Target":
                            if (!StepRequiredTarget(smc, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
                                smc.sendMessageToPlayer(smss.FailureOutput);
                            break;
                        case "Hit":
                            if (!StepHit(smss, smc, this.BaseStat, targetType, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
                            {
                                smc.sendMessageToPlayer(smss.FailureOutput);
                                SkillIncrease(smc, this.BaseStat, false);
                            }
                            else
                            {
                                smc.CurrentActivity = null;
                                SkillIncrease(smc, this.BaseStat, true);
                            }
                            break;
                        case "Information":
                            smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, targetType, targetID));
                            break;
                        case "Pause":
                            System.Threading.Thread.Sleep(smss.RequiredObjectAmount * 1000);
                            break;
                        case "Repeat":
                            this.UseSkill(smc, false, targetType, targetID);
                            break;
                    }
                }
            }
        }

        #region "Skill Step Methods"

        private bool StepRequiredObject(SMCharacter smc, string requiredObjectType, int requiredObjectAmount)
        {
            // First look at what is in the players hand.
            // TODO use inventory system to do this!

            // Check if, in the list, they have the required item / amount
            // TODO linq to see if they have enough of the item (i.e. do a count on the list).
            //      if they don't have enough return a false.

            return true;
        }

        private bool StepRequiredTarget(SMCharacter smc, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
        {
            // Check the type of target they've specified is correct...
            // Get the target item
            SMItem targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemId == targetID);

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

            return true;
        }

        private bool StepHit(SMSkillStep smss, SMCharacter smc, string baseStat, string targetType, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
        {
            // Get the object to hit the target with.
            // TODO Get the objects from the equipped items.
            SMItem charItemToUse = new SMItem(); // TODO REPLACE THIS WITH CORRECT ITEM.
            float charItembaseDamage = charItemToUse.BaseDamage; // TODO GET THE REAL VALUE OF THE DAMAGE DEALT!

            // Get the base attribute from the character
            int baseStatValue = smc.Attributes.GetBaseStatValue(baseStat);

            // Work out the damage multiplier based on attribute level (+/-)
            int baseStatRequiredAmount = this.Prerequisites.First(pr => pr.SkillStatName == baseStat).PreReqLevel;
            float positiveNegativeBaseStat = baseStatValue - baseStatRequiredAmount;
            int charLevelOfSkill = smc.Skills.First(skill => skill.SkillName == this.SkillName).SkillLevel;
            float damageMultiplier = (positiveNegativeBaseStat + charLevelOfSkill) / 100;

            // Get the target toughness and HP
            int targetToughness, targetHP;
            SMItem targetItem; // for use when it's a target item
            SMCharacter targetChar; // for use when it's a target character
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
            }
            else // Assume it's an item
            {
                // Get the target item
                targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemId == targetID);

                // Get the toughness and the hitpoints
                targetToughness = targetItem.Toughness;
                targetHP = targetItem.HitPoints;
                targetName = targetItem.ItemName;
                destroyedObjectType = targetItem.DestroyedOutput;
            }

            // Hit the target
            // calculate the actual damage amount
            float actualDamageAmount = (charItembaseDamage * (1 + damageMultiplier)) - targetToughness;

            // Reduce the targets HP
            int newTargetHP = targetHP - (int)actualDamageAmount;
            // if the targets HP reaches 0 it has "died" or been "destroyed"
            if (newTargetHP<0)
            {
                // Replace the object with the alterobject type
                if (targetType == "Character")
                {
                    // TODO Add "Die" method to the characer
                }
                else // Assume it's an item
                {
                    // Todo add the new item to the room.
                    // Get the target item
                    targetItem = smc.GetRoom().RoomItems.FirstOrDefault(ri => ri.ItemId == targetID);
                    string[] destroyedObjectInfo = targetItem.DestroyedOutput.Split(',');
                    int numberOfObjectsToCreate = int.Parse(destroyedObjectInfo[0]);
                    while (numberOfObjectsToCreate > 0)
                    {
                        // Reduce the number of items waiting to be created
                        numberOfObjectsToCreate--;

                        // Add the item to the room
                        smc.GetRoom().AddItem(targetItem.GetDestroyedItem());
                        
                        // Remove the destroyed item from the room.
                        // TODO Need to remove the item from the room.
                    }
                }
                smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, targetType, targetID));
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
            successOutput.Replace("{TARGETNAME}", targetName);
            successOutput.Replace("{CHARNAME}", smc.GetFullName());
            successOutput.Replace("{Object.DestroyedOutput}", objectDestroyedName);

            // Parse the string to change the text elements around as needed

            return "";
        }

        private void SkillIncrease(SMCharacter smc, string BaseStat, bool skillSuccess)
        {
            // Skill Increase
            // Work out the change of skill increase based on level (also include a small bonus for skill success)

            // Random chance to see if someone achieves the skill increase change
            //      Increase the skill by one.
            //      Send message to the player.

            // Attribute Increase
            // TODO add an attribute increase method check to SMAttributes
        }

        #endregion

    }

    public class SMCharacterSkill : SMSkill
    {
        [JsonProperty("SkillLevel")]
        public int SkillLevel { get; set; }
    }

    public class SMSkillPrerequisite
    {
        [JsonProperty("IsSkill")]
        public bool IsSkill { get; set; }

        [JsonProperty("SkillStatName")]
        public string SkillStatName { get; set; }

        [JsonProperty("PreReqLevel")]
        public int PreReqLevel { get; set; }
    }

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

}