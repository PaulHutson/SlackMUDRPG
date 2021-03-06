﻿using Newtonsoft.Json;
using SlackMUDRPG.Utility;
using SlackMUDRPG.Utility.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	/// <summary>
	/// The pulse class that contains information about the current information.
	/// </summary>
	public class SMPulse
	{
		[JsonProperty("TimeOfDay")]
		public int TimeOfDay { get; set; }

		[JsonProperty("LastUpdate")]
		public int LastUpdate { get; set; }

		[JsonProperty("Month")]
		public string Month { get; set; }

		[JsonProperty("MonthLastChange")]
		public string MonthLastChange { get; set; }

		[JsonProperty("Season")]
		public string Season { get; set; }

		[JsonProperty("Weather")]
		public string Weather { get; set; }

		public void Initiate()
		{
			// Get the current date time
			int currentUnixTime = Utility.Utils.GetUnixTime();

			// Load: 
			// Notices
			List<SMNotices> smn = new List<SMNotices>();
			string getNotices = Utility.Utils.GetFileJSON("Pulse", "Notices");
			if (getNotices != null)
			{
				smn = JsonConvert.DeserializeObject<List<SMNotices>>(getNotices);
			}
			HttpContext.Current.Application["SMNotices"] = smn;
			
			// DayInformation
			SMDay smd = new SMDay();
			string getDayNightDetails = Utility.Utils.GetFileJSON("Pulse", "Day");
			if (getDayNightDetails != null)
			{
				smd = JsonConvert.DeserializeObject<SMDay>(getDayNightDetails);
			}
			HttpContext.Current.Application["SMDay"] = smd;

			HttpContext.Current.Application["CurrentDay"] = 1;
			this.LastUpdate = currentUnixTime;
			this.TimeOfDay = 150;
			HttpContext.Current.Application["DayOrNight"] = "D";

			// TODO Load More Data
			// Seasons
			// Weather
			// MonthInformation
			// Load the last world data
						
			// Begin the Pulses
			Pulse();
		}

		/// <summary>
		/// Pulse the world
		/// </summary>
		public void Pulse()
		{
			// Work out the start time
			int currentUnixTime = Utility.Utils.GetUnixTime();

			// Get ingame date time info
			SMDay smd = new SMDay();
			smd = (SMDay)HttpContext.Current.Application["SMDay"];
			int totalDayNightHours = 24 / (smd.LengthOfDay + smd.LengthOfNight);
			int numberOfMinutesPerGameDay = totalDayNightHours * 60;
			int numberOfMinutesPerDayTime = (numberOfMinutesPerGameDay / (smd.LengthOfDay + smd.LengthOfNight)) * smd.LengthOfDay;
			int numberOfMinutesPerNightTime = (numberOfMinutesPerGameDay / (smd.LengthOfDay + smd.LengthOfNight)) * smd.LengthOfNight;

			List<SMNotices> smn = (List<SMNotices>)HttpContext.Current.Application["SMNotices"];

			// Update in game time
			int newTime = this.TimeOfDay + (Utility.Utils.GetDifferenceBetweenUnixTimestampsInMinutes(this.LastUpdate, currentUnixTime)*totalDayNightHours);
			string dayOrNight = HttpContext.Current.Application["DayOrNight"].ToString();
			if (newTime > numberOfMinutesPerGameDay)
			{
				string noticeS = smn.FirstOrDefault(notice => notice.NoticeType == "DayStart").Notice ?? "A new day begins...";
				newTime = newTime - numberOfMinutesPerGameDay;
				new SlackMud().BroadcastMessage(ResponseFormatterFactory.Get().Italic(noticeS));
				HttpContext.Current.Application["DayOrNight"] = "D";
			}
			else if ((newTime > numberOfMinutesPerDayTime) && (HttpContext.Current.Application["DayOrNight"].ToString() != "N"))
			{
				string noticeS = smn.FirstOrDefault(notice => notice.NoticeType == "NightStart").Notice ?? "Night falls";
				new SlackMud().BroadcastMessage(ResponseFormatterFactory.Get().Italic(noticeS));
				HttpContext.Current.Application["DayOrNight"] = "N";
			}

			// Update the variables
			this.LastUpdate = currentUnixTime;
			this.TimeOfDay = newTime;

			// NPC Cycles in the game world
			List<SMNPC> smnpcl = new List<SMNPC>();
			smnpcl = (List<SMNPC>)HttpContext.Current.Application["SMNPCs"];

			if (smnpcl != null)
			{
				smnpcl = smnpcl.FindAll(npc => npc.NPCResponses.Count(response => response.ResponseType == "Pulse") > 0);
				if (smnpcl != null)
				{
					foreach (SMNPC npc in smnpcl)
					{
						npc.RespondToAction("Pulse", null);
					}
				}
			}

			// Spawns
			// Find all rooms that are in memory that could have a spawn
			List<SMRoom> smrl = new List<SMRoom>();
			smrl = (List<SMRoom>)HttpContext.Current.Application["SMRooms"];

			// If there are any rooms in memory...
			if (smrl != null)
			{
                // ... find the rooms that have any possible NPC spawns.
                List<SMRoom> roomsWithNPCSpawns = smrl.FindAll(room => room.NPCSpawns != null);
				if (roomsWithNPCSpawns != null)
				{
					// loop around the rooms
					foreach (SMRoom smr in roomsWithNPCSpawns)
					{
						// Check whether there are any spawns.
						smr.Spawn();
					}
				}

                // ... find the rooms that have any possible Item spawns.
                List<SMRoom> roomsWithItemSpawns = smrl.FindAll(room => room.ItemSpawns != null);
                if (roomsWithItemSpawns != null)
                {
                    // loop around the rooms
                    foreach (SMRoom smr in roomsWithItemSpawns)
                    {
                        // Check whether there are any spawns.
                        smr.ItemSpawn();
                    }
                }
            }

			// TODO Randonly decide whether the weather effect will change.
			

            // Find all players
            List<SMCharacter> lsmc = (List<SMCharacter>)HttpContext.Current.Application["SMCharacters"];
            foreach (SMCharacter c in lsmc.ToList())
            {
                // Remove any attribute effects that have expired
                if (c.Attributes.Effects != null)
                {
                    int currentTime = Utility.Utils.GetUnixTime();
                    c.Attributes.Effects.RemoveAll(e => e.EffectLength <= currentTime);
                }

                // Hurt.
                if (c.Attributes.HitPoints < c.Attributes.MaxHitPoints)
                {
                    Random rNumber = new Random();
                    double rDouble = (rNumber.NextDouble() * 100);

                    if (rDouble <= 5)
                    {
                        c.Attributes.HitPoints++;
                        if (c.Attributes.Effects != null)
                        {
                            List<SMEffect> smel = c.Attributes.Effects.FindAll(e => e.EffectType.ToLower() == "Healing Rate".ToLower());

                            if (smel != null)
                            {
                                int increasedHealingRate = 0;

                                foreach (SMEffect sme in smel)
                                {
                                    increasedHealingRate += int.Parse(sme.AdditionalData);
                                }

                                c.Attributes.HitPoints = c.Attributes.HitPoints + increasedHealingRate;
                            }
                        }
                        c.SaveToApplication();
                        c.SaveToFile();
                    }
                }

                // ... remove any daily quests that have expired.
                if (c.QuestLog != null)
                {
                    c.QuestLog.RemoveAll(qs => ((qs.Completed == true) && (qs.Daily == true) && (Utility.Utils.GetDifferenceBetweenUnixTimestamps(qs.LastDateUpdated, Utility.Utils.GetUnixTime())>86400)));
                    c.SaveToApplication();
                    c.SaveToFile();
                }
            }

            // Work out the end time
            // If less than two minutes
            // Let the thread sleep for the remainder of the time
            int timeToWait = Utility.Utils.GetDifferenceBetweenUnixTimestamps(currentUnixTime, Utility.Utils.GetUnixTime());

			// If the time between the pulses is less than two minutes...
			if (timeToWait < (120 * 1000))
			{
				// ... send the thread to sleep
				Thread.Sleep((120 * 1000) - timeToWait);
			}
			
			// Recall the pulse.
			Pulse();
		}
	}
	
	/// <summary>
	/// Notice types (many different notices can be held here).
	/// </summary>
	public class SMNotices
	{
		[JsonProperty("NoticeType")]
		public string NoticeType { get; set; }

		[JsonProperty("Notice")]
		public string Notice { get; set; }
	}

	/// <summary>
	/// Weather types that can be loaded.
	/// </summary>
	public class SMWeather
	{
		[JsonProperty("WeatherType")]
		public string WeatherType { get; set; }

		[JsonProperty("AllowedTimes")]
		public string AllowedTimes { get; set; }

		[JsonProperty("EffectVerb")]
		public string EffectVerb { get; set; }

		[JsonProperty("Effect")]
		public string Effect { get; set; }

		[JsonProperty("WeatherEffectStart")]
		public string WeatherEffectStart { get; set; }

		[JsonProperty("WeatherEffectStop")]
		public string WeatherEffectStop { get; set; }
	}

	/// <summary>
	/// Day lengths (day and night)
	/// </summary>
	public class SMDay
	{
		[JsonProperty("LengthOfDay")]
		public int LengthOfDay { get; set; }

		[JsonProperty("LengthOfNight")]
		public int LengthOfNight { get; set; }
	}

	/// <summary>
	/// Months in the game and the various seasons
	/// </summary>
	public class SMMonth
	{
		[JsonProperty("Name")]
		public int Name { get; set; }

		[JsonProperty("Season")]
		public int Season { get; set; }

		[JsonProperty("Length")]
		public int Length { get; set; }
	}
}