using TotovBuilder.AzureFunctions.Models;

namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    /// <summary>
    /// Represents test data.
    /// </summary>
    public static partial class TestData
    {
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
    }
}
