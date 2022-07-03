namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a moddable item.
    /// </summary>
    public  interface IModdable : IItem
    {
        /// <summary>
        /// Mod slots.
        /// </summary>
        public IModSlot[] ModSlots { get; set; }
    }
}
