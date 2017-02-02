using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SlackMUDRPG.CommandClasses
{
	public class SMPulse
	{
		[JsonProperty("TimeOfDay")]
		public int TimeOfDay { get; set; }

		[JsonProperty("LastUpdate")]
		public string LastUpdate { get; set; }

		[JsonProperty("Month")]
		public string Month { get; set; }

		[JsonProperty("MonthLastChange")]
		public string MonthLastChange { get; set; }

		[JsonProperty("Season")]
		public string Season { get; set; }

		[JsonProperty("Weather")]
		public string Weather { get; set; }

		public void Initite()
		{
			// Get the current date time


			// Load: 
			// Notices
			// Weather
			// DayInformation
			// MonthInformation


			// Load the last world data

			// Calculate what the in game date should be
			

			// Begin the Pulses

		}

		public void Pulse()
		{
			// Work out the start time

			// Update in game time

			// Randonly decide whether the weather effect will change.
			

			// Work out the end time
			// If less than two minutes
			// Let the thread sleep for the remainder of the time
			Thread.Sleep(60);

			// Recall the pulse.
			Pulse();
		}
	}

	public class SMNotices
	{
		[JsonProperty("NoticeType")]
		public string NoticeType { get; set; }

		[JsonProperty("Notice")]
		public string Notice { get; set; }
	}

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

	public class SMDay
	{
		[JsonProperty("LengthOfDay")]
		public int LengthOfDay { get; set; }

		[JsonProperty("LengthOfNight")]
		public int LengthOfNight { get; set; }
	}

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