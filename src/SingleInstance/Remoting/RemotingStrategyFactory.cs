namespace WindowsRecipes.TaskbarSingleInstance.Remoting
{
    /// <summary>
    /// A factory class for creating an <see cref="ArgumentsDeliveryStrategy"/> of type <see cref="RemotingStrategy"/>.
    /// </summary>
    public class RemotingStrategyFactory : DeliveryStrategyFactory
    {
        /// <summary>
        /// Constructs and returnes a new instance of <see cref="RemotingStrategy"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="RemotingStrategy"/>.</returns>
        protected internal override ArgumentsDeliveryStrategy CreateStrategy()
        {
            return new RemotingStrategy();
        }
    }
}
