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

        public void UseSkill(SMCharacter smc, bool beginSkillUse = true, string targetType = null, string targetName = null, string targetID = null)
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
                            if (!StepRequiredTarget(smc, targetType, targetName, smss.StepRequiredObject, smss.RequiredObjectAmount))
                                smc.sendMessageToPlayer(smss.FailureOutput);
                            break;
                        case "Hit":
                            if (!StepHit(smc, this.BaseStat, targetType, targetName, smss.StepRequiredObject, smss.RequiredObjectAmount, targetID))
                            {
                                smc.sendMessageToPlayer(smss.FailureOutput);
                                SkillIncrease(smc, this.BaseStat, false);
                            }
                            else
                            {
                                smc.CurrentActivity = null;
                                smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, targetType, targetName, targetID));
                                SkillIncrease(smc, this.BaseStat, true);
                            }
                            break;
                        case "Information":
                            smc.GetRoom().Announce(SuccessOutputParse(smss.SuccessOutput, smc, targetType, targetName, targetID));
                            break;
                        case "Pause":
                            System.Threading.Thread.Sleep(smss.RequiredObjectAmount * 1000);
                            break;
                        case "Repeat":
                            this.UseSkill(smc, false, targetType, targetName, targetID);
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

        private bool StepRequiredTarget(SMCharacter smc, string targetType, string targetName, string requiredTargetObjectType, int requiredTargetObjectAmount)
        {
            // Check the type of target they've specified is correct...
            // ... if the step required object type is null then as long as there is a target all is good.
            // return false if something isn't there.

            return true;
        }

        private bool StepHit(SMCharacter smc, string BaseStat, string targetType, string targetName, string requiredTargetObjectType, int requiredTargetObjectAmount, string targetID)
        {
            // Get the object to hit the target with.

            // Get the base attribute from the character to see how much they get.

            // Work out the damage multiplier based on attribute level (+/-)

            // Hit the target 
            // Reduce the targets HP
            //      if the targets HP reaches 0 it has "died" or been "destroyed"
            //          Replace the object with the alterobject type

            // Check to see if we should reduce the objects HP (wear and tear)
            // reduce the objects HP
            //      if the objects HP reaches 0 then the item is "broken" and the player needs to be told via a message.
            //      Stop any repeat actions against the character happening.
            
            return true;
        }

        private string SuccessOutputParse(string successOutput, SMCharacter smc, string targetType, string targetName, string targetID)
        {
            // Get the target type details

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
        public string PreReqLevel { get; set; }
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