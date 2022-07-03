namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of a mod slot.
    /// </summary>
    public interface IModSlot
    {
        /// <summary>
        /// IDs of compatible items.
        /// </summary>
        public string[] CompatibleItemIds { get; set; }

        /// <summary>
        /// ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Maximum number of times the item can be stacked in this mod slot.
        /// Mainly used to force the ammunition quantity to 1 in the special chamber mod slot.
        /// </summary>
        public double MaxStackableAmount { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines whether having an item in the mod slot is required for the parent item to be usable.
        /// </summary>
        public bool Required { get; set; }
    }
}
