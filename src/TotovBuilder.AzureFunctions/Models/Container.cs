using TotovBuilder.AzureFunctions.Abstraction.Models;

namespace TotovBuilder.AzureFunctions.Models
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
