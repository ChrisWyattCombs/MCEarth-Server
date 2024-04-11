﻿using System;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace ProjectEarthServerAPI.Util
{
	public class GenericUtils
	{
		private static uint _streamVersion = 0;

		public static T ParseJsonFile<T>(string playerId, string fileNameWithoutJsonExtension) where T : new()
		{
			var filepath = $"./data/players/{playerId}/{fileNameWithoutJsonExtension}.json";
			if (!File.Exists(filepath))
			{
				if (!Directory.Exists($"./data/players/{playerId}"))
					Directory.CreateDirectory($"./data/players/{playerId}");

				SetupJsonFile<T>(playerId, filepath); // Generic setup for each player specific json type
			}

			var invjson = File.ReadAllText(filepath);
			var parsedobj = JsonConvert.DeserializeObject<T>(invjson);
			return parsedobj;
		}

		private static bool SetupJsonFile<T>(string playerId, string filepath) where T : new()
		{
			try
			{
				Log.Information($"[{playerId}]: Creating default json with Type: {typeof(T)}.");
				var obj = new T(); // TODO: Implement Default Values for each player property/json we store for them

				File.WriteAllText(filepath, JsonConvert.SerializeObject(obj));
				return true;
			}
			catch (Exception ex)
			{
				Log.Error($"[{playerId}]: Creating default json failed! Type: {typeof(T)}");
				Log.Debug($"Exception: {ex}");
				return false;
			}
		}

		public static bool WriteJsonFile<T>(string playerId, T objToWrite, string fileNameWithoutJsonExtension)
		{
			try
			{
				var filepath = $"./data/players/{playerId}/{fileNameWithoutJsonExtension}.json"; // Path should exist, as you cant really write to the file before reading it first

				File.WriteAllText(filepath, JsonConvert.SerializeObject(objToWrite));

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static uint GetNextStreamVersion()
		{
			_streamVersion++;
			return _streamVersion;
		}
	}
}
