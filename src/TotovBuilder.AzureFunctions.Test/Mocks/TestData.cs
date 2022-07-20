using System;
using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static class TestData
    {
        public const string EmptyApiData1 = @"{
  ""data"": {
    ""tasks"": {}
  }
}";

        public const string EmptyApiData2 = @"{
  ""data"": {
    ""tasks"": []
  }
}";

        public const string EmptyApiData3 = @"{
  ""data"": {
    ""tasks"": """"
  }
}";

        public const string InvalidApiData1 = @"{
  data"": {
    ""tasks"": null
  }
}";

        public const string InvalidApiData2 = @"{
  ""invalid"": {
    ""tasks"": null
  }
}";

        public const string InvalidApiData3 = @"{
  ""data"": {
    ""invalid"": null
  }
}";

        public static ChangelogEntry[] Changelog = new ChangelogEntry[]
        {
            new ChangelogEntry()
            {
                Changes = new ChangelogChange[]
                {
                    new ChangelogChange()
                    {
                        Language = "en",
                        Text = "Added a thing."
                    },
                    new ChangelogChange()
                    {
                        Language = "fr",
                        Text = "Ajout d'une chose."
                    }
                },
                Date = new DateTime(2022, 1, 2),
                Version = "1.1.0",
            },
            new ChangelogEntry()
            {
                Changes = new ChangelogChange[]
                {
                    new ChangelogChange()
                    {
                        Language = "en",
                        Text = "Initial release."
                    },
                    new ChangelogChange()
                    {
                        Language = "fr",
                        Text = "Sortie initiale."
                    }
                },
                Date = new DateTime(2022, 1, 1),
                Version = "1.0.0"
            }
        };

        public const string ChangelogJson = @"[
    {
      ""version"": ""1.1.0"",
      ""date"": ""2022-01-02T00:00:00+01:00"",
      ""changes"": [
        {
          ""language"": ""en"",
          ""text"": ""Added a thing.""
        },
        {
          ""language"": ""fr"",
          ""text"": ""Ajout d'une chose.""
        }
      ]
    },
    {
      ""version"": ""1.0.0"",
      ""date"": ""2022-01-01T00:00:00+01:00"",
      ""changes"": [
        {
          ""language"": ""en"",
          ""text"": ""Initial release.""
        },
        {
          ""language"": ""fr"",
          ""text"": ""Sortie initiale.""
        }
      ]
    }
]";

        public static ItemCategory[] ItemCategories = new ItemCategory[]
        {
            new ItemCategory()
            {
                Id = "ammunition",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5485a8684bdc2da71d8b4567", Name = "Ammo" }
                }
            },
            new ItemCategory()
            {
                Id = "armband",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5b3f15d486f77432d0509248", Name = "ArmBand" }
                }
            },
            new ItemCategory()
            {
                Id = "armor",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448e54d4bdc2dcc718b4568", Name = "Armor" }
                }
            },
            new ItemCategory()
            {
                Id = "armorMod",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "57bef4c42459772e8d35a53b", Name = "ArmoredEquipment" }
                }
            },
            new ItemCategory()
            {
                Id = "backpack",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448e53e4bdc2d60728b4567", Name = "Backpack" }
                }
            },
            new ItemCategory()
            {
                Id = "compass",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5f4fbaaca5573a5ac31db429", Name = "Compass" }
                }
            },
            new ItemCategory()
            {
                Id = "currency",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "543be5dd4bdc2deb348b4569", Name = "Money" }
                }
            },
            new ItemCategory()
            {
                Id = "eyewear",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448e5724bdc2ddf718b4568", Name = "Visors" }
                }
            },
            new ItemCategory()
            {
                Id = "faceCover",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5a341c4686f77469e155819e", Name = "FaceCover" }
                }
            },
            new ItemCategory()
            {
                Id = "food",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448e8d64bdc2dce718b4568", Name = "Drink" },
                    new TarkovItemCategory() { Id = "5448e8d04bdc2ddf718b4569", Name = "Food" }
                }
            },
            new ItemCategory()
            {
                Id = "grenade",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "543be6564bdc2df4348b4568", Name = "ThrowWeap" }
                }
            },
            new ItemCategory()
            {
                Id = "headphones",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5645bcb74bdc2ded0b8b4578", Name = "Headphones" }
                }
            },
            new ItemCategory()
            {
                Id = "headwear",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5a341c4086f77401f2541505", Name = "Headwear" }
                }
            },
            new ItemCategory()
            {
                Id = "magazine",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448bc234bdc2d3c308b4569", Name = "Magazine" }
                }
            },
            new ItemCategory()
            {
                Id = "mainWeapon",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5447b5fc4bdc2d87278b4567", Name = "AssaultCarbine" },
                    new TarkovItemCategory() { Id = "5447b5f14bdc2d61278b4567", Name = "AssaultRifle" },
                    new TarkovItemCategory() { Id = "5447bedf4bdc2d87278b4568", Name = "GrenadeLauncher" },
                    new TarkovItemCategory() { Id = "5447bed64bdc2d97278b4568", Name = "MachineGun" },
                    new TarkovItemCategory() { Id = "5447b6194bdc2d67278b4567", Name = "MarksmanRifle" },
                    new TarkovItemCategory() { Id = "5447b6094bdc2dc3278b4567", Name = "Shotgun" },
                    new TarkovItemCategory() { Id = "5447b5e04bdc2d62278b4567", Name = "Smg" },
                    new TarkovItemCategory() { Id = "5447b6254bdc2dc3278b4568", Name = "SniperRifle" }
                }
            },
            new ItemCategory()
            {
                Id = "medical",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448f3a14bdc2d27728b4569", Name = "Drugs" },
                    new TarkovItemCategory() { Id = "5448f3ac4bdc2dce718b4569", Name = "Medical" },
                    new TarkovItemCategory() { Id = "5448f39d4bdc2d0a728b4568", Name = "MedKit" },
                    new TarkovItemCategory() { Id = "5448f3a64bdc2d60728b456a", Name = "Stimulator" }
                }
            },
            new ItemCategory()
            {
                Id = "meleeWeapon",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5447e1d04bdc2dff2f8b4567", Name = "Knife" }
                }
            },
            new ItemCategory()
            {
                Id = "mod",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5a74651486f7744e73386dd1", Name = "AuxiliaryMod" },
                    new TarkovItemCategory() { Id = "55818b084bdc2d5b648b4571", Name = "Flashlight" },
                    new TarkovItemCategory() { Id = "55818b224bdc2dde698b456f", Name = "Mount" },
                    new TarkovItemCategory() { Id = "5a2c3a9486f774688b05e574", Name = "NightVision" },
                    new TarkovItemCategory() { Id = "5d21f59b6dbe99052b54ef83", Name = "ThermalVision" }
                }
            },
            new ItemCategory()
            {
                Id = "rangedWeaponMod",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "55818add4bdc2d5b648b456f", Name = "AssaultScope" },
                    new TarkovItemCategory() { Id = "555ef6e44bdc2de9068b457e", Name = "Barrel" },
                    new TarkovItemCategory() { Id = "55818afb4bdc2dde698b456d", Name = "Bipod" },
                    new TarkovItemCategory() { Id = "55818a6f4bdc2db9688b456b", Name = "Charge" },
                    new TarkovItemCategory() { Id = "55818ad54bdc2ddc698b4569", Name = "Collimator" },
                    new TarkovItemCategory()
                    {
                        Id = "55818acf4bdc2dde698b456b",
                        Name = "CompactCollimator"
                    },
                    new TarkovItemCategory() { Id = "550aa4bf4bdc2dd6348b456b", Name = "FlashHider" },
                    new TarkovItemCategory() { Id = "55818af64bdc2d5b648b4570", Name = "Foregrip" },
                    new TarkovItemCategory() { Id = "56ea9461d2720b67698b456f", Name = "Gasblock" },
                    new TarkovItemCategory() { Id = "55818a104bdc2db9688b4569", Name = "Handguard" },
                    new TarkovItemCategory() { Id = "55818ac54bdc2d5b648b456e", Name = "IronSight" },
                    new TarkovItemCategory() { Id = "55818b014bdc2ddc698b456b", Name = "Launcher" },
                    new TarkovItemCategory() { Id = "550aa4dd4bdc2dc9348b4569", Name = "MuzzleCombo" },
                    new TarkovItemCategory() { Id = "55818ae44bdc2dde698b456c", Name = "OpticScope" },
                    new TarkovItemCategory() { Id = "55818a684bdc2ddd698b456d", Name = "PistolGrip" },
                    new TarkovItemCategory() { Id = "55818a304bdc2db5418b457d", Name = "Receiver" },
                    new TarkovItemCategory() { Id = "550aa4cd4bdc2dd8348b456c", Name = "Silencer" },
                    new TarkovItemCategory() { Id = "55818aeb4bdc2ddc698b456a", Name = "SpecialScope" },
                    new TarkovItemCategory() { Id = "55818a594bdc2db9688b456a", Name = "Stock" },
                    new TarkovItemCategory() { Id = "55818b164bdc2ddc698b456c", Name = "TacticalCombo" }
                }
            },
            new ItemCategory()
            {
                Id = "secondaryWeapon",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5447b5cf4bdc2d65278b4567", Name = "Pistol" }
                }
            },
            new ItemCategory()
            {
                Id = "securedContainer",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448bf274bdc2dfc2f8b456a", Name = "MobContainer" }
                }
            },
            new ItemCategory()
            {
                Id = "vest",
                TarkovItemCategories = new TarkovItemCategory[]
                {
                    new TarkovItemCategory() { Id = "5448e5284bdc2dcb718b4567", Name = "Vest" }
                }
            }
        };

        public const string ItemCategoriesJson = @"[
  {
    ""id"": ""ammunition"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5485a8684bdc2da71d8b4567"",
        ""name"": ""Ammo""
      }
    ]
  },
  {
    ""id"": ""armband"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5b3f15d486f77432d0509248"",
        ""name"": ""ArmBand""
      }
    ]
  },
  {
    ""id"": ""armor"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448e54d4bdc2dcc718b4568"",
        ""name"": ""Armor""
      }
    ]
  },
  {
    ""id"": ""armorMod"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""57bef4c42459772e8d35a53b"",
        ""name"": ""ArmoredEquipment""
      }
    ]
  },
  {
    ""id"": ""backpack"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448e53e4bdc2d60728b4567"",
        ""name"": ""Backpack""
      }
    ]
  },
  {
    ""id"": ""compass"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5f4fbaaca5573a5ac31db429"",
        ""name"": ""Compass""
      }
    ]
  },
  {
    ""id"": ""currency"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""543be5dd4bdc2deb348b4569"",
        ""name"": ""Money""
      }
    ]
  },
  {
    ""id"": ""eyewear"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448e5724bdc2ddf718b4568"",
        ""name"": ""Visors""
      }
    ]
  },
  {
    ""id"": ""faceCover"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5a341c4686f77469e155819e"",
        ""name"": ""FaceCover""
      }
    ]
  },
  {
    ""id"": ""food"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448e8d64bdc2dce718b4568"",
        ""name"": ""Drink""
      },
      {
        ""id"": ""5448e8d04bdc2ddf718b4569"",
        ""name"": ""Food""
      }
    ]
  },
  {
    ""id"": ""grenade"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""543be6564bdc2df4348b4568"",
        ""name"": ""ThrowWeap""
      }
    ]
  },
  {
    ""id"": ""headphones"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5645bcb74bdc2ded0b8b4578"",
        ""name"": ""Headphones""
      }
    ]
  },
  {
    ""id"": ""headwear"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5a341c4086f77401f2541505"",
        ""name"": ""Headwear""
      }
    ]
  },
  {
    ""id"": ""magazine"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448bc234bdc2d3c308b4569"",
        ""name"": ""Magazine""
      }
    ]
  },
  {
    ""id"": ""mainWeapon"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5447b5fc4bdc2d87278b4567"",
        ""name"": ""AssaultCarbine""
      },
      {
        ""id"": ""5447b5f14bdc2d61278b4567"",
        ""name"": ""AssaultRifle""
      },
      {
        ""id"": ""5447bedf4bdc2d87278b4568"",
        ""name"": ""GrenadeLauncher""
      },
      {
        ""id"": ""5447bed64bdc2d97278b4568"",
        ""name"": ""MachineGun""
      },
      {
        ""id"": ""5447b6194bdc2d67278b4567"",
        ""name"": ""MarksmanRifle""
      },
      {
        ""id"": ""5447b6094bdc2dc3278b4567"",
        ""name"": ""Shotgun""
      },
      {
        ""id"": ""5447b5e04bdc2d62278b4567"",
        ""name"": ""Smg""
      },
      {
        ""id"": ""5447b6254bdc2dc3278b4568"",
        ""name"": ""SniperRifle""
      }
    ]
  },
  {
    ""id"": ""medical"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448f3a14bdc2d27728b4569"",
        ""name"": ""Drugs""
      },
      {
        ""id"": ""5448f3ac4bdc2dce718b4569"",
        ""name"": ""Medical""
      },
      {
        ""id"": ""5448f39d4bdc2d0a728b4568"",
        ""name"": ""MedKit""
      },
      {
        ""id"": ""5448f3a64bdc2d60728b456a"",
        ""name"": ""Stimulator""
      }
    ]
  },
  {
    ""id"": ""meleeWeapon"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5447e1d04bdc2dff2f8b4567"",
        ""name"": ""Knife""
      }
    ]
  },
  {
    ""id"": ""mod"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5a74651486f7744e73386dd1"",
        ""name"": ""AuxiliaryMod""
      },
      {
        ""id"": ""55818b084bdc2d5b648b4571"",
        ""name"": ""Flashlight""
      },
      {
        ""id"": ""55818b224bdc2dde698b456f"",
        ""name"": ""Mount""
      },
      {
        ""id"": ""5a2c3a9486f774688b05e574"",
        ""name"": ""NightVision""
      },
      {
        ""id"": ""5d21f59b6dbe99052b54ef83"",
        ""name"": ""ThermalVision""
      }
    ]
  },
  {
    ""id"": ""rangedWeaponMod"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""55818add4bdc2d5b648b456f"",
        ""name"": ""AssaultScope""
      },
      {
        ""id"": ""555ef6e44bdc2de9068b457e"",
        ""name"": ""Barrel""
      },
      {
        ""id"": ""55818afb4bdc2dde698b456d"",
        ""name"": ""Bipod""
      },
      {
        ""id"": ""55818a6f4bdc2db9688b456b"",
        ""name"": ""Charge""
      },
      {
        ""id"": ""55818ad54bdc2ddc698b4569"",
        ""name"": ""Collimator""
      },
      {
        ""id"": ""55818acf4bdc2dde698b456b"",
        ""name"": ""CompactCollimator""
      },
      {
        ""id"": ""550aa4bf4bdc2dd6348b456b"",
        ""name"": ""FlashHider""
      },
      {
        ""id"": ""55818af64bdc2d5b648b4570"",
        ""name"": ""Foregrip""
      },
      {
        ""id"": ""56ea9461d2720b67698b456f"",
        ""name"": ""Gasblock""
      },
      {
        ""id"": ""55818a104bdc2db9688b4569"",
        ""name"": ""Handguard""
      },
      {
        ""id"": ""55818ac54bdc2d5b648b456e"",
        ""name"": ""IronSight""
      },
      {
        ""id"": ""55818b014bdc2ddc698b456b"",
        ""name"": ""Launcher""
      },
      {
        ""id"": ""550aa4dd4bdc2dc9348b4569"",
        ""name"": ""MuzzleCombo""
      },
      {
        ""id"": ""55818ae44bdc2dde698b456c"",
        ""name"": ""OpticScope""
      },
      {
        ""id"": ""55818a684bdc2ddd698b456d"",
        ""name"": ""PistolGrip""
      },
      {
        ""id"": ""55818a304bdc2db5418b457d"",
        ""name"": ""Receiver""
      },
      {
        ""id"": ""550aa4cd4bdc2dd8348b456c"",
        ""name"": ""Silencer""
      },
      {
        ""id"": ""55818aeb4bdc2ddc698b456a"",
        ""name"": ""SpecialScope""
      },
      {
        ""id"": ""55818a594bdc2db9688b456a"",
        ""name"": ""Stock""
      },
      {
        ""id"": ""55818b164bdc2ddc698b456c"",
        ""name"": ""TacticalCombo""
      }
    ]
  },
  {
    ""id"": ""secondaryWeapon"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5447b5cf4bdc2d65278b4567"",
        ""name"": ""Pistol""
      }
    ]
  },
  {
    ""id"": ""securedContainer"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448bf274bdc2dfc2f8b456a"",
        ""name"": ""MobContainer""
      }
    ]
  },
  {
    ""id"": ""vest"",
    ""tarkovItemCategories"": [
      {
        ""id"": ""5448e5284bdc2dcb718b4567"",
        ""name"": ""Vest""
      }
    ]
  }
]
";

        public const string Items = @"{
    ""57dc2fa62459775949412633"": {
        ""_id"": ""57dc2fa62459775949412633"",
        ""_name"": ""weapon_izhmash_aks74u_545x39"",
        ""_parent"": ""5447b5f14bdc2d61278b4567"",
        ""_type"": ""Item"",
        ""_props"": {
        ""Name"": ""Kalashnikov AKS-74U 5.45x39"",
        ""ShortName"": ""AKS-74U"",
        ""Description"": ""Reduced version of AKS-74 assault rifle, developed in the early 80s for combat vehicle crews and airborne troops, also became very popular with law enforcement and special forces for its compact size."",
        ""Weight"": 1.809,
        ""BackgroundColor"": ""black"",
        ""Width"": 3,
        ""Height"": 1,
        ""StackMaxSize"": 1,
        ""Rarity"": ""Rare"",
        ""SpawnChance"": 15,
        ""CreditsPrice"": 13643,
        ""ItemSound"": ""weap_ar"",
        ""Prefab"": {
            ""path"": ""assets/content/weapons/aks74u/weapon_izhmash_aks74u_545x39_container.bundle"",
            ""rcid"": """"
        },
        ""UsePrefab"": {
            ""path"": """",
            ""rcid"": """"
        },
        ""StackObjectsCount"": 1,
        ""NotShownInSlot"": false,
        ""ExaminedByDefault"": true,
        ""ExamineTime"": 1,
        ""IsUndiscardable"": false,
        ""IsUnsaleable"": false,
        ""IsUnbuyable"": false,
        ""IsUngivable"": false,
        ""IsLockedafterEquip"": false,
        ""QuestItem"": false,
        ""LootExperience"": 20,
        ""ExamineExperience"": 4,
        ""HideEntrails"": false,
        ""RepairCost"": 70,
        ""RepairSpeed"": 4,
        ""ExtraSizeLeft"": 0,
        ""ExtraSizeRight"": 0,
        ""ExtraSizeUp"": 0,
        ""ExtraSizeDown"": 0,
        ""ExtraSizeForceAdd"": false,
        ""MergesWithChildren"": true,
        ""CanSellOnRagfair"": true,
        ""CanRequireOnRagfair"": true,
        ""ConflictingItems"": [],
        ""FixedPrice"": false,
        ""Unlootable"": false,
        ""UnlootableFromSlot"": ""FirstPrimaryWeapon"",
        ""UnlootableFromSide"": [],
        ""ChangePriceCoef"": 1,
        ""AllowSpawnOnLocations"": [],
        ""SendToClient"": false,
        ""AnimationVariantsNumber"": 0,
        ""DiscardingBlock"": false,
        ""RagFairCommissionModifier"": 1,
        ""Grids"": [],
        ""Slots"": [
            {
            ""_name"": ""mod_pistol_grip"",
            ""_id"": ""57dc31bc245977596d4ef3d2"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Shift"": 0,
                    ""Filter"": [
                    ""5f6341043ada5942720e2dc5"",
                    ""5beec8ea0db834001a6f9dbf"",
                    ""5649ad3f4bdc2df8348b4585"",
                    ""5649ade84bdc2d1b2b8b4587"",
                    ""59e62cc886f77440d40b52a1"",
                    ""5a0071d486f77404e23a12b2"",
                    ""57e3dba62459770f0c32322b"",
                    ""5cf54404d7f00c108840b2ef"",
                    ""5e2192a498a36665e8337386"",
                    ""5b30ac585acfc433000eb79c"",
                    ""59e6318286f77444dd62c4cc"",
                    ""5cf50850d7f00c056e24104c"",
                    ""5cf508bfd7f00c056e24104e"",
                    ""5947f92f86f77427344a76b1"",
                    ""5947fa2486f77425b47c1a9b"",
                    ""5c6bf4aa2e2216001219b0ae"",
                    ""5649ae4a4bdc2d1b2b8b4588"",
                    ""5998517986f7746017232f7e""
                    ]
                }
                ]
            },
            ""_required"": true,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c4c4bdc2db4468b457e""
            },
            {
            ""_name"": ""mod_stock"",
            ""_id"": ""57dc31ce245977593d4e1453"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Shift"": 0,
                    ""Filter"": [
                    ""59ecc28286f7746d7a68aa8c"",
                    ""5ab626e4d8ce87272e4c6e43"",
                    ""57dc347d245977596754e7a1""
                    ]
                }
                ]
            },
            ""_required"": false,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c4c4bdc2db4468b457e""
            },
            {
            ""_name"": ""mod_charge"",
            ""_id"": ""57dc31e1245977597164366e"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Shift"": 0,
                    ""Filter"": [
                    ""5648ac824bdc2ded0b8b457d""
                    ]
                }
                ]
            },
            ""_required"": false,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c4c4bdc2db4468b457e""
            },
            {
            ""_name"": ""mod_magazine"",
            ""_id"": ""57dc31f2245977596c274b4f"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""AnimationIndex"": -1,
                    ""Filter"": [
                    ""564ca9df4bdc2d35148b4569"",
                    ""564ca99c4bdc2d16268b4589"",
                    ""55d480c04bdc2d1d4e8b456a"",
                    ""5cbdaf89ae9215000e5b9c94"",
                    ""55d481904bdc2d8c2f8b456a"",
                    ""55d482194bdc2d1d4e8b456b"",
                    ""55d4837c4bdc2d1d4e8b456c"",
                    ""5aaa4194e5b5b055d06310a5"",
                    ""5bed61680db834001d2c45ab"",
                    ""5bed625c0db834001c062946""
                    ]
                }
                ]
            },
            ""_required"": false,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c394bdc2dae468b4577""
            },
            {
            ""_name"": ""mod_muzzle"",
            ""_id"": ""57dc35ce2459775971643671"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Shift"": 0,
                    ""Filter"": [
                    ""5ac72e945acfc43f3b691116"",
                    ""5ac7655e5acfc40016339a19"",
                    ""5649aa744bdc2ded0b8b457e"",
                    ""5f633f791b231926f2329f13"",
                    ""5943eeeb86f77412d6384f6b"",
                    ""5cc9a96cd7f00c011c04e04a"",
                    ""5649ab884bdc2ded0b8b457f"",
                    ""57dc324a24597759501edc20"",
                    ""59bffc1f86f77435b128b872"",
                    ""593d493f86f7745e6b2ceb22"",
                    ""564caa3d4bdc2d17108b458e"",
                    ""57ffb0e42459777d047111c5""
                    ]
                }
                ]
            },
            ""_required"": false,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c4c4bdc2db4468b457e""
            },
            {
            ""_name"": ""mod_reciever"",
            ""_id"": ""57dc35fb245977596d4ef3d7"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Shift"": 0,
                    ""Filter"": [
                    ""57dc334d245977597164366f"",
                    ""5839a7742459773cf9693481""
                    ]
                }
                ]
            },
            ""_required"": false,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c4c4bdc2db4468b457e""
            },
            {
            ""_name"": ""mod_gas_block"",
            ""_id"": ""59d368ce86f7747e6a5beb03"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Shift"": 0,
                    ""Filter"": [
                    ""59d36a0086f7747e673f3946""
                    ]
                }
                ]
            },
            ""_required"": true,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d30c4c4bdc2db4468b457e""
            }
        ],
        ""CanPutIntoDuringTheRaid"": true,
        ""CantRemoveFromSlotsDuringRaid"": [],
        ""weapClass"": ""assaultRifle"",
        ""weapUseType"": ""primary"",
        ""ammoCaliber"": ""Caliber545x39"",
        ""Durability"": 95,
        ""MaxDurability"": 100,
        ""OperatingResource"": 4000,
        ""RepairComplexity"": 0,
        ""durabSpawnMin"": 25,
        ""durabSpawnMax"": 75,
        ""isFastReload"": true,
        ""RecoilForceUp"": 152,
        ""RecoilForceBack"": 455,
        ""Convergence"": 1,
        ""RecoilAngle"": 90,
        ""weapFireType"": [
            ""single"",
            ""fullauto""
        ],
        ""RecolDispersion"": 35,
        ""bFirerate"": 650,
        ""Ergonomics"": 40,
        ""Velocity"": -17.9,
        ""bEffDist"": 300,
        ""bHearDist"": 80,
        ""isChamberLoad"": true,
        ""chamberAmmoCount"": 1,
        ""isBoltCatch"": false,
        ""defMagType"": ""55d480c04bdc2d1d4e8b456a"",
        ""defAmmo"": ""56dff3afd2720bba668b4567"",
        ""shotgunDispersion"": 0,
        ""Chambers"": [
            {
            ""_name"": ""patron_in_weapon"",
            ""_id"": ""57dc318524597759805c1581"",
            ""_parent"": ""57dc2fa62459775949412633"",
            ""_props"": {
                ""filters"": [
                {
                    ""Filter"": [
                    ""5c0d5e4486f77478390952fe"",
                    ""56dfef82d2720bbd668b4567"",
                    ""56dff026d2720bb8668b4567"",
                    ""56dff061d2720bb5668b4567"",
                    ""56dff0bed2720bb0668b4567"",
                    ""56dff216d2720bbd668b4568"",
                    ""56dff2ced2720bb4668b4567"",
                    ""56dff338d2720bbd668b4569"",
                    ""56dff3afd2720bba668b4567"",
                    ""56dff421d2720b5f5a8b4567"",
                    ""56dff4a2d2720bbd668b456a"",
                    ""56dff4ecd2720b5f5a8b4568""
                    ],
                    ""MaxStackCount"": 0
                }
                ]
            },
            ""_required"": false,
            ""_mergeSlotWithChildren"": false,
            ""_proto"": ""55d4af244bdc2d962f8b4571""
            }
        ],
        ""CameraRecoil"": 0.165,
        ""CameraSnap"": 3.5,
        ""ReloadMode"": ""ExternalMagazine"",
        ""CenterOfImpact"": 0.1,
        ""AimPlane"": 0.15,
        ""DeviationCurve"": 1,
        ""DeviationMax"": 100,
        ""Foldable"": true,
        ""Retractable"": false,
        ""TacticalReloadStiffnes"": {
            ""x"": 0.95,
            ""y"": 0.33,
            ""z"": 0.95
        },
        ""TacticalReloadFixation"": 0.95,
        ""RecoilCenter"": {
            ""x"": 0,
            ""y"": -0.25,
            ""z"": 0
        },
        ""RotationCenter"": {
            ""x"": 0,
            ""y"": -0.1,
            ""z"": -0.03
        },
        ""RotationCenterNoStock"": {
            ""x"": 0,
            ""y"": -0.27,
            ""z"": -0.08
        },
        ""SizeReduceRight"": 0,
        ""FoldedSlot"": ""mod_stock"",
        ""CompactHandling"": true,
        ""SightingRange"": 100,
        ""MinRepairDegradation"": 0,
        ""MaxRepairDegradation"": 0.01,
        ""IronSightRange"": 100,
        ""MustBoltBeOpennedForExternalReload"": false,
        ""MustBoltBeOpennedForInternalReload"": false,
        ""BoltAction"": false,
        ""HipAccuracyRestorationDelay"": 0.2,
        ""HipAccuracyRestorationSpeed"": 7,
        ""HipInnaccuracyGain"": 0.16,
        ""ManualBoltCatch"": false,
        ""AimSensitivity"": 0.65,
        ""BurstShotsCount"": 3
        },
        ""_proto"": ""5644bd2b4bdc2d3b4c8b4572""
    }
}";

        public const string PresetsJson = @"{
    ""content"": [],
    ""itemId"": ""57dc2fa62459775949412633"",
    ""modSlots"": [
      {
        ""item"": {
          ""content"": [],
          ""itemId"": ""57e3dba62459770f0c32322b"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_pistol_grip""
      },
      {
        ""item"": {
          ""content"": [],
          ""itemId"": ""57dc347d245977596754e7a1"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_stock""
      },
      {
        ""item"": {
          ""content"": [],
          ""itemId"": ""564ca99c4bdc2d16268b4589"",
          ""modSlots"": [
            {
              ""item"": {
                ""content"": [],
                ""itemId"": ""56dfef82d2720bbd668b4567"",
                ""modSlots"": [],
                ""quantity"": 1
              },
              ""modSlotName"": ""cartridges""
            }
          ],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_magazine""
      },
      {
        ""item"": {
          ""content"": [],
          ""itemId"": ""57dc324a24597759501edc20"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_muzzle""
      },
      {
        ""item"": {
          ""content"": [],
          ""itemId"": ""57dc334d245977597164366f"",
          ""modSlots"": [],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_reciever""
      },
      {
        ""item"": {
          ""content"": [],
          ""itemId"": ""59d36a0086f7747e673f3946"",
          ""modSlots"": [
            {
              ""item"": {
                ""content"": [],
                ""itemId"": ""57dc32dc245977596d4ef3d3"",
                ""modSlots"": [],
                ""quantity"": 1
              },
              ""modSlotName"": ""mod_handguard""
            }
          ],
          ""quantity"": 1
        },
        ""modSlotName"": ""mod_gas_block""
      }
    ],
    ""quantity"": 1
  }";

        public static Price[] Prices = new Price[]
        {
            new Price()
            {
                CurrencyName = "RUB",
                Merchant = "mechanic",
                MerchantLevel = 3,
                QuestId = "5ae327c886f7745c7b3f2f3f",
                Value = 67809,
                ValueInMainCurrency = 67809
            },
            new Price()
            {
                CurrencyName = "USD",
                Merchant = "peacekeeper",
                MerchantLevel = 2,
                Value = 900,
                ValueInMainCurrency = 96300
            },
            new Price()
            {
                CurrencyName = "RUB",
                Merchant = "flea-market",
                Value = 88655,
                ValueInMainCurrency = 88655
            },
            new Price()
            {
                CurrencyName = "RUB",
                Merchant = "flea-market",
                Value = 94324,
                ValueInMainCurrency = 94324
            }
        };

        public static Quest[] Quests = new Quest[]
        {
            new Quest()
            {
                Id = "59675d6c86f7740a842fc482",
                Name = "Ice Cream Cones",
                Merchant = "prapor",
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"
            },
            new Quest()
            {
                Id = "1",
                Name = "Checking",
                Merchant = "prapor",
                WikiLink = "https://escapefromtarkov.fandom.com/wiki/Checking"
            }
        };

        public const string QuestsJson = @"{
  ""data"": {
    ""tasks"": [
      {
        ""id"": ""59675d6c86f7740a842fc482"",
        ""name"": ""Ice Cream Cones"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Ice_Cream_Cones"",
        ""trader"": {
          ""normalizedName"": ""prapor""
        }
      },
      {
        ""id"": ""5a27bb1e86f7741f27621b7e"",
        ""name"": ""Cargo X - Part 1"",
        ""wikiLink"": ""https://escapefromtarkov.fandom.com/wiki/Cargo_X_-_Part_1"",
        ""trader"": {
          ""normalizedName"": ""peacekeeper""
        }
      }
    ]
  }
}";
    }
}
