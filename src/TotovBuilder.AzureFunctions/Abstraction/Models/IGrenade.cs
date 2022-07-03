namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a grenade.
    /// </summary>
    public interface IGrenade: IItem
    {
        /// <summary>
        /// Delay before explosion in seconds.
        /// </summary>
        public double ExplosionDelay { get; set; }

        /// <summary>
        /// Type of ammunition of the fragments.
        /// </summary>
        public string FragmentAmmunitionId { get; set; }

        /// <summary>
        /// Number of fragments.
        /// </summary>
        public double FragmentsAmount { get; set; }

        /// <summary>
        /// Maximum explosion range in meters.
        /// </summary>
        public double MaximumExplosionRange { get; set; }

        /// <summary>
        /// Minimum explosion range in meters.
        /// </summary>
        public double MinimumExplosionRange { get; set; }
    }
}
