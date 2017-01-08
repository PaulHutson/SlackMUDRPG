using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SlackMUDRPG.Tests
{
    [TestClass]
    public class TestCharacterCommands
    {
        [TestMethod]
        public void Test_Login_NoChar()
        {
            string a = ""; // CommandClasses.SlackMud.Login("THISWILLFAIL");
            string  expected = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
                    expected += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_Login_Char()
        {
            string a = ""; // CommandClasses.SlackMud.Login("123");
            string expected = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
            expected += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_GetChar()
        {
            string a = ""; // CommandClasses.SlackMud.Login("123");
            string expected = "Welcome back Paul";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_GetCharNewCharacter()
        {
            string a = ""; // CommandClasses.SlackMud.Login("123", true);
            string expected = "Welcome to SlackMud!\n";
            expected += "We've created your character in the magical world of Arrelvia!";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_GetChar_NoChar()
        {
            string a = ""; // CommandClasses.SlackMud.Login("WONTWORK");
            string expected = "You do not have a character yet, you need to create one...";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_CreateChar()
        {
            string a = ""; // CommandClasses.SlackMud.CreateCharacter("123", "Paul", "Hutson", 34, 'm');
            string expected = "Welcome back Paul";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_GetLocation()
        {
            string a = ""; // CommandClasses.SlackMud.GetLocationDetails("1", "123");
            string expected = "Welcome back Paul";
            Assert.AreEqual(a, expected, false);
        }
        
    }
}
