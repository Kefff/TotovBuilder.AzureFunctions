namespace TotovBuilder.AzureFunctions.Abstraction.Models.Items
{
    /// <summary>
    /// Provides the functionalities of a melee weapon.
    /// </summary>
    public interface IMeleeWeapon : IItem
    {
        /// <summary>
        /// Chop damage.
        /// </summary>
        double ChopDamage { get; set; }

        /// <summary>
        /// Hit radius in meters.
        /// </summary>
        double HitRadius { get; set; }

        /// <summary>
        /// Stab damage.
        /// </summary>
        double StabDamage { get; set; }
    }
}
