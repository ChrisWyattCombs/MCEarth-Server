﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProjectEarthServerAPI.Models.Features;
using ProjectEarthServerAPI.Util;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ProjectEarthServerAPI.Models;
using ProjectEarthServerAPI.Models.Player;
using Asp.Versioning;

namespace ProjectEarthServerAPI.Controllers
{
	// TODO: Not done. Rewards need inventory implementation, timers for crafting process, and recipeId -> recipe time checks
	[Authorize]
	public class CraftingController : Controller
	{
		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/{slot}/start")]
		public async Task<IActionResult> PostNewCraftingJob(int slot)
		{
			string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			using (StreamReader reader = new StreamReader(Request.Body))
			{
				var body = await reader.ReadToEndAsync();
				var req = JsonConvert.DeserializeObject<CraftingRequest>(body);
				var craftingJob = await Task.Run(() => CraftingUtils.StartCraftingJob(authtoken, slot, req));

				var updateResponse = new CraftingUpdates { updates = new Updates() };
				var nextStreamId = GenericUtils.GetNextStreamVersion();

				updateResponse.updates.crafting = nextStreamId;
				updateResponse.updates.inventory = nextStreamId;

				return Content(JsonConvert.SerializeObject(updateResponse), "application/json");
			}
		}


		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/finish/price")]
		public IActionResult GetCraftingPrice(int slot)
		{
			TimeSpan remainingTime = TimeSpan.Parse(Request.Query["remainingTime"]);
			var returnPrice = new CraftingPriceResponse {result = new CraftingPrice {cost = 1, discount = 0, validTime = remainingTime}, updates = new Updates()};

			return Content(JsonConvert.SerializeObject(returnPrice), "application/json");
		}

		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/{slot}/finish")]
		public async Task<IActionResult> PostCraftingFinish(int slot)
		{
			using (var reader = new StreamReader(Request.Body))
			{
				var body = await reader.ReadToEndAsync();
				var req = JsonConvert.DeserializeObject<FinishCraftingJobRequest>(body);

				var result = CraftingUtils.FinishCraftingJobNow(User.FindFirstValue(ClaimTypes.NameIdentifier), slot, req.expectedPurchasePrice);
				return Content(JsonConvert.SerializeObject(result), "application/json");
			}
		}

		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/{slot}")]
		public IActionResult GetCraftingStatus(int slot)
		{
			string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var craftingStatus = CraftingUtils.GetCraftingJobInfo(authtoken, slot);

			return Content(JsonConvert.SerializeObject(craftingStatus), "application/json");
			//return Accepted(Content(returnTokens, "application/json"));
		}

		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/{slot}/collectItems")]
		public IActionResult GetCollectCraftingItems(int slot)
		{
			string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var returnUpdates = CraftingUtils.FinishCraftingJob(authtoken, slot);

			return Content(JsonConvert.SerializeObject(returnUpdates), "application/json");
			//return Accepted(Content(returnTokens, "application/json"));
		}

		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/{slot}/stop")]
		public IActionResult GetStopCraftingJob(int slot)
		{
			string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var returnUpdates = CraftingUtils.CancelCraftingJob(authtoken, slot);

			//return Accepted();

			return Content(JsonConvert.SerializeObject(returnUpdates), "application/json");
			//return Accepted(Content(returnTokens, "application/json"));
		}

		[ApiVersion("1.1")]
		[Route("1/api/v{version:apiVersion}/crafting/{slot}/unlock")]
		public IActionResult GetUnlockCraftingSlot(int slot)
		{
			string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var returnUpdates = CraftingUtils.UnlockCraftingSlot(authtoken, slot);

			return Content(JsonConvert.SerializeObject(returnUpdates), "application/json");
		}
	}
}
