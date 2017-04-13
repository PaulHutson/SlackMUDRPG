using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    /// <summary>
    /// This class is a basic implementation of a single type currency system (used for simplicity).
    /// </summary>
    public class SMCurrency
    {
        /// <summary>
        /// The Amount of the Currency that is held.
        /// </summary>
        public int AmountOfCurrency { get; set; }

        /// <summary>
        /// Add some currency to the account
        /// </summary>
        /// <param name="amountToAdd">The amount of currency to add.</param>
        public void AddCurrency(int amountToAdd)
        {
            AmountOfCurrency += amountToAdd;
        }

        /// <summary>
        /// Remove some currency from the account
        /// </summary>
        /// <param name="amountToRemove">The amount of currency to remove.</param>
        public void RemoveCurrency(int amountToRemove)
        {
            AmountOfCurrency -= amountToRemove;
        }

        /// <summary>
        /// Check that the player has a certain amount of currency
        /// </summary>
        /// <param name="amountToCheck"></param>
        /// <returns></returns>
        public bool CheckCurrency(int amountToCheck)
        {
            if (amountToCheck >= AmountOfCurrency) {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the currency amount as a string for viewing.
        /// </summary>
        /// <returns>A string showing the currency amount + the currency type string</returns>
        public string GetCurrencyAmount()
        {
            string returnString = "";
            string[] currencyTypeString = ConfigurationManager.AppSettings.Get("CurrencyType").Split('|');

            if (AmountOfCurrency > 1)
            {
                returnString = AmountOfCurrency + " " + currencyTypeString[1];
            }
            else
            {
                returnString = AmountOfCurrency + " " + currencyTypeString[0];
            }
            return returnString;
        }
    }
}