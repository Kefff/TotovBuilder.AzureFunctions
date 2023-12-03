using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TotovBuilder.AzureFunctions.Abstractions.Configuration;
using TotovBuilder.AzureFunctions.Functions;
using Xunit;

namespace TotovBuilder.AzureFunctions.Test.Functions
{
    /// <summary>
    /// Represents tests on the <see cref="GenerateWebsiteData"/> class.
    /// </summary>
    public class GenerateWebsiteDataTests
    {
        [Fact]
        public async Task Run_ShouldGenerateWebsiteData()
        {
            // Arrange
            ScheduleTrigger scheduleTrigger = new ScheduleTrigger()
            {
                IsPastDue = false,
                ScheduleStatus = new ScheduleStatus()
                {
                    Last = new DateTime(2023, 12, 1, 8, 0, 0),
                    LastUpdated = new DateTime(2023, 12, 1, 8, 30, 0),
                    Next = new DateTime(2023, 12, 2, 8, 0, 0)
                }
            };

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {
                    "items",
                    @"[
	{
		""baseItemId"": ""58948c8e86f77409493f7266"",
		""caliber"": ""Caliber9x19PARA"",
		""defaultPresetId"": null,
		""ergonomics"": 40,
		""fireModes"": [
			""SingleFire"",
			""FullAuto""
		],
		""fireRate"": 850,
		""horizontalRecoil"": 299,
		""minuteOfAngle"": 6.19,
		""modSlots"": [
			{
				""compatibleItemIds"": [
					""5efb0da7a29a85116f6ea05f"",
					""5c3df7d588a4501f290594e5"",
					""58864a4f2459770fcc257101"",
					""56d59d3ad2720bdb418b4577"",
					""5c925fa22e221601da359b7b"",
					""5a3c16fe86f77452b62de32a"",
					""5efb0e16aeb21837e749c7ff"",
					""5c0d56a986f774449d5de529""
				],
				""maxStackableAmount"": 1,
				""name"": ""chamber0"",
				""required"": false
			},
			{
				""compatibleItemIds"": [
					""55d4b9964bdc2d1d4e8b456e"",
					""571659bb2459771fb2755a12"",
					""602e71bd53a60014f9705bfa"",
					""6113c3586c780c1e710c90bc"",
					""6113cc78d3a39d50044c065a"",
					""6113cce3d92c473c770200c7"",
					""5cc9bcaed7f00c011c04e179"",
					""5bb20e18d4351e00320205d5"",
					""5bb20e0ed4351e3bac1212dc"",
					""6193dcd0f8ee7e52e4210a28"",
					""5d025cc1d7ad1a53845279ef"",
					""5c6d7b3d2e221600114c9b7d"",
					""57c55efc2459772d2c6271e7"",
					""57af48872459771f0b2ebf11"",
					""57c55f092459772d291a8463"",
					""57c55f112459772d28133310"",
					""57c55f172459772d27602381"",
					""5a339805c4a2826c6e06d73d"",
					""55802f5d4bdc2dac148b458f"",
					""5d15cf3bd7ad1a67e71518b2"",
					""59db3a1d86f77429e05b4e92"",
					""5fbcbd6c187fea44d52eda14"",
					""59db3acc86f7742a2c4ab912"",
					""59db3b0886f77429d72fb895"",
					""615d8faecabb9b7ad90f4d5d"",
					""5b07db875acfc40dc528a5f6"",
					""5894a51286f77426d13baf02"",
					""63f5feead259b42f0b4d6d0f""
				],
				""maxStackableAmount"": 1,
				""name"": ""mod_pistol_grip"",
				""required"": false
			},
			{
				""compatibleItemIds"": [
					""5c5db6742e2216000f1b2852"",
					""5c5db6552e2216001026119d"",
					""5894a05586f774094708ef75"",
					""5c5db6652e221600113fba51""
				],
				""maxStackableAmount"": 1,
				""name"": ""mod_magazine"",
				""required"": false
			},
			{
				""compatibleItemIds"": [
					""5894a5b586f77426d2590767""
				],
				""maxStackableAmount"": 1,
				""name"": ""mod_reciever"",
				""required"": false
			},
			{
				""compatibleItemIds"": [
					""58ac1bf086f77420ed183f9f"",
					""5894a13e86f7742405482982"",
					""5fbcc429900b1d5091531dd7"",
					""5fbcc437d724d907e2077d5c"",
					""5c5db6ee2e221600113fba54"",
					""5c5db6f82e2216003a0fe914""
				],
				""maxStackableAmount"": 1,
				""name"": ""mod_stock"",
				""required"": false
			},
			{
				""compatibleItemIds"": [
					""5c5db6b32e221600102611a0"",
					""58949edd86f77409483e16a9"",
					""58949fac86f77409483e16aa""
				],
				""maxStackableAmount"": 1,
				""name"": ""mod_charge"",
				""required"": false
			}
		],
		""verticalRecoil"": 51,
		""categoryId"": ""mainWeapon"",
		""conflictingItemIds"": [],
		""iconLink"": ""https://assets.tarkov.dev/58dffd4586f77408a27629b2-icon.webp"",
		""id"": ""58dffd4586f77408a27629b2"",
		""imageLink"": ""https://assets.tarkov.dev/58dffd4586f77408a27629b2-image.webp"",
		""maxStackableAmount"": 1,
		""marketLink"": ""https://tarkov.dev/item/sig-mpx-9x19-submachine-gun-silenced"",
		""name"": ""SIG MPX 9x19 submachine gun Silenced"",
		""prices"": [],
		""shortName"": ""MPX Silenced"",
		""weight"": 0.64,
		""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/SIG_MPX_9x19_submachine_gun""
	}
]"
                }
            };

            Mock<IConfigurationLoader> configurationLoader = new Mock<IConfigurationLoader>();
            configurationLoader.Setup(m => m.Load()).Verifiable();

            //GenerateWebsiteData function = new GenerateWebsiteData(configurationLoader.Object, );

            //// Act
            //await function.Run(scheduleTrigger);

            //// Assert
            //configurationLoader.Verify();
        }
    }
}
