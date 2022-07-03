using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents ammunition.
    /// </summary>
    public class Ammunition : Item, IAmmunition
    {
        /// <inheritdoc/>
        public double AccuracyPercentageModifier { get; set; }

        /// <inheritdoc/>
        public double ArmorDamagePercentage { get; set; }

        /// <inheritdoc/>
        public double[] ArmorPenetrations { get; set; } = Array.Empty<double>();

        /// <inheritdoc/>
        public bool Blinding { get; set; }

        /// <inheritdoc/>
        public string Caliber { get; set; } = string.Empty;

        /// <inheritdoc/>
        public double DurabilityBurnPercentageModifier { get; set; }

        /// <inheritdoc/>
        public double FleshDamage { get; set; }

        /// <inheritdoc/>
        public double FragmentationChancePercentage { get; set; }

        /// <inheritdoc/>
        public double HeavyBleedingPercentageChance { get; set; }

        /// <inheritdoc/>
        public double LightBleedingPercentageChance { get; set; }

        /// <inheritdoc/>
        public double PenetrationPower { get; set; }

        /// <inheritdoc/>
        public double Projectiles { get; set; }

        /// <inheritdoc/>
        public double RecoilPercentageModifier { get; set; }

        /// <inheritdoc/>
        public bool Subsonic { get; set; }

        /// <inheritdoc/>
        public bool Tracer { get; set; }

        /// <inheritdoc/>
        public double Velocity { get; set; }
    }
}
