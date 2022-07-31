using TotovBuilder.AzureFunctions.Abstraction.Models.Items;

namespace TotovBuilder.AzureFunctions.Models.Items
{
    /// <summary>
    /// Represents an item that can contain other items.
    /// </summary>
    public class Container : Item, IContainer
    {
        /// <inheritdoc/>
        public double Capacity { get; set; }
    }
}
