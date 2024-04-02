using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using ProjectEarthServerAPI.Models;
using ProjectEarthServerAPI.Util;
using ProjectEarthServerAPI.Models.Features;
using ProjectEarthServerAPI.Models.Player;
using Serilog;
using Uma.Uuid;
using Serilog.Events;

namespace ProjectEarthServerAPI
{

	public class Program
    {

		public static void Main(string[] args)
		{
			TypeDescriptor.AddAttributes(typeof(Uuid), new TypeConverterAttribute(typeof(StringToUuidConv)));

			// Init Logging
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.File("logs/debug.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8338607, outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
				.MinimumLevel.Override("ProjectEarthServerAPI.Authentication", LogEventLevel.Warning)
				.CreateLogger();

			//Initialize state singleton from config
			StateSingleton.Instance.config = ServerConfig.getFromFile();
			StateSingleton.Instance.catalog = CatalogResponse.FromFiles(StateSingleton.Instance.config.itemsFolderLocation, StateSingleton.Instance.config.efficiencyCategoriesFolderLocation);
			StateSingleton.Instance.recipes = Recipes.FromFile(StateSingleton.Instance.config.recipesFileLocation);
			StateSingleton.Instance.settings = SettingsResponse.FromFile(StateSingleton.Instance.config.settingsFileLocation);
			StateSingleton.Instance.challengeStorage = ChallengeStorage.FromFiles(StateSingleton.Instance.config.challengeStorageFolderLocation);
			StateSingleton.Instance.productCatalog = ProductCatalogResponse.FromFile(StateSingleton.Instance.config.productCatalogFileLocation);
			StateSingleton.Instance.tappableData = TappableUtils.loadAllTappableSets();
			StateSingleton.Instance.activeTappables = new();
			StateSingleton.Instance.levels = ProfileUtils.readLevelDictionary();
			StateSingleton.Instance.shopItems = ShopUtils.readShopItemDictionary();

			//Start api
			CreateHostBuilder(args).Build().Run();

			Log.Information("Server started!");
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

}
