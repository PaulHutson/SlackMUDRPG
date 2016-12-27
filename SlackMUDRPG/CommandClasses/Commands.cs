using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public static class Commands
    {
        static String[] helloList = { "Hello", "Greetings", "Bonjour", "A very good day to you all", "Hey", "Howdy", "Well Hello!", "Yo", "Hi", "Beunas dias", "Good day", "Hi-ya", "How goes it?", "Howdy-do", "Shalom", "Whazzzzuppp?", "G'Day" };
        static Random rnd = new Random();

        public static string HelloSlack(string extraItemsIn = "")
        {
            int r = rnd.Next(helloList.Count());

            // Create actual name links
            // String format: <a class="internal_member_link" data-member-name="kerryb" target="/team/kerryb" href="/team/kerryb">@kerryb</a>
            String[] splitString = extraItemsIn.Split(' ');
            string extraItemsList = "";
            foreach (string item in splitString)
            {
                if ((item.Substring(0, 1) == "@") || (item.Substring(0, 1) == "#"))
                {
                    extraItemsList += "<" + item + "> ";
                }
                else
                {
                    extraItemsList += item + " ";
                }
            }

            return helloList[r] + " " + extraItemsList;
        }
    }
}