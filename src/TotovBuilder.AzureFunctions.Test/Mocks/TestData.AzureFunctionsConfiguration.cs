namespace TotovBuilder.AzureFunctions.Test.Mocks
{
    public static partial class TestData
    {
        public static AzureFunctionsConfiguration AzureFunctionsConfiguration = new AzureFunctionsConfiguration()
        {
            ApiBartersQuery = "{ barters { level requiredItems { item { id } quantity } rewardItems { item { id } quantity } trader { normalizedName } taskUnlock { id }  }  }",
            ApiItemsQuery = "{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }",
            ApiPricesQuery = "{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }",
            ApiQuestsQuery = "{ tasks { id name trader { normalizedName } wikiLink } }",
            ApiUrl = "https://api.tarkov.dev/graphql",
            AzureArmorPenetrationsBlobName = "armor-penetrations.json",
            AzureChangelogBlobName = "changelog.json",
            AzureItemCategoriesBlobName = "item-categories.json",
            AzurePresetsBlobName= "presets.json",            
            AzureTarkovValuesBlobName = "tarkov-values.json",
            AzureWebsiteConfigurationBlobName = "website-configuration.json",
            CacheDuration = 43200,
            FetchTimeout = 30,
            PriceCacheDuration = 3600,
        };

        public const string AzureFunctionsConfigurationJson = @"{
    ""ApiBartersQuery"": ""{ barters { level requiredItems { item { id } quantity } rewardItems { item { id } quantity } trader { normalizedName } taskUnlock { id }  }  }"",
    ""ApiItemsQuery"": ""{ items { categories { id } iconLink id imageLink link name properties { __typename ... on ItemPropertiesAmmo { accuracy ammoType armorDamage caliber damage fragmentationChance heavyBleedModifier initialSpeed lightBleedModifier penetrationChance penetrationPower projectileCount recoil ricochetChance stackMaxSize tracer } ... on ItemPropertiesArmor { class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesArmorAttachment { class durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesBackpack { capacity } ... on ItemPropertiesChestRig { capacity class durability ergoPenalty material { name } speedPenalty turnPenalty zones } ... on ItemPropertiesContainer { capacity } ... on ItemPropertiesGlasses { blindnessProtection class durability material { name } } ... on ItemPropertiesGrenade { contusionRadius fragments fuse maxExplosionDistance minExplosionDistance type } ... on ItemPropertiesHelmet { class deafening durability ergoPenalty headZones material { name } speedPenalty turnPenalty } ... on ItemPropertiesMagazine { ammoCheckModifier capacity ergonomics loadModifier malfunctionChance } ... on ItemPropertiesScope { ergonomics recoil zoomLevels } ... on ItemPropertiesWeapon { caliber ergonomics fireModes fireRate recoilHorizontal recoilVertical } ... on ItemPropertiesWeaponMod { ergonomics recoil } } shortName weight wikiLink } }"",
    ""ApiPricesQuery"": ""{ items { id buyFor { currency price priceRUB vendor { ... on TraderOffer { minTraderLevel taskUnlock { id } trader { normalizedName }  } ... on FleaMarket { normalizedName }  }  }  }  }"",
    ""ApiQuestsQuery"": ""{ tasks { id name trader { normalizedName } wikiLink } }"",
    ""ApiUrl"": ""https://api.tarkov.dev/graphql"",
    ""AzureArmorPenetrationsBlobName"": ""armor-penetrations.json"",
    ""AzureChangelogBlobName"": ""changelog.json"",
    ""AzureItemCategoriesBlobName"": ""item-categories.json"",
    ""AzurePresetsBlobName"": ""presets.json"",
    ""AzureTarkovValuesBlobName"": ""tarkov-values.json"",
    ""AzureWebsiteConfigurationBlobName"": ""website-configuration.json"",
    ""CacheDuration"": 43200,
    ""FetchTimeout"": 30,
    ""PriceCacheDuration"": 3600
}";
    }
}
