﻿namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a magazine.
    /// </summary>
    public interface IMagazine: IContainer, IMod
    {
        /// <summary>
        /// IDs of accepted ammunition.
        /// </summary>
        public string[] AcceptedAmmunitionIds { get; set; }

        /// <summary>
        /// Modifier added to the check speed in percentage.
        /// </summary>
        public double CheckSpeedPercentageModifier { get; set; }

        /// <summary>
        /// Modifier added to the loading speed in percentage.
        /// </summary>
        public double LoadSpeedPercentageModifier { get; set; }
    }
}
