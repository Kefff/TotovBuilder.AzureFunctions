using TotovBuilder.AzureFunctions.Models.Items;

namespace TotovBuilder.AzureFunctions.Abstraction.Models.Items
{
    /// <summary>
    /// Provides the functionalities of a moddable item.
    /// </summary>
    public interface IModdable : IItem
    {
        /// <summary>
        /// Mod slots.
        /// </summary>
        ModSlot[] ModSlots { get; set; }
    }
}
