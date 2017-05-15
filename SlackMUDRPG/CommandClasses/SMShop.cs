using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using SlackMUDRPG.Utility.Formatters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
    public class SMShop
    {
        [JsonProperty("ShopInventory")]
        public List<SMShopItem> ShopInventory { get; set; }

        private ResponseFormatter Formatter = null;

        public SMShop() {
            this.Formatter = ResponseFormatterFactory.Get();
        }

        public string GetInventory()
        {
            string returnString = "";
            string[] currencyTypeString = ConfigurationManager.AppSettings.Get("CurrencyType").Split('|');
            int shopNumber = 0;

            foreach (SMShopItem ssi in ShopInventory)
            {
                shopNumber++;

                string currencyPlural = currencyTypeString[0];
                if (ssi.Cost > 0)
                {
                    currencyPlural = currencyTypeString[1];
                }

                string amountAvailable = "";
                if (!ssi.UnlimitedAvailable)
                {
                    if (ssi.AmountForSale > 0)
                    {
                        amountAvailable = " (" + ssi.AmountForSale + " available)";
                    }
                    else
                    {
                        amountAvailable = " (none available)";
                    }
                }

                returnString += this.Formatter.ListItem(shopNumber + ". " + ssi.Item.ItemName + " - " + ssi.Cost + " " + currencyPlural + amountAvailable);
            }

            return returnString;
        }

        public bool BuyItem(string itemNumber, int numberToBuy, SMCharacter smc)
        {
            // Check that there is an item with that number
            SMShopItem ssi = ShopInventory.FirstOrDefault(item => item.ItemNumber == int.Parse(itemNumber));

            // If the item isn't null continue
            if (ssi != null)
            {
                // check the player has enough money to pay for the item(s)
                // Get the total value
                int totalValue = numberToBuy * ssi.Cost;

                if (smc.Currency.CheckCurrency(totalValue))
                {
                    // remove the money from the character
                    smc.Currency.RemoveCurrency(totalValue);

                    // add the item to the character
                    while (numberToBuy > 0)
                    {
                        numberToBuy--;
						smc.ReceiveItem(ssi.Item, true);
                    }

                    // Return that the item was bought.
                    return true;                    
                }
                else // They don't have enough to buy the item
                {
                    smc.sendMessageToPlayer(this.Formatter.Italic("You don't have enough money for that (needed " + totalValue + ", you have " + smc.Currency.AmountOfCurrency + ")"));
                    return false;
                }

            }
            else // Return that the item isn't valid...
            {
                smc.sendMessageToPlayer(this.Formatter.ListItem("The item you've specified isn't valid, please check and try again"));
                return false;
            }
        }
    }

    public class SMShopItem
    {
        [JsonProperty("ItemNumber")]
        public int ItemNumber { get; set; }

        [JsonProperty("Item")]
        public SMItem Item { get; set; }

        [JsonProperty("AmountForSale")]
        public int AmountForSale { get; set; }

        [JsonProperty("UnlimitedAvailable")]
        public bool UnlimitedAvailable { get; set; }

        [JsonProperty("Cost")]
        public int Cost { get; set; }
    }
}