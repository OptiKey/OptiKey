using System;

namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// The base class for implementing a strategy to recieve and deliver command line arguments from/to other application instances.
    /// </summary>
    /// <remarks>
    /// Inherit this class to implement your own command line arguments notification strategy. 
    /// When this is the first application instance, the strategy is used to recieve notifications of command line arguments from new application instances.
    /// When this is not the first application instance, the strategy is used to deliver the command line arguments to the first application instance.
    /// </remarks>
    public abstract class ArgumentsDeliveryStrategy
    {
        #region Protected Members

        /// <summary>
        /// Override this method to initalize a listener to incoming command line arguments, as the first application instance.
        /// </summary>
        /// <param name="applicationId">The application ID which identifies the application as a single instance.</param>
        protected abstract void OnInitializeFirstInstance(string applicationId);

        /// <summary>
        /// Override this method to deliver the command line arguments to the first application instance.
        /// </summary>
        /// <param name="applicationId">The application ID which identifies the application as a single instance.</param>
        /// <param name="args">The command line arguments to deliver.</param>
        /// <remarks>
        /// If the delivery fails, an appropriate excption should be thrown.
        /// </remarks>
        protected abstract void OnDeliverArgumentsToFirstInstance(string applicationId, string[] args);

        /// <summary>
        /// Override this method to cleanup unneeded resources upon disposal.
        /// </summary>
        protected virtual void OnCleanup() { }

        /// <summary>
        /// Derived classes should call this method to notify the application instance of incoming command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments recieved.</param>
        /// <param name="remoteIsAdmin">true if the remote party has admin priviliges, false otherwise.</param>
        protected void NotifyArgumentsReceived(string[] args, bool remoteIsAdmin)
        {
            Action<string[], bool> localArgsNotification = argsNotification;  // Create local copy to avoid concurrency issues on cleanup
            if (localArgsNotification != null)
                localArgsNotification(args, remoteIsAdmin);
        }

        #endregion

        #region Internal Members

        internal void InitializeFirstInstance(string applicationId, Action<string[], bool> argsNotification)
        {
            this.argsNotification = argsNotification;
            OnInitializeFirstInstance(applicationId);
        }

        internal void DeliverArgumentsToFirstInstance(string applicationId, string[] args)
        {
            OnDeliverArgumentsToFirstInstance(applicationId, args);
        }

        internal void Cleanup()
        {
            OnCleanup();
            argsNotification = null;
        }

        #endregion

        private Action<string[], bool> argsNotification;
    }
}
