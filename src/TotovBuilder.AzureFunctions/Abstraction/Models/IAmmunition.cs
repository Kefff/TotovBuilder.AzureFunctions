namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of ammunition.
    /// </summary>
    public interface IAmmunition : IItem
    {
        /// <summary>
        /// Modifier added to the weapon accuracy in percentage.
        /// </summary>
        double AccuracyPercentageModifier { get; set; }

        /// <summary>
        /// Armor damage percentage.
        /// </summary>
        double ArmorDamagePercentage { get; set; }

        /// <summary>
        /// List of the penetration effectiveness by armor class.
        /// </summary>
        double[] ArmorPenetrations { get; set; }

        /// <summary>
        /// Indicates whether the ammunitions can blind opponents.
        /// </summary>
        bool Blinding { get; set; }

        /// <summary>
        /// Caliber.
        /// </summary>
        string Caliber { get; set; }

        /// <summary>
        /// Durability burn percentage modifier.
        /// </summary>
        double DurabilityBurnPercentageModifier { get; set; }

        /// <summary>
        /// Damage done the the body when penetrating armor.
        /// </summary>
        double FleshDamage { get; set; }

        /// <summary>
        /// Percentage of chance to fragment and inflict additional damage to the body.
        /// </summary>
        double FragmentationChancePercentage { get; set; }

        /// <summary>
        /// Percentage of chance to inflict a heavy bleeding when hitting flesh.
        /// </summary>
        double HeavyBleedingPercentageChance { get; set; }

        /// <summary>
        /// Percentage of chance to inflict a light bleeding when hitting flesh.
        /// </summary>
        double LightBleedingPercentageChance { get; set; }

        /// <summary>
        /// Armor penetration power.
        /// </summary>
        double PenetrationPower { get; set; }

        /// <summary>
        /// Number of projectiles.
        /// Usually 1 except for shotgun buckshot ammunition which fires multiple pellets.
        /// </summary>
        double Projectiles { get; set; }

        /// <summary>
        /// Modifier added to the weapon recoil in percentage.
        /// </summary>
        double RecoilPercentageModifier { get; set; }

        /// <summary>
        /// Indicates whether the ammunition is subsonic or not.
        /// </summary>
        bool Subsonic { get; set; }

        /// <summary>
        /// Indicates whether the ammunition is will have a coloured trail while flying.
        /// </summary>
        bool Tracer { get; set; }

        /// <summary>
        /// Bullet velocity.
        /// </summary>
        double Velocity { get; set; }
    }
}
