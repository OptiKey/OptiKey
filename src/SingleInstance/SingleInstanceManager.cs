using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;

namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// The main class used for setting up a single instance application.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="Initialize"/> method to create and initialize a single instance manager.
    /// Note that normally only one instance of this class should be created.
    /// </remarks>
    public sealed class SingleInstanceManager : IDisposable
    {
        #region Initialization

        /// <summary>
        /// Constructs and initializes a new instance of the single instance manager.
        /// </summary>
        /// <param name="setup">Setup information for the single instance manager.</param>
        /// <returns>An initialized single instance manager.</returns>
        /// <remarks>
        /// Use this method to initialize a single instance manager with the given setup information.
        /// Note that normally only one call to this method should be performed, at the beggining of the application execution.
        /// </remarks>
        public static SingleInstanceManager Initialize(SingleInstanceManagerSetup setup)
        {
            if (setup == null)
                throw new ArgumentNullException("setup");

            // Initialize the SingleInstanceManager instance
            DeliveryStrategyFactory factory = setup.Factory ?? GetDefaultFactory();
            SingleInstanceManager instance = new SingleInstanceManager
                (GenerateUserLocalId(setup.ApplicationId), setup.ArgumentsHandler,
                factory.CreateStrategy(), setup.ArgumentsProvider, setup.ArgumentsHandlerInvoker, setup.InstanceNotificationOption,
                setup.DelivaryFailureNotification);

            if (!instance.TryEnsureFirstInstance())
            {
                // If this is not the first application instance (another instance is already running) then we need to exit/throw
                instance.Dispose();
                instance = null;

                switch (setup.TerminationOption)
                {
                    case TerminationOption.Exit:
                        Environment.Exit(setup.ExitCode);
                        break;
                    case TerminationOption.Throw:
                        throw new ApplicationInstanceAlreadyExistsException();
                    default:
                        Debug.Assert(false, "Should never be here!");
                        throw new Exception("Should never be here!");
                }
            }
            return instance;
        }

        #endregion

        #region Constructor & Fields

        private SingleInstanceManager(string applicationId, Action<string[]> argumentsHandler,
            ArgumentsDeliveryStrategy strategy, Func<string, string[]> argumentsProvider,
            IArgumentsHandlerInvoker argumentsHandlerInvoker, InstanceNotificationOption instanceNotificationOption, 
            Action<Exception> deliveryFailureNotification)
        {
            this.applicationId = applicationId;
            this.argumentsHandler = argumentsHandler;
            this.strategy = strategy;
            this.argumentsProvider = argumentsProvider ?? (appId => Environment.GetCommandLineArgs());
            this.argumentsHandlerInvoker = argumentsHandlerInvoker ?? new ThreadPoolInvoker();
            this.instanceNotificationOption = instanceNotificationOption;
            this.deliveryFailureNotification = deliveryFailureNotification;
        }

        private readonly string applicationId;
        private readonly Action<string[]> argumentsHandler;
        private readonly ArgumentsDeliveryStrategy strategy;
        private readonly Func<string, string[]> argumentsProvider;
        private readonly IArgumentsHandlerInvoker argumentsHandlerInvoker;
        private readonly InstanceNotificationOption instanceNotificationOption;
        private readonly Action<Exception> deliveryFailureNotification;

        private Mutex singleInstanceMutex;

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the instance, and releases all held resources.
        /// </summary>
        public void Dispose()
        {
            if (singleInstanceMutex != null)
            {
                singleInstanceMutex.Close();
                singleInstanceMutex = null;
            }
            strategy.Cleanup();
        }

        #endregion

        #region Helper Methods

        private static DeliveryStrategyFactory GetDefaultFactory()
        {
            return new Remoting.RemotingStrategyFactory();
        }

        private static string GenerateUserLocalId(string applicationId)
        {
            return String.Format(@"{0}:{1}", Environment.UserName, applicationId);
        }

        /// <summary>
        /// Used to check if this is the first application instance, and act accordingly.
        /// </summary>
        /// <returns>true if this is the first application instance, false otherwise.</returns>
        /// <remarks>
        /// If this is the first application instance, the argument delivery strategy is initialized to listen for incoming argument notifications.
        /// Otherwise, the argument delivery strategy is used to notify the already running application instance.
        /// </remarks>
        private bool TryEnsureFirstInstance()
        {
            if (IsFirstApplicationInstance())
            {
                strategy.InitializeFirstInstance(applicationId, NotifyArgumentsReceived);
                return true;
            }
            else
            {
                try
                {
                    // There is another instance running and we must notify it
                    // with the command line arguments.
                    strategy.DeliverArgumentsToFirstInstance(applicationId, argumentsProvider(applicationId));
                }
                catch (Exception ex)
                {
                    if (deliveryFailureNotification != null)
                        deliveryFailureNotification(ex);
                }
                return false;
            }
        }

        /// <summary>
        /// Checks if an application instance was already created, and marks this as the first instance if it is not.
        /// </summary>
        /// <returns>true if this is the first application instance, false otherwise.</returns>
        private bool IsFirstApplicationInstance()
        {
            Debug.Assert(singleInstanceMutex == null);

            bool createdNew;
            singleInstanceMutex = new Mutex(true, applicationId, out createdNew);
            return createdNew;
        }

        /// <summary>
        /// Notifies the registered handler of the arguments recieved.
        /// </summary>
        /// <param name="args">The command line arguments recieved.</param>
        /// <param name="remoteIsAdmin">Indicates whether the remote party is an administrator.</param>
        /// <remarks>
        /// Note that notification is performed according to the <see cref="InstanceNotificationOption"/> given in the manager setup. 
        /// In addition, note that the registered handler is invoked using the handler invoker supplied upon <see cref="Initialize">initializtion</see>.
        /// </remarks>
        private void NotifyArgumentsReceived(string[] args, bool remoteIsAdmin)
        {
            if (instanceNotificationOption == InstanceNotificationOption.NotifyOnlyIfAdmin)
            {
                bool localIsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
                if (localIsAdmin && !remoteIsAdmin)
                    throw new InvalidOperationException("The remote party should posses administrative rights to contact this application instance");
            }

            if (argumentsHandler != null)
                argumentsHandlerInvoker.Invoke(argumentsHandler, args);
        }

        #endregion
    }
}
