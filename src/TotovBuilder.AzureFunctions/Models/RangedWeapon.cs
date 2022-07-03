﻿using System;
using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
{
    /// <summary>
    /// Represents a ranged weapon.
    /// </summary>
    public class RangedWeapon : Item, IRangedWeapon
    {
        /// <inheritdoc/>
        public string Caliber { get; set; } = string.Empty;
        
        /// <inheritdoc/>
        public double Ergonomics { get; set; }
        
        /// <inheritdoc/>
        public double FireRate { get; set; }
        
        /// <inheritdoc/>
        public double HorizontalRecoil { get; set; }
        
        /// <inheritdoc/>
        public double VerticalRecoil { get; set; }

        /// <inheritdoc/>
        public IModSlot[] ModSlots { get; set; } = Array.Empty<IModSlot>();
    }
}