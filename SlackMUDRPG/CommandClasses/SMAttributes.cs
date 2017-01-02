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

        [JsonProperty("ft")]
        public int Fortitude { get; set; }

        [JsonProperty("hp")]
        public int HitPoints { get; set; }

        [JsonProperty("maxhp")]
        public int MaxHitPoints { get; set; }

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
                case "ft":
                    this.Fortitude += 1;
                    if (this.Fortitude > maxAttributeValue)
                    {
                        this.Fortitude = maxAttributeValue;
                    };
                    updatedAttributeValue = this.Fortitude;
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

        /// <summary>
        /// Returns the base stat value for a specific attribute
        /// </summary>
        /// <param name="statShortName">Short attribute name i.e. STR, INT, DEX, WP, SS
        /// <returns></returns>
        public int GetBaseStatValue(string statShortName)
        {
            int statValue = 0;
            switch (statShortName)
            {
                case ("STR"):
                    statValue = this.Strength;
                    break;
                case ("INT"):
                    statValue = this.Intelligence;
                    break;
                case ("CHR"):
                    statValue = this.Charisma;
                    break;
                case ("DEX"):
                    statValue = this.Dexterity;
                    break;
                case ("WP"):
                    statValue = this.WillPower;
                    break;
                case ("FT"):
                    statValue = this.WillPower;
                    break;
                case ("SS"):
                    statValue = this.SocialStanding;
                    break;
            }
            return statValue;
        }

        #region "Derived Atributes"

        public int GetToughness()
        {
            // Base Toughness based on the fortitude of someone.
            return this.Fortitude / 10;
        }

        #endregion
    }
}