
namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// Represents the action to perform when trying to initalize a <see cref="SingleInstanceManager"/>
    /// which is not the first application instance.
    /// </summary>
    public enum TerminationOption
    {
        /// <summary>
        /// Exit the application using <see cref="System.Environment.Exit"/>.
        /// </summary>
        Exit,
        /// <summary>
        /// Throw an <see cref="ApplicationInstanceAlreadyExistsException"/>.
        /// </summary>
        Throw
    }
}