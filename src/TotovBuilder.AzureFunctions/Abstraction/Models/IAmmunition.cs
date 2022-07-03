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
        public double AccuracyPercentageModifier { get; set; }

        /// <summary>
        /// Armor damage percentage.
        /// </summary>
        public double ArmorDamagePercentage { get; set; }

        /// <summary>
        /// List of the penetration effectiveness by armor class.
        /// </summary>
        public double[] ArmorPenetrations { get; set; }

        /// <summary>
        /// Indicates whether the ammunitions can blind opponents.
        /// </summary>
        public bool Blinding { get; set; }

        /// <summary>
        /// Caliber.
        /// </summary>
        public string Caliber { get; set; }

        /// <summary>
        /// Durability burn percentage modifier.
        /// </summary>
        public double DurabilityBurnPercentageModifier { get; set; }

        /// <summary>
        /// Damage done the the body when penetrating armor.
        /// </summary>
        public double FleshDamage { get; set; }

        /// <summary>
        /// Percentage of chance to fragment and inflict additional damage to the body.
        /// </summary>
        public double FragmentationChancePercentage { get; set; }

        /// <summary>
        /// Percentage of chance to inflict a heavy bleeding when hitting flesh.
        /// </summary>
        public double HeavyBleedingPercentageChance { get; set; }

        /// <summary>
        /// Percentage of chance to inflict a light bleeding when hitting flesh.
        /// </summary>
        public double LightBleedingPercentageChance { get; set; }

        /// <summary>
        /// Armor penetration power.
        /// </summary>
        public double PenetrationPower { get; set; }

        /// <summary>
        /// Number of projectiles.
        /// Usually 1 except for shotgun buckshot ammunition which fires multiple pellets.
        /// </summary>
        public double Projectiles { get; set; }

        /// <summary>
        /// Modifier added to the weapon recoil in percentage.
        /// </summary>
        public double RecoilPercentageModifier { get; set; }

        /// <summary>
        /// Indicates whether the ammunition is subsonic or not.
        /// </summary>
        public bool Subsonic { get; set; }

        /// <summary>
        /// Indicates whether the ammunition is will have a coloured trail while flying.
        /// </summary>
        public bool Tracer { get; set; }

        /// <summary>
        /// Bullet velocity.
        /// </summary>
        public double Velocity { get; set; }
    }
}
