using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandsClasses
{
    public static class SlackMud
    {
        public static string Login(string userID)
        {
            string returnString = "";

            string path = @"~\JSON\Characters\Char" + userID + ".json";
            if (!File.Exists(path))
            {
                returnString = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
                returnString += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            }
            else
            {
                returnString = GetCharacter(userID);
            }

            return returnString;
        }

        public static string GetCharacter(string userID)
        {
            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");
            if (File.Exists(path))
            {
                // In progress
                SMCharacter SMChar = new SMCharacter();
                return "Loaded the character";
            }
            else
            {
                return "You do not have a character yet, you need to create one...";
            }
        }

        public static string CreateCharacter(string userID, string firstName, string lastName, int age, char sexIn)
        {
            SMCharacter SMChar = new SMCharacter();
            SMChar.FirstName = firstName;
            SMChar.LastName = lastName;
            SMChar.LastLogindate = DateTime.Now;
            SMChar.LastInteractionDate = DateTime.Now;
            SMChar.RoomLocation = "0";
            SMChar.PKFlag = false;
            SMChar.Sex = sexIn;

            var SMCharJSON = JsonConvert.SerializeObject(SMChar);

            string path = HttpContext.Current.Server.MapPath("~/JSON/Characters/Char" + userID + ".json");
            if (!File.Exists(path))
            {
                //File.Create(path);
                //TextWriter tw = new StreamWriter(path);
                //tw.WriteLine(SMCharJSON);
                //tw.Close();

                using (StreamWriter w = new StreamWriter(path, true))
                {
                    w.WriteLine(SMCharJSON); // Write the text
                }

                return "Character Created";
            }
            else if (File.Exists(path))
            {
                return "You already have a character, you can not create another.";
            }

            return "Error";
        }
    }
}