namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a ranged weapon mod.
    /// <summary>
    public interface IRangedWeaponMod: IMod
    {
        /// <summary>
        /// Modifier added to the weapon accuracy in percentage.
        /// <summary>
        double AccuracyPercentageModifier { get; set; }

        /// <summary>
        /// Modifier added to the weapon recoil in percentage.
        /// <summary>
        double RecoilPercentageModifier { get; set; }
    }
}
