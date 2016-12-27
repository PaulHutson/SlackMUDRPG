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
            string a = CommandsClasses.SlackMud.Login("THISWILLFAIL");
            string  expected = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
                    expected += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            Assert.AreEqual(a, expected, false);
        }

        [TestMethod]
        public void Test_Login_Char()
        {
            string a = CommandsClasses.SlackMud.Login("123");
            string expected = "You must create a character, to do so, use the command /sm CreateCharacter FIRSTNAME,LASTNAME,SEX,AGE\n";
            expected += "i.e. /sm CreateCharacter Paul,Hutson,m,34";
            Assert.AreEqual(a, expected, false);
        }
    }
}
