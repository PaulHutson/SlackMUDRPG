using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMAttributes
    {
        [JsonProperty("str")]
        public int Strength { get; set; }

        [JsonProperty("int")]
        public int Intelligence { get; set; }

        [JsonProperty("chr")]
        public int Charisma { get; set; }

        [JsonProperty("dex")]
        public int Dexterity { get; set; }

        [JsonProperty("wp")]
        public int WillPower { get; set; }

        [JsonProperty("SocialStanding")]
        public int SocialStanding { get; set; }

        /// <summary>
        /// Increases an attribute type by 1
        /// </summary>
        /// <param name="attributeType">The attribute type in short form</param>
        /// <returns>The increased amount, if a 0 is returned then an error has occurred</returns>
        public int IncreaseAttribute(string attributeType)
        {
            // Set the return variable in advance
            int updatedAttributeValue = 0;

            // Note that the max attribute in here is currently 20, we could change that for each item really...
            int maxAttributeValue = 20;
            switch (attributeType)
            {
                case "str":
                    this.Strength += 1;
                    if (this.Strength > maxAttributeValue)
                    {
                        this.Strength = maxAttributeValue;
                    };
                    updatedAttributeValue = this.Strength;
                    break;
                case "int":
                    this.Intelligence += 1;
                    if (this.Intelligence > maxAttributeValue)
                    {
                        this.Intelligence = maxAttributeValue;
                    };
                    updatedAttributeValue = this.Intelligence;
                    break;
                case "chr":
                    this.Charisma += 1;
                    if (this.Charisma > maxAttributeValue)
                    {
                        this.Charisma = maxAttributeValue;
                    };
                    updatedAttributeValue = this.Charisma;
                    break;
                case "dex":
                    this.Dexterity += 1;
                    if (this.Dexterity > maxAttributeValue)
                    {
                        this.Dexterity = maxAttributeValue;
                    };
                    updatedAttributeValue = this.Dexterity;
                    break;
                case "wp":
                    this.WillPower += 1;
                    if (this.WillPower > maxAttributeValue)
                    {
                        this.WillPower = maxAttributeValue;
                    };
                    updatedAttributeValue = this.WillPower;
                    break;
                case "SocialStanding":
                    this.SocialStanding += 1;
                    if (this.SocialStanding > maxAttributeValue)
                    {
                        this.SocialStanding = maxAttributeValue;
                    };
                    updatedAttributeValue = this.SocialStanding;
                    break;
            }

            return updatedAttributeValue;
        }
    }
}