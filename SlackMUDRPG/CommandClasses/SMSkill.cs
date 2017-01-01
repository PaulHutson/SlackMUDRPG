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

        public string UseSkill(SMCharacter smc, string targetType = null, string targetName = null)
        {
            // Loop around the steps
            foreach(SMSkillStep smss in this.SkillSteps)
            {
                switch (smss.StepType)
                {
                    case "Object":
                        if (!StepRequiredObject(smc, smss.StepRequiredObject, smss.RequiredObjectAmount))
                            return smss.FailureOutput;
                        break;
                    case "Target":
                        if (!StepRequiredTarget(smc, targetType, targetName, smss.StepRequiredObject, smss.RequiredObjectAmount))
                            return smss.FailureOutput;
                        break;
                    case "Hit":
                        if (!StepHit(smc, this.BaseStat, targetType, targetName, smss.StepRequiredObject, smss.RequiredObjectAmount))
                            return smss.FailureOutput;
                        break;
                    case "SkillLevelIncrease":
                        break;
                    case "Repeat":
                        break;
                }
            }

            return "";
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

        private bool StepHit(SMCharacter smc, string BaseStat, string targetType, string targetName, string requiredTargetObjectType, int requiredTargetObjectAmount)
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
    /// Hit: Hits something with the object that they've used.
    /// Repeat: Repeats everything until stopped (i.e. will continue to do something over and over).
    /// SkillLevelIncrease: Checks to see if the skill level should increase, as things get harder then the skills become harder to get better at.
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