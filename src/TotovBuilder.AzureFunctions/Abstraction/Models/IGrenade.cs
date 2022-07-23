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
        double ExplosionDelay { get; set; }

        /// <summary>
        /// Type of ammunition of the fragments.
        /// </summary>
        string FragmentAmmunitionId { get; set; }

        /// <summary>
        /// Number of fragments.
        /// </summary>
        double FragmentsAmount { get; set; }

        /// <summary>
        /// Maximum explosion range in meters.
        /// </summary>
        double MaximumExplosionRange { get; set; }

        /// <summary>
        /// Minimum explosion range in meters.
        /// </summary>
        double MinimumExplosionRange { get; set; }

        /// <summary>
        /// Type of grenade.
        /// </summary>
        string Type { get; set; }
    }
}
