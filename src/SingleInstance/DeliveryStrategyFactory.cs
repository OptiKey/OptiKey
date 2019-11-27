
namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// The base class for a command line arguments delivery strategy factory.
    /// </summary>
    /// <remarks>
    /// Derive this class to enable the <see cref="SingleInstanceManager"/> to use a custom delivery strategy.
    /// </remarks>
    public abstract class DeliveryStrategyFactory
    {
        /// <summary>
        /// Override this method to return a custom <see cref="ArgumentsDeliveryStrategy"/>.
        /// </summary>
        /// <returns>A custom <see cref="ArgumentsDeliveryStrategy"/>.</returns>
        protected internal abstract ArgumentsDeliveryStrategy CreateStrategy();
    }
}
