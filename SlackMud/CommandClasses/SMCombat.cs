using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public static class SMCombat
	{

		#region "Attack Openers"

		/// <summary>
		/// Attack a character
		/// </summary>
		/// <param name="attackingCharacter">The attacking character</param>
		/// <param name="targetItem">The target string</param>
		public static void Attack(SMCharacter attackingCharacter, SMCharacter targetCharacter)
		{
			// Check that the target has hitpoints
			if (targetCharacter.Attributes.HitPoints > 0)
			{
				// Work out if this is an NPC or not
				SMRoom room = attackingCharacter.GetRoom();

				SMNPC targetNPC = room.GetNPCs().FirstOrDefault(checkChar => checkChar.GetFullName() == targetCharacter.GetFullName());

                if (targetNPC != null)
                {
                    room.ProcessNPCReactions("PlayerCharacter.AttacksThem", attackingCharacter);
                }

                List<SMNPC> NPCsInRoom = targetCharacter.GetRoom().GetNPCs().FindAll(npc => npc.GetFullName() != attackingCharacter.GetFullName());

                if (NPCsInRoom.Count > 0)
                {
					room.ProcessNPCReactions("PlayerCharacter.Attack", attackingCharacter);
                }

				// Use the skill
				attackingCharacter.UseSkill(GetSkillToUse(attackingCharacter), targetCharacter.GetFullName(), true);
			}
			else // Report that the target is already dead...
			{
				attackingCharacter.sendMessageToPlayer(targetCharacter.GetFullName() + " is already dead!");
			}
		}

		/// <summary>
		/// Attack an item
		/// </summary>
		/// <param name="attackingCharacter">The attacking character</param>
		/// <param name="targetItem">The target item</param>
		public static void Attack(SMCharacter attackingCharacter, SMItem targetItem)
		{
			// Check that the target has hitpoints
			if (targetItem.HitPoints > 0)
			{
                // Use the skill
                attackingCharacter.UseSkill(GetSkillToUse(attackingCharacter), targetItem.ItemName, true);
            }
			else // Report that the target can not be found
			{
				attackingCharacter.sendMessageToPlayer(targetItem.ItemName + " can not be found");
			}
		}

		/// <summary>
		/// Attack a Creature
		/// </summary>
		/// <param name="attackingCharacter">The attacking character</param>
		/// <param name="targetCharacter">The target character</param>
		public static void Attack(SMCharacter attackingCharacter, SMCreature targetCreature)
		{
            // Check that the target has hitpoints
            if (targetCreature.Attributes.HitPoints > 0)
            {
                // Use the skill
                attackingCharacter.UseSkill(GetSkillToUse(attackingCharacter), targetCreature.CreatureName, true);
            }
            else // Report that the target can not be found
            {
                attackingCharacter.sendMessageToPlayer(targetCreature.CreatureName + " can not be found");
            }
        }


        public static string GetSkillToUse(SMCharacter attackingCharacter)
        {
            string skillToUse = "Brawl";
            if (!attackingCharacter.AreHandsEmpty())
            {
                // Get the equipped item from the character if any
                SMItem smi = attackingCharacter.GetEquippedItem();
                skillToUse = "Basic Attack";

                if ((smi != null) && (smi.RequiredSkills != null))
                {
                    // Check the player has the required skills
                    bool hasAllRequiredSkills = true;
                    bool isFirst = true;

                    foreach (SMRequiredSkill smrs in smi.RequiredSkills)
                    {
                        if (hasAllRequiredSkills)
                        {
                            hasAllRequiredSkills = attackingCharacter.HasRequiredSkill(smrs.SkillName, smrs.SkillLevel);
                            if (isFirst)
                            {
                                skillToUse = smrs.SkillName;
                            }
                        }
                    }

                    // If the player has all the required skills
                    if (!hasAllRequiredSkills)
                    {
                        // Tell the player they can't really wield that item.
                        attackingCharacter.sendMessageToPlayer("You are not skilled with the " + smi.ItemFamily + ", practicing gives you a chance to increase your skill");
                    }
                }
            }
            return skillToUse;
        }
		#endregion

		#region "Combat Helper Functions"



		#endregion

	}
}