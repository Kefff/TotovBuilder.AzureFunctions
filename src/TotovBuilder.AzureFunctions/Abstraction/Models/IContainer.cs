namespace TotovBuilder.AzureFunctions.Abstraction.Models
{
    /// <summary>
    /// Provides the functionalities of an item that can contain other items.
    /// </summary>
    public interface IContainer : IItem
    {
        /// <summary>
        /// Capacity.
        /// </summary>
        double Capacity { get; set; }
    }
}
