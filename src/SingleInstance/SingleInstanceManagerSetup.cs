using System;

namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// Used to pass initialization data to an initialized instance of the <see cref="SingleInstanceManager"/>.
    /// </summary>
    public sealed class SingleInstanceManagerSetup
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="SingleInstanceManagerSetup"/> class.
        /// </summary>
        /// <param name="applicationId">The application ID which is used to identify the application as a single instance.</param>
        public SingleInstanceManagerSetup(string applicationId)
        {
            ApplicationId = applicationId;
            TerminationOption = TerminationOption.Exit;
            //InstanceNotificationOption = InstanceNotificationOption.NotifyAnyway;
            InstanceNotificationOption = InstanceNotificationOption.NotifyOnlyIfAdmin;
        }

        private string applicationId;

        /// <summary>
        /// The application ID which is used to identify the application as a single instance.
        /// </summary>
        public string ApplicationId
        {
            get { return applicationId; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("applicationId");
                if (value == String.Empty)
                    throw new ArgumentException("The application ID cannot be an empty string");

                applicationId = value;
            }
        }
        
        /// <summary>
        /// The handler invoked when command line arguments are recieved from a remote instance.
        /// </summary>
        /// <remarks>
        /// The hadnler is invoked using the invoker configure via <see cref="ArgumentsHandlerInvoker"/>.
        /// </remarks>
        public Action<string[]> ArgumentsHandler
        {
            get;
            set;
        }

        /// <summary>
        /// The action to perform when trying to initalize a <see cref="SingleInstanceManager"/>
        /// which is not the first application instance.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="WindowsRecipes.TaskbarSingleInstance.TerminationOption.Exit"/>. When <see cref="WindowsRecipes.TaskbarSingleInstance.TerminationOption.Exit"/> is set,
        /// the value configured in <see cref="ExitCode"/> is used as the exit code.
        /// </remarks>
        public TerminationOption TerminationOption
        {
            get;
            set;
        }
        
        /// <summary>
        /// The exit code to be used if this is not the first application instance, and <see cref="WindowsRecipes.TaskbarSingleInstance.TerminationOption.Exit"/>
        /// is specified for <see cref="TerminationOption"/>.
        /// </summary>
        public int ExitCode
        {
            get;
            set;
        }

        /// <summary>
        /// A custom provider for the current process' command line arguments.
        /// </summary>
        /// <remarks>
        /// By default <see cref="Environment.GetCommandLineArgs"/> is used.
        /// </remarks>
        public Func<string, string[]> ArgumentsProvider
        {
            get;
            set;
        }

        /// <summary>
        /// A custom provider for the command line arguments delivery strategy.
        /// </summary>
        /// <remarks>
        /// By default <see cref="Remoting.RemotingStrategyFactory"/> is used.
        /// </remarks>
        public DeliveryStrategyFactory Factory
        {
            get;
            set;
        }

        /// <summary>
        /// A custom arguments handler invoker.
        /// </summary>
        /// <remarks>
        /// This property determines how the host application gets notified of incoming command line arguments from other instances.
        /// By default <see cref="ThreadPoolInvoker"/> is used. For specific application types (e.g. WinForms &amp; WPF), use appropriate custom invokers.
        /// </remarks>
        public IArgumentsHandlerInvoker ArgumentsHandlerInvoker
        {
            get;
            set;
        }

        /// <summary>
        /// Used to set the behavior of the <see cref="SingleInstanceManager"/> when the first application instance is run as an admin.
        /// </summary>
        /// <remarks>
        /// If the first application instance is not run as an administrator, then setting this setting has no effect.
        /// The default value is <see cref="WindowsRecipes.TaskbarSingleInstance.InstanceNotificationOption.NotifyAnyway"/>.
        /// </remarks>
        public InstanceNotificationOption InstanceNotificationOption
        {
            get;
            set;
        }

        /// <summary>
        /// Specify a handler to be called when delivery of command line arguments to the first instance has failed for some reason.
        /// </summary>
        /// <remarks>
        /// A common reason for failure might be that the <see cref="SingleInstanceManager"/> is run with 
        /// <see cref="WindowsRecipes.TaskbarSingleInstance.InstanceNotificationOption.NotifyOnlyIfAdmin"/> and this instance doesn't have administrative priviliges.
        /// </remarks>
        public Action<Exception> DelivaryFailureNotification 
        { 
            get; 
            set; 
        }
    }
}