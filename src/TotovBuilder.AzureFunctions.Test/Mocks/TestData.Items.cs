using System;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
        public static Item[] Items = new Item[]
        {
            new Ammunition()
            {
                AccuracyPercentageModifier = -0.05,
                ArmorDamagePercentage = 0.76,
                //ArmorPenetrations = , // TODO : OBTAIN FROM WIKI
                //Blinding = , // TODO : MISSING
                Caliber = "Caliber762x39",
                CategoryId = "ammunition",
                //ConflictingItemIds = , // TODO : MISSING
                //DurabilityBurnPercentageModifier = , // TODO : MISSING
                FleshDamage = 47,
                FragmentationChancePercentage = 0.05,
                HeavyBleedingPercentageChance = 0.1,
                IconLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-icon.jpg",
                Id = "601aa3d2b2bcb34913271e6d",
                ImageLink = "https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-image.jpg",
                LightBleedingPercentageChance = 0.1,
                MarketLink = "https://tarkov.dev/item/762x39mm-mai-ap",
                MaxStackableAmount = 60,
                Name = "7.62x39mm MAI AP",
                PenetrationPower = 58,
                Projectiles = 1,
                RecoilPercentageModifier = 0.10,
                ShortName = "MAI AP",
                Subsonic = false,
                Tracer = false,
                Velocity = 730,
                Weight = 0.012,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/7.62x39mm_MAI_AP"
            },
            new Armor()
            {
                ArmorClass = 6,
                ArmoredAreas = new string[]
                {
                    "Left arm",
                    "Right arm",
                    "Thorax",
                    "Stomach"
                },
                //ConflictingItemIds = , // TODO : MISSING
                CategoryId = "armor",
                Durability = 85,
                ErgonomicsPercentageModifier = -0.27,
                IconLink = "https://assets.tarkov.dev/545cdb794bdc2d3a198b456a-icon.jpg",
                Id = "545cdb794bdc2d3a198b456a",
                ImageLink = "https://assets.tarkov.dev/545cdb794bdc2d3a198b456a-image.jpg",
                MarketLink = "https://tarkov.dev/item/6b43-6a-zabralo-sh-body-armor",
                Material = "Combined materials",
                MaxStackableAmount = 1,
                MovementSpeedPercentageModifier = -0.35,
                Name = "6B43 6A \"Zabralo-Sh\" body armor",
                //RicochetChance = , // TODO : MISSING
                ShortName = "6B43 6A",
                TurningSpeedPercentageModifier = -0.21,
                Weight = 20,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/6B43_6A_%22Zabralo-Sh%22_body_armor"
            },
            new ArmorMod()
            {
                ArmorClass = 4,
                ArmoredAreas = new string[]
                {
                    "Eyes",
                    "Jaws"
                },
                //BlindnessProtectionPercentage = , // TODO : MISSING
                CategoryId = "armorMod",
                //ConflictingItemIds = , // TODO : MISSING
                Durability = 85,
                ErgonomicsPercentageModifier = -0.1,
                IconLink = "https://assets.tarkov.dev/5ca2113f86f7740b2547e1d2-icon.jpg",
                Id = "5ca2113f86f7740b2547e1d2",
                ImageLink = "https://assets.tarkov.dev/5ca2113f86f7740b2547e1d2-image.jpg",
                MarketLink = "https://tarkov.dev/item/vulkan-5-face-shield",
                //Material = , // TODO : MISSING
                MaxStackableAmount = 1,
                //ModSlots = , // TODO : MISSING
                MovementSpeedPercentageModifier = 0,
                Name = "Vulkan-5 face shield",
                //RicochetChance = , // TODO : MISSING
                ShortName = "Vulkan-5 FS",
                TurningSpeedPercentageModifier = -0.09,
                Weight = 1.8,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Vulkan-5_face_shield",
            },
            new Container()
            {
                Capacity = 35,
                CategoryId = "backpack",
                //ConflictingItemIds = , // TODO : MISSING
                IconLink = "https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-icon.jpg",
                Id = "5ab8ebf186f7742d8b372e80",
                ImageLink = "https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-image.jpg",
                MarketLink = "https://tarkov.dev/item/sso-attack-2-raid-backpack",
                MaxStackableAmount = 1,
                Name = "SSO Attack 2 raid backpack",
                ShortName = "Attack 2",
                Weight = 6.12,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/SSO_Attack_2_raid_backpack"
            },
            new Eyewear()
            {
                BlindnessProtectionPercentage = 0.1,
                CategoryId = "eyewear",
                //ConflictingItemIds = , // TODO : MISSING
                IconLink = "https://assets.tarkov.dev/5b432be65acfc433000ed01f-icon.jpg",
                Id = "5b432be65acfc433000ed01f",
                ImageLink = "https://assets.tarkov.dev/5b432be65acfc433000ed01f-image.jpg",
                MarketLink = "https://tarkov.dev/item/6b34-anti-fragmentation-glasses",
                MaxStackableAmount = 1,
                Name = "6B34 anti-fragmentation glasses",
                ShortName = "6B34",
                Weight = 0.12,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/6B34_anti-fragmentation_glasses"
            },
            new Grenade()
            {
                CategoryId = "grenade",
                //ConflictingItemIds = , // TODO : MISSING
                ExplosionDelay = 3,
                //FragmentAmmunitionId = , // TODO : MISSING
                FragmentsAmount = 100,
                MaximumExplosionRange = 6,
                MinimumExplosionRange = 2,
                IconLink = "https://assets.tarkov.dev/5e32f56fcb6d5863cc5e5ee4-icon.jpg",
                Id = "5e32f56fcb6d5863cc5e5ee4",
                ImageLink = "https://assets.tarkov.dev/5e32f56fcb6d5863cc5e5ee4-image.jpg",
                MarketLink = "https://tarkov.dev/item/vog-17-khattabka-improvised-hand-grenade",
                MaxStackableAmount = 1,
                Name = "VOG-17 Khattabka improvised hand grenade",
                ShortName = "VOG-17",
                Type = "Grenade",
                Weight = 0.28,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/VOG-17_Khattabka_improvised_hand_grenade"
            },
            new Headwear()
            {
                ArmorClass = 4,
                ArmoredAreas = new string[]
                {
                    "Top",
                    "Nape"
                },
                CategoryId = "headwear",
                //ConflictingItemIds = , // TODO : MISSING
                Deafening = "None",
                Durability = 30,
                ErgonomicsPercentageModifier = -0.06,
                IconLink = "https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-icon.jpg",
                Id = "5e4bfc1586f774264f7582d3",
                ImageLink = "https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-image.jpg",
                MarketLink = "https://tarkov.dev/item/msa-gallet-tc-800-high-cut-combat-helmet",
                Material = "Combined materials",
                MaxStackableAmount = 1,
                //ModSlots = , // TODO : MISSING
                MovementSpeedPercentageModifier = -0.02,
                Name = "MSA Gallet TC 800 High Cut combat helmet",
                //RicochetChance = , // TODO : MISSING
                ShortName = "TC 800",
                TurningSpeedPercentageModifier = -0.08,
                Weight = 1.17,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/MSA_Gallet_TC_800_High_Cut_combat_helmet"
            },
            new Magazine()
            {
                //AcceptedAmmunitionIds = , // TODO : MISSING
                Capacity = 7,
                CategoryId = "magazine",
                CheckSpeedPercentageModifier = -0.2,
                //ConflictingItemIds = , // TODO : MISSING
                ErgonomicsModifier = -1,
                IconLink = "https://assets.tarkov.dev/5e81c4ca763d9f754677befa-icon.jpg",
                Id = "5e81c4ca763d9f754677befa",
                ImageLink = "https://assets.tarkov.dev/5e81c4ca763d9f754677befa-image.jpg",
                LoadSpeedPercentageModifier = -0.25,
                MalfunctionPercentage = 0.04,
                MarketLink = "https://tarkov.dev/item/m1911a1-45-acp-7-round-magazine",
                MaxStackableAmount = 1,
                ModSlots = Array.Empty<ModSlot>(),
                Name = "M1911A1 .45 ACP 7-round magazine",
                ShortName = "1911",
                Weight = 0.16,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/M1911A1_.45_ACP_7-round_magazine"
            },
            new MeleeWeapon()
            {
                CategoryId = "meleeWeapon",
                //ChopDamage = , // TODO : MISSING
                //ConflictingItemIds = , // TODO : MISSING
                //HitRadius = , // TODO : MISSING
                IconLink = "https://assets.tarkov.dev/5c0126f40db834002a125382-icon.jpg",
                Id = "5c0126f40db834002a125382",
                ImageLink = "https://assets.tarkov.dev/5c0126f40db834002a125382-image.jpg",
                MarketLink = "https://tarkov.dev/item/red-rebel-ice-pick",
                MaxStackableAmount = 1,
                Name = "Red Rebel ice pick",
                ShortName = "RedRebel",
                //StabDamage = , // TODO : MISSING
                Weight = 0.65,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Red_Rebel_ice_pick"
            },
            new Mod()
            {
                CategoryId = "mod",
                //ConflictingItemIds = , // TODO : MISSING
                ErgonomicsModifier = -2,
                IconLink = "https://assets.tarkov.dev/59d790f486f77403cb06aec6-icon.jpg",
                Id = "59d790f486f77403cb06aec6",
                ImageLink = "https://assets.tarkov.dev/59d790f486f77403cb06aec6-image.jpg",
                MarketLink = "https://tarkov.dev/item/armytek-predator-pro-v3-xhp35-hi-flashlight",
                MaxStackableAmount = 1,
                //ModSlots = , // TODO : MISSING
                Name = "Armytek Predator Pro v3 XHP35 HI flashlight",
                ShortName = "XHP35",
                Weight = 0.12,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Armytek_Predator_Pro_v3_XHP35_HI_flashlight"
            },
            new RangedWeapon()
            {
                Caliber = "Caliber545x39",
                CategoryId = "mainWeapon",
                //ConflictingItemIds = , // TODO : MISSING
                Ergonomics = 44,
                FireModes = new string[] { "Single fire", "Full auto" },
                FireRate = 650,
                HorizontalRecoil = 445,
                IconLink = "https://assets.tarkov.dev/57dc2fa62459775949412633-icon.jpg",
                Id = "57dc2fa62459775949412633",
                ImageLink = "https://assets.tarkov.dev/57dc2fa62459775949412633-image.jpg",
                MarketLink = "https://tarkov.dev/item/kalashnikov-aks-74u-545x39-assault-rifle",
                MaxStackableAmount = 1,
                //ModSlots = , // TODO : MISSING
                Name = "Kalashnikov AKS-74U 5.45x39 assault rifle",
                ShortName = "AKS-74U",
                VerticalRecoil = 141,
                Weight = 1.809,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Kalashnikov_AKS-74U_5.45x39_assault_rifle"
            },
            new RangedWeaponMod()
            {
                //AccuracyPercentageModifier = , // TODO : MISSING
                CategoryId = "rangedWeaponMod",
                //ConflictingItemIds = , // TODO : MISSING
                ErgonomicsModifier = 5,
                IconLink = "https://assets.tarkov.dev/5d2c76ed48f03532f2136169-icon.jpg",
                Id = "5d2c76ed48f03532f2136169",
                ImageLink = "https://assets.tarkov.dev/5d2c76ed48f03532f2136169-image.jpg",
                MarketLink = "https://tarkov.dev/item/ak-akademia-bastion-dust-cover",
                MaxStackableAmount = 1,
                //ModSlots = , // TODO : MISSING
                Name = "AK AKademia Bastion dust cover",
                RecoilPercentageModifier = -0.01,
                ShortName = "Bastion",
                Weight = 0.237,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/AK_AKademia_Bastion_dust_cover"
            },
            new RangedWeaponMod()
            {
                //AccuracyPercentageModifier = , // TODO : MISSING
                CategoryId = "rangedWeaponMod",
                //ConflictingItemIds = , // TODO : MISSING
                ErgonomicsModifier = -6,
                IconLink = "https://assets.tarkov.dev/61714eec290d254f5e6b2ffc-icon.jpg",
                Id = "61714eec290d254f5e6b2ffc",
                ImageLink = "https://assets.tarkov.dev/61714eec290d254f5e6b2ffc-image.jpg",
                MarketLink = "https://tarkov.dev/item/schmidt-bender-pm-ii-3-12x50-scope",
                MaxStackableAmount = 1,
                //ModSlots = , // TODO : MISSING
                Name = "Schmidt & Bender PM II 3-12x50 scope",
                RecoilPercentageModifier = 0,
                ShortName = "PM II 3-12x50",
                Weight = 0.9,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Schmidt_%26_Bender_PM_II_3-12x50_scope"
            },
            new Vest()
            {
                ArmorClass = 4,
                ArmoredAreas = new string[]
                {
                    "Thorax",
                    "Stomach"
                },
                Capacity = 12,
                CategoryId = "vest",
                //ConflictingItemIds = , // TODO : MISSING
                Durability = 40,
                ErgonomicsPercentageModifier = -0.15,
                IconLink = "https://assets.tarkov.dev/5d5d646386f7742797261fd9-icon.jpg",
                Id = "5d5d646386f7742797261fd9",
                ImageLink = "https://assets.tarkov.dev/5d5d646386f7742797261fd9-image.jpg",
                MarketLink = "https://tarkov.dev/item/6b3tm-01m-armored-rig",
                Material = "Titan",
                MaxStackableAmount = 1,
                MovementSpeedPercentageModifier = -0.10,
                Name = "6B3TM-01M armored rig",
                RicochetChance = null, // TODO : MISSING
                ShortName = "6B3TM-01M",
                TurningSpeedPercentageModifier = -0.05,
                Weight = 9.2,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/6B3TM-01M_armored_rig"
            },
            new Item()
            {
                CategoryId = "other",
                //ConflictingItemIds = , // TODO : MISSING
                IconLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg",
                Id = "5c1d0c5f86f7744bb2683cf0",
                ImageLink = "https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg",
                MarketLink = "https://tarkov.dev/item/terragroup-labs-keycard-blue",
                MaxStackableAmount = 1,
                Name = "TerraGroup Labs keycard (Blue)",
                ShortName = "Blue",
                Weight = 0.01,
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)"
            }
        };

        public const string ItemsJson = @"{
  ""data"": {
    ""items"": [
    {
        ""categories"": [
            {
                ""id"": ""5485a8684bdc2da71d8b4567""
            }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-icon.jpg"",
        ""id"": ""601aa3d2b2bcb34913271e6d"",
        ""imageLink"": ""https://assets.tarkov.dev/601aa3d2b2bcb34913271e6d-image.jpg"",
        ""link"": ""https://tarkov.dev/item/762x39mm-mai-ap"",
        ""name"": ""7.62x39mm MAI AP"",
        ""properties"": {
            ""__typename"": ""ItemPropertiesAmmo"",
            ""accuracy"": -5,
            ""armorDamage"": 76,
            ""caliber"": ""Caliber762x39"",
            ""damage"": 47,
            ""fragmentationChance"": 0.05,
            ""heavyBleedModifier"": 0.1,
            ""initialSpeed"": 730,
            ""lightBleedModifier"": 0.1,
            ""penetrationChance"": 0.65,
            ""penetrationPower"": 58,
            ""projectileCount"": 1,
            ""recoil"": 10,
            ""ricochetChance"": 0.435,
            ""stackMaxSize"": 60,
            ""tracer"": false
        },
        ""shortName"": ""MAI AP"",
        ""weight"": 0.012,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/7.62x39mm_MAI_AP""
        },
      {
        ""categories"": [
          {
            ""id"": ""5448e54d4bdc2dcc718b4568""
          },
          {
            ""id"": ""57bef4c42459772e8d35a53b""
          },
          {
            ""id"": ""543be5f84bdc2dd4348b456a""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/545cdb794bdc2d3a198b456a-icon.jpg"",
        ""id"": ""545cdb794bdc2d3a198b456a"",
        ""imageLink"": ""https://assets.tarkov.dev/545cdb794bdc2d3a198b456a-image.jpg"",
        ""link"": ""https://tarkov.dev/item/6b43-6a-zabralo-sh-body-armor"",
        ""name"": ""6B43 6A \""Zabralo-Sh\"" body armor"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesArmor"",
          ""class"": 6,
          ""durability"": 85,
          ""ergoPenalty"": -27,
          ""material"": {
            ""name"": ""Combined materials""
          },
          ""speedPenalty"": -0.35,
          ""turnPenalty"": -0.21,
          ""zones"": [""Left Arm"", ""Right Arm"", ""THORAX"", ""STOMACH""]
        },
        ""shortName"": ""6B43 6A"",
        ""weight"": 20,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/6B43_6A_%22Zabralo-Sh%22_body_armor""
      },
      {
        ""categories"": [
          {
            ""id"": ""57bef4c42459772e8d35a53b""
          },
          {
            ""id"": ""543be5f84bdc2dd4348b456a""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5ca2113f86f7740b2547e1d2-icon.jpg"",
        ""id"": ""5ca2113f86f7740b2547e1d2"",
        ""imageLink"": ""https://assets.tarkov.dev/5ca2113f86f7740b2547e1d2-image.jpg"",
        ""link"": ""https://tarkov.dev/item/vulkan-5-face-shield"",
        ""name"": ""Vulkan-5 face shield"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesArmorAttachment"",
          ""class"": 4,
          ""durability"": 85,
          ""ergoPenalty"": -10,
          ""headZones"": [
            ""Eyes"",
            ""Jaws""
          ],
          ""material"": null,
          ""speedPenalty"": 0,
          ""turnPenalty"": -0.09
        },
        ""shortName"": ""Vulkan-5 FS"",
        ""weight"": 1.8,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Vulkan-5_face_shield""
      },
      {
        ""categories"": [
          {
            ""id"": ""5448e53e4bdc2d60728b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-icon.jpg"",
        ""id"": ""5ab8ebf186f7742d8b372e80"",
        ""imageLink"": ""https://assets.tarkov.dev/5ab8ebf186f7742d8b372e80-image.jpg"",
        ""link"": ""https://tarkov.dev/item/sso-attack-2-raid-backpack"",
        ""name"": ""SSO Attack 2 raid backpack"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesBackpack"",
          ""capacity"": 35
        },
        ""shortName"": ""Attack 2"",
        ""weight"": 6.12,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/SSO_Attack_2_raid_backpack""
      },
      {
        ""categories"": [
          {
            ""id"": ""5448e5724bdc2ddf718b4568""
          },
          {
            ""id"": ""57bef4c42459772e8d35a53b""
          },
          {
            ""id"": ""543be5f84bdc2dd4348b456a""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5b432be65acfc433000ed01f-icon.jpg"",
        ""id"": ""5b432be65acfc433000ed01f"",
        ""imageLink"": ""https://assets.tarkov.dev/5b432be65acfc433000ed01f-image.jpg"",
        ""link"": ""https://tarkov.dev/item/6b34-anti-fragmentation-glasses"",
        ""name"": ""6B34 anti-fragmentation glasses"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesGlasses"",
          ""blindnessProtection"": 0.1,
          ""class"": 0,
          ""durability"": 0,
          ""material"": null
        },
        ""shortName"": ""6B34"",
        ""weight"": 0.12,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/6B34_anti-fragmentation_glasses""
      },
      {
        ""categories"": [
          {
            ""id"": ""543be6564bdc2df4348b4568""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5e32f56fcb6d5863cc5e5ee4-icon.jpg"",
        ""id"": ""5e32f56fcb6d5863cc5e5ee4"",
        ""imageLink"": ""https://assets.tarkov.dev/5e32f56fcb6d5863cc5e5ee4-image.jpg"",
        ""link"": ""https://tarkov.dev/item/vog-17-khattabka-improvised-hand-grenade"",
        ""name"": ""VOG-17 Khattabka improvised hand grenade"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesGrenade"",
          ""contusionRadius"": 9,
          ""fragments"": 100,
          ""fuse"": 3,
          ""maxExplosionDistance"": 6,
          ""minExplosionDistance"": 2,
          ""type"": ""Grenade""
        },
        ""shortName"": ""VOG-17"",
        ""weight"": 0.28,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/VOG-17_Khattabka_improvised_hand_grenade""
      },
      {
        ""categories"": [
          {
            ""id"": ""5a341c4086f77401f2541505""
          },
          {
            ""id"": ""57bef4c42459772e8d35a53b""
          },
          {
            ""id"": ""543be5f84bdc2dd4348b456a""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-icon.jpg"",
        ""id"": ""5e4bfc1586f774264f7582d3"",
        ""imageLink"": ""https://assets.tarkov.dev/5e4bfc1586f774264f7582d3-image.jpg"",
        ""link"": ""https://tarkov.dev/item/msa-gallet-tc-800-high-cut-combat-helmet"",
        ""name"": ""MSA Gallet TC 800 High Cut combat helmet"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesHelmet"",
          ""class"": 4,
          ""deafening"": ""None"",
          ""durability"": 30,
          ""ergoPenalty"": -6,
          ""headZones"": [""Top"", ""Nape""],
          ""material"": {
            ""name"": ""Combined materials""
          },
          ""speedPenalty"": -0.02,
          ""turnPenalty"": -0.08
        },
        ""shortName"": ""TC 800"",
        ""weight"": 1.17,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/MSA_Gallet_TC_800_High_Cut_combat_helmet""
      },
      {
        ""categories"": [
          {
            ""id"": ""5448bc234bdc2d3c308b4569""
          },
          {
            ""id"": ""55802f3e4bdc2de7118b4584""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5e81c4ca763d9f754677befa-icon.jpg"",
        ""id"": ""5e81c4ca763d9f754677befa"",
        ""imageLink"": ""https://assets.tarkov.dev/5e81c4ca763d9f754677befa-image.jpg"",
        ""link"": ""https://tarkov.dev/item/m1911a1-45-acp-7-round-magazine"",
        ""name"": ""M1911A1 .45 ACP 7-round magazine"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesMagazine"",
          ""ammoCheckModifier"": -0.2,
          ""capacity"": 7,
          ""ergonomics"": -1,
          ""loadModifier"": -0.25,
          ""malfunctionChance"": 0.04
        },
        ""shortName"": ""1911"",
        ""weight"": 0.16,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/M1911A1_.45_ACP_7-round_magazine""
      },
      {
        ""categories"": [
          {
            ""id"": ""5447e1d04bdc2dff2f8b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c0126f40db834002a125382-icon.jpg"",
        ""id"": ""5c0126f40db834002a125382"",
        ""imageLink"": ""https://assets.tarkov.dev/5c0126f40db834002a125382-image.jpg"",
        ""link"": ""https://tarkov.dev/item/red-rebel-ice-pick"",
        ""name"": ""Red Rebel ice pick"",
        ""properties"": null,
        ""shortName"": ""RedRebel"",
        ""weight"": 0.65,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Red_Rebel_ice_pick""
      },
      {
        ""categories"": [
          {
            ""id"": ""55818b084bdc2d5b648b4571""
          },
          {
            ""id"": ""550aa4154bdc2dd8348b456b""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/59d790f486f77403cb06aec6-icon.jpg"",
        ""id"": ""59d790f486f77403cb06aec6"",
        ""imageLink"": ""https://assets.tarkov.dev/59d790f486f77403cb06aec6-image.jpg"",
        ""link"": ""https://tarkov.dev/item/armytek-predator-pro-v3-xhp35-hi-flashlight"",
        ""name"": ""Armytek Predator Pro v3 XHP35 HI flashlight"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesWeaponMod"",
          ""ergonomics"": -2,
          ""recoil"": 0
        },
        ""shortName"": ""XHP35"",
        ""weight"": 0.12,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Armytek_Predator_Pro_v3_XHP35_HI_flashlight""
      },
      {
        ""categories"": [
          {
            ""id"": ""5447b5f14bdc2d61278b4567""
          },
          {
            ""id"": ""5422acb9af1c889c16000029""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/57dc2fa62459775949412633-icon.jpg"",
        ""id"": ""57dc2fa62459775949412633"",
        ""imageLink"": ""https://assets.tarkov.dev/57dc2fa62459775949412633-image.jpg"",
        ""link"": ""https://tarkov.dev/item/kalashnikov-aks-74u-545x39-assault-rifle"",
        ""name"": ""Kalashnikov AKS-74U 5.45x39 assault rifle"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesWeapon"",
          ""caliber"": ""Caliber545x39"",
          ""ergonomics"": 44,
          ""fireModes"": [""Single fire"", ""Full Auto""],
          ""fireRate"": 650,
          ""recoilHorizontal"": 445,
          ""recoilVertical"": 141
        },
        ""shortName"": ""AKS-74U"",
        ""weight"": 1.809,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Kalashnikov_AKS-74U_5.45x39_assault_rifle""
      },
      {
        ""categories"": [
          {
            ""id"": ""55818a304bdc2db5418b457d""
          },
          {
            ""id"": ""55802f4a4bdc2ddb688b4569""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5d2c76ed48f03532f2136169-icon.jpg"",
        ""id"": ""5d2c76ed48f03532f2136169"",
        ""imageLink"": ""https://assets.tarkov.dev/5d2c76ed48f03532f2136169-image.jpg"",
        ""link"": ""https://tarkov.dev/item/ak-akademia-bastion-dust-cover"",
        ""name"": ""AK AKademia Bastion dust cover"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesWeaponMod"",
          ""ergonomics"": 5,
          ""recoil"": -1
        },
        ""shortName"": ""Bastion"",
        ""weight"": 0.237,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/AK_AKademia_Bastion_dust_cover""
      },
      {
        ""categories"": [
          {
            ""id"": ""55818ae44bdc2dde698b456c""
          },
          {
            ""id"": ""5448fe7a4bdc2d6f028b456b""
          },
          {
            ""id"": ""550aa4154bdc2dd8348b456b""
          },
          {
            ""id"": ""5448fe124bdc2da5018b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/61714eec290d254f5e6b2ffc-icon.jpg"",
        ""id"": ""61714eec290d254f5e6b2ffc"",
        ""imageLink"": ""https://assets.tarkov.dev/61714eec290d254f5e6b2ffc-image.jpg"",
        ""link"": ""https://tarkov.dev/item/schmidt-bender-pm-ii-3-12x50-scope"",
        ""name"": ""Schmidt & Bender PM II 3-12x50 scope"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesScope"",
          ""ergonomics"": -6,
          ""recoil"": 0,
          ""zoomLevels"": [[12, 3]]
        },
        ""shortName"": ""PM II 3-12x50"",
        ""weight"": 0.9,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Schmidt_%26_Bender_PM_II_3-12x50_scope""
      },
      {
        ""categories"": [
          {
            ""id"": ""5448e5284bdc2dcb718b4567""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5d5d646386f7742797261fd9-icon.jpg"",
        ""id"": ""5d5d646386f7742797261fd9"",
        ""imageLink"": ""https://assets.tarkov.dev/5d5d646386f7742797261fd9-image.jpg"",
        ""link"": ""https://tarkov.dev/item/6b3tm-01m-armored-rig"",
        ""name"": ""6B3TM-01M armored rig"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesChestRig"",
          ""capacity"": 12,
          ""class"": 4,
          ""durability"": 40,
          ""ergoPenalty"": -15,
          ""material"": {
            ""name"": ""Titan""
          },
          ""speedPenalty"": -0.1,
          ""turnPenalty"": -0.05,
          ""zones"": [""THORAX"", ""STOMACH""]
        },
        ""shortName"": ""6B3TM-01M"",
        ""weight"": 9.2,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/6B3TM-01M_armored_rig""
      },
      {
        ""categories"": [
          {
            ""id"": ""5c164d2286f774194c5e69fa""
          },
          {
            ""id"": ""543be5e94bdc2df1348b4568""
          }
        ],
        ""iconLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-icon.jpg"",
        ""id"": ""5c1d0c5f86f7744bb2683cf0"",
        ""imageLink"": ""https://assets.tarkov.dev/5c1d0c5f86f7744bb2683cf0-image.jpg"",
        ""link"": ""https://tarkov.dev/item/terragroup-labs-keycard-blue"",
        ""name"": ""TerraGroup Labs keycard (Blue)"",
        ""properties"": {
          ""__typename"": ""ItemPropertiesKey""
        },
        ""shortName"": ""Blue"",
        ""weight"": 0.01,
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/TerraGroup_Labs_keycard_(Blue)""
      }
    ]
  }
}";

//        public const string ItemsJson = @"{
//    ""57dc2fa62459775949412633"": {
//        ""_id"": ""57dc2fa62459775949412633"",
//        ""_name"": ""weapon_izhmash_aks74u_545x39"",
//        ""_parent"": ""5447b5f14bdc2d61278b4567"",
//        ""_type"": ""Item"",
//        ""_props"": {
//        ""Name"": ""Kalashnikov AKS-74U 5.45x39"",
//        ""ShortName"": ""AKS-74U"",
//        ""Description"": ""Reduced version of AKS-74 assault rifle, developed in the early 80s for combat vehicle crews and airborne troops, also became very popular with law enforcement and special forces for its compact size."",
//        ""Weight"": 1.809,
//        ""BackgroundColor"": ""black"",
//        ""Width"": 3,
//        ""Height"": 1,
//        ""StackMaxSize"": 1,
//        ""Rarity"": ""Rare"",
//        ""SpawnChance"": 15,
//        ""CreditsPrice"": 13643,
//        ""ItemSound"": ""weap_ar"",
//        ""Prefab"": {
//            ""path"": ""assets/content/weapons/aks74u/weapon_izhmash_aks74u_545x39_container.bundle"",
//            ""rcid"": """"
//        },
//        ""UsePrefab"": {
//            ""path"": """",
//            ""rcid"": """"
//        },
//        ""StackObjectsCount"": 1,
//        ""NotShownInSlot"": false,
//        ""ExaminedByDefault"": true,
//        ""ExamineTime"": 1,
//        ""IsUndiscardable"": false,
//        ""IsUnsaleable"": false,
//        ""IsUnbuyable"": false,
//        ""IsUngivable"": false,
//        ""IsLockedafterEquip"": false,
//        ""QuestItem"": false,
//        ""LootExperience"": 20,
//        ""ExamineExperience"": 4,
//        ""HideEntrails"": false,
//        ""RepairCost"": 70,
//        ""RepairSpeed"": 4,
//        ""ExtraSizeLeft"": 0,
//        ""ExtraSizeRight"": 0,
//        ""ExtraSizeUp"": 0,
//        ""ExtraSizeDown"": 0,
//        ""ExtraSizeForceAdd"": false,
//        ""MergesWithChildren"": true,
//        ""CanSellOnRagfair"": true,
//        ""CanRequireOnRagfair"": true,
//        ""ConflictingItems"": [],
//        ""FixedPrice"": false,
//        ""Unlootable"": false,
//        ""UnlootableFromSlot"": ""FirstPrimaryWeapon"",
//        ""UnlootableFromSide"": [],
//        ""ChangePriceCoef"": 1,
//        ""AllowSpawnOnLocations"": [],
//        ""SendToClient"": false,
//        ""AnimationVariantsNumber"": 0,
//        ""DiscardingBlock"": false,
//        ""RagFairCommissionModifier"": 1,
//        ""Grids"": [],
//        ""Slots"": [
//            {
//            ""_name"": ""mod_pistol_grip"",
//            ""_id"": ""57dc31bc245977596d4ef3d2"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Shift"": 0,
//                    ""Filter"": [
//                    ""5f6341043ada5942720e2dc5"",
//                    ""5beec8ea0db834001a6f9dbf"",
//                    ""5649ad3f4bdc2df8348b4585"",
//                    ""5649ade84bdc2d1b2b8b4587"",
//                    ""59e62cc886f77440d40b52a1"",
//                    ""5a0071d486f77404e23a12b2"",
//                    ""57e3dba62459770f0c32322b"",
//                    ""5cf54404d7f00c108840b2ef"",
//                    ""5e2192a498a36665e8337386"",
//                    ""5b30ac585acfc433000eb79c"",
//                    ""59e6318286f77444dd62c4cc"",
//                    ""5cf50850d7f00c056e24104c"",
//                    ""5cf508bfd7f00c056e24104e"",
//                    ""5947f92f86f77427344a76b1"",
//                    ""5947fa2486f77425b47c1a9b"",
//                    ""5c6bf4aa2e2216001219b0ae"",
//                    ""5649ae4a4bdc2d1b2b8b4588"",
//                    ""5998517986f7746017232f7e""
//                    ]
//                }
//                ]
//            },
//            ""_required"": true,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c4c4bdc2db4468b457e""
//            },
//            {
//            ""_name"": ""mod_stock"",
//            ""_id"": ""57dc31ce245977593d4e1453"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Shift"": 0,
//                    ""Filter"": [
//                    ""59ecc28286f7746d7a68aa8c"",
//                    ""5ab626e4d8ce87272e4c6e43"",
//                    ""57dc347d245977596754e7a1""
//                    ]
//                }
//                ]
//            },
//            ""_required"": false,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c4c4bdc2db4468b457e""
//            },
//            {
//            ""_name"": ""mod_charge"",
//            ""_id"": ""57dc31e1245977597164366e"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Shift"": 0,
//                    ""Filter"": [
//                    ""5648ac824bdc2ded0b8b457d""
//                    ]
//                }
//                ]
//            },
//            ""_required"": false,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c4c4bdc2db4468b457e""
//            },
//            {
//            ""_name"": ""mod_magazine"",
//            ""_id"": ""57dc31f2245977596c274b4f"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""AnimationIndex"": -1,
//                    ""Filter"": [
//                    ""564ca9df4bdc2d35148b4569"",
//                    ""564ca99c4bdc2d16268b4589"",
//                    ""55d480c04bdc2d1d4e8b456a"",
//                    ""5cbdaf89ae9215000e5b9c94"",
//                    ""55d481904bdc2d8c2f8b456a"",
//                    ""55d482194bdc2d1d4e8b456b"",
//                    ""55d4837c4bdc2d1d4e8b456c"",
//                    ""5aaa4194e5b5b055d06310a5"",
//                    ""5bed61680db834001d2c45ab"",
//                    ""5bed625c0db834001c062946""
//                    ]
//                }
//                ]
//            },
//            ""_required"": false,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c394bdc2dae468b4577""
//            },
//            {
//            ""_name"": ""mod_muzzle"",
//            ""_id"": ""57dc35ce2459775971643671"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Shift"": 0,
//                    ""Filter"": [
//                    ""5ac72e945acfc43f3b691116"",
//                    ""5ac7655e5acfc40016339a19"",
//                    ""5649aa744bdc2ded0b8b457e"",
//                    ""5f633f791b231926f2329f13"",
//                    ""5943eeeb86f77412d6384f6b"",
//                    ""5cc9a96cd7f00c011c04e04a"",
//                    ""5649ab884bdc2ded0b8b457f"",
//                    ""57dc324a24597759501edc20"",
//                    ""59bffc1f86f77435b128b872"",
//                    ""593d493f86f7745e6b2ceb22"",
//                    ""564caa3d4bdc2d17108b458e"",
//                    ""57ffb0e42459777d047111c5""
//                    ]
//                }
//                ]
//            },
//            ""_required"": false,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c4c4bdc2db4468b457e""
//            },
//            {
//            ""_name"": ""mod_reciever"",
//            ""_id"": ""57dc35fb245977596d4ef3d7"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Shift"": 0,
//                    ""Filter"": [
//                    ""57dc334d245977597164366f"",
//                    ""5839a7742459773cf9693481""
//                    ]
//                }
//                ]
//            },
//            ""_required"": false,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c4c4bdc2db4468b457e""
//            },
//            {
//            ""_name"": ""mod_gas_block"",
//            ""_id"": ""59d368ce86f7747e6a5beb03"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Shift"": 0,
//                    ""Filter"": [
//                    ""59d36a0086f7747e673f3946""
//                    ]
//                }
//                ]
//            },
//            ""_required"": true,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d30c4c4bdc2db4468b457e""
//            }
//        ],
//        ""CanPutIntoDuringTheRaid"": true,
//        ""CantRemoveFromSlotsDuringRaid"": [],
//        ""weapClass"": ""assaultRifle"",
//        ""weapUseType"": ""primary"",
//        ""ammoCaliber"": ""Caliber545x39"",
//        ""Durability"": 95,
//        ""MaxDurability"": 100,
//        ""OperatingResource"": 4000,
//        ""RepairComplexity"": 0,
//        ""durabSpawnMin"": 25,
//        ""durabSpawnMax"": 75,
//        ""isFastReload"": true,
//        ""RecoilForceUp"": 152,
//        ""RecoilForceBack"": 455,
//        ""Convergence"": 1,
//        ""RecoilAngle"": 90,
//        ""weapFireType"": [
//            ""single"",
//            ""fullauto""
//        ],
//        ""RecolDispersion"": 35,
//        ""bFirerate"": 650,
//        ""Ergonomics"": 40,
//        ""Velocity"": -17.9,
//        ""bEffDist"": 300,
//        ""bHearDist"": 80,
//        ""isChamberLoad"": true,
//        ""chamberAmmoCount"": 1,
//        ""isBoltCatch"": false,
//        ""defMagType"": ""55d480c04bdc2d1d4e8b456a"",
//        ""defAmmo"": ""56dff3afd2720bba668b4567"",
//        ""shotgunDispersion"": 0,
//        ""Chambers"": [
//            {
//            ""_name"": ""patron_in_weapon"",
//            ""_id"": ""57dc318524597759805c1581"",
//            ""_parent"": ""57dc2fa62459775949412633"",
//            ""_props"": {
//                ""filters"": [
//                {
//                    ""Filter"": [
//                    ""5c0d5e4486f77478390952fe"",
//                    ""56dfef82d2720bbd668b4567"",
//                    ""56dff026d2720bb8668b4567"",
//                    ""56dff061d2720bb5668b4567"",
//                    ""56dff0bed2720bb0668b4567"",
//                    ""56dff216d2720bbd668b4568"",
//                    ""56dff2ced2720bb4668b4567"",
//                    ""56dff338d2720bbd668b4569"",
//                    ""56dff3afd2720bba668b4567"",
//                    ""56dff421d2720b5f5a8b4567"",
//                    ""56dff4a2d2720bbd668b456a"",
//                    ""56dff4ecd2720b5f5a8b4568""
//                    ],
//                    ""MaxStackCount"": 0
//                }
//                ]
//            },
//            ""_required"": false,
//            ""_mergeSlotWithChildren"": false,
//            ""_proto"": ""55d4af244bdc2d962f8b4571""
//            }
//        ],
//        ""CameraRecoil"": 0.165,
//        ""CameraSnap"": 3.5,
//        ""ReloadMode"": ""ExternalMagazine"",
//        ""CenterOfImpact"": 0.1,
//        ""AimPlane"": 0.15,
//        ""DeviationCurve"": 1,
//        ""DeviationMax"": 100,
//        ""Foldable"": true,
//        ""Retractable"": false,
//        ""TacticalReloadStiffnes"": {
//            ""x"": 0.95,
//            ""y"": 0.33,
//            ""z"": 0.95
//        },
//        ""TacticalReloadFixation"": 0.95,
//        ""RecoilCenter"": {
//            ""x"": 0,
//            ""y"": -0.25,
//            ""z"": 0
//        },
//        ""RotationCenter"": {
//            ""x"": 0,
//            ""y"": -0.1,
//            ""z"": -0.03
//        },
//        ""RotationCenterNoStock"": {
//            ""x"": 0,
//            ""y"": -0.27,
//            ""z"": -0.08
//        },
//        ""SizeReduceRight"": 0,
//        ""FoldedSlot"": ""mod_stock"",
//        ""CompactHandling"": true,
//        ""SightingRange"": 100,
//        ""MinRepairDegradation"": 0,
//        ""MaxRepairDegradation"": 0.01,
//        ""IronSightRange"": 100,
//        ""MustBoltBeOpennedForExternalReload"": false,
//        ""MustBoltBeOpennedForInternalReload"": false,
//        ""BoltAction"": false,
//        ""HipAccuracyRestorationDelay"": 0.2,
//        ""HipAccuracyRestorationSpeed"": 7,
//        ""HipInnaccuracyGain"": 0.16,
//        ""ManualBoltCatch"": false,
//        ""AimSensitivity"": 0.65,
//        ""BurstShotsCount"": 3
//        },
//        ""_proto"": ""5644bd2b4bdc2d3b4c8b4572""
//    }
//}";
    }
}