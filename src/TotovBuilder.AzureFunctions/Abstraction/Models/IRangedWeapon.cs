namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a ranged weapon.
    /// <summary>
    public interface IRangedWeapon: IModdable
    {
        /// <summary>
        /// Caliber.
        /// <summary>
        public string Caliber { get; set; }

        /// <summary>
        /// Ergonomics.
        /// Influences amongst other things the weapon sway, the amount of time the weapon can be held aiming
        /// and the noise the weapon makes when aiming.
        /// <summary>
        public double Ergonomics { get; set; }

        /// <summary>
        /// Fire rate in bullets per second.
        /// <summary>
        public double FireRate { get; set; }

        /// <summary>
        /// Horizontal recoil.
        /// <summary>
        public double HorizontalRecoil { get; set; }

        /// <summary>
        /// Vertical recoil.
        /// <summary>
        public double VerticalRecoil { get; set; }
    }
}
