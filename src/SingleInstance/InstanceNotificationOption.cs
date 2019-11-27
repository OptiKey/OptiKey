
namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// Used to set the behavior of the <see cref="SingleInstanceManager"/> when the first application instance is run as an admin.
    /// </summary>
    /// <remarks>
    /// If the first application instance is not run as an administrator, then setting this setting has no effect.
    /// </remarks>
    public enum InstanceNotificationOption
    {
        /// <summary>
        /// Always allow the first application instance to be notified of incoming arguments, regardless of whether these
        /// arguments originate from an admin or non-admin instance.
        /// </summary>
        NotifyAnyway,
        /// <summary>
        /// Only allows the first (admin) application instance to be notified of incoming arguments, if these arguments
        /// originate from an admin instance.
        /// </summary>
        NotifyOnlyIfAdmin
    }
}