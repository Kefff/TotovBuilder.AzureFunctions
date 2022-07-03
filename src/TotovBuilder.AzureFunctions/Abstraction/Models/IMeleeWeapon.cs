namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a melee weapon.
    /// </summary>
    public interface IMeleeWeapon: IItem
    {
        /// <summary>
        /// Chop damage.
        /// </summary>
        public double ChopDamage { get; set; }

        /// <summary>
        /// Hit radius in meters.
        /// </summary>
        public double HitRadius { get; set; }

        /// <summary>
        /// Stab damage.
        /// </summary>
        public double StabDamage { get; set; }
    }
}
