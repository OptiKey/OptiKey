using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;
using System.Threading;

namespace WindowsRecipes.TaskbarSingleInstance.Remoting
{
    /// <summary>
    /// An implementation of an <see cref="ArgumentsDeliveryStrategy"/> using .NET IPC remoting as the communication mechanism.
    /// </summary>
    public sealed class RemotingStrategy : ArgumentsDeliveryStrategy
    {
        //Marked as internal since only the factory needs to create this class
        internal RemotingStrategy()
        {
        }

        /// <summary>
        /// Remote service name.
        /// </summary>
        private const string RemoteServiceName = "SingleInstanceApplicationService";

        /// <summary>
        /// IPC channel for communications.
        /// </summary>
        private IpcServerChannel channel;

        /// <summary>
        /// Initalize a listener for incoming command line arguments, as the first application instance, using a remoting IPC service.
        /// </summary>
        /// <param name="applicationId">The application ID which identifies the application as a single instance.</param>
        protected override void OnInitializeFirstInstance(string applicationId)
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Dictionary<string, string>();

            props["name"] = applicationId;
            props["portName"] = applicationId;
            props["exclusiveAddressUse"] = "false";

            // Create the IPC Server channel with the channel properties
            channel = new IpcServerChannel(props, serverProvider);

            // Register the channel with the channel services
            ChannelServices.RegisterChannel(channel, true);

            // Expose the remote service with the "RemoteServiceName"
            IPCRemoteService remoteService = new IPCRemoteService(this);
            RemotingServices.Marshal(remoteService, RemoteServiceName);
        }

        /// <summary>
        /// Delivers the command line arguments to the first application instance, via a .NET remoting call to a remote service.
        /// </summary>
        /// <param name="applicationId">The application ID which identifies the application as a single instance.</param>
        /// <param name="args">The command line arguments to deliver.</param>
        protected override void OnDeliverArgumentsToFirstInstance(string applicationId, string[] args)
        {
            IpcClientChannel secondInstanceChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(secondInstanceChannel, true);

            string remotingServiceUrl = String.Format("ipc://{0}/{1}", applicationId, RemoteServiceName);

            // Obtain a reference to the remoting service exposed by the server i.e the first instance of the application
            IPCRemoteService firstInstanceRemoteServiceReference = (IPCRemoteService)RemotingServices.Connect(typeof(IPCRemoteService), remotingServiceUrl);

            // Check that the remote service exists, in some cases the first instance may not yet have created one, in which case
            // the second instance should just exit
            if (firstInstanceRemoteServiceReference != null)
            {
                // Invoke a method of the remote service exposed by the first instance passing on the command line
                // arguments and causing the first instance to activate itself
                firstInstanceRemoteServiceReference.DeliverArguments(args);
            }
        }

        /// <summary>
        /// Cleanup unneeded resources upon disposal.
        /// </summary>
        protected override void OnCleanup()
        {
            if (channel != null)
            {
                ChannelServices.UnregisterChannel(channel);
                channel = null;
            }

            base.OnCleanup();
        }

        #region Private Classes

        private class IPCRemoteService : MarshalByRefObject
        {
            private readonly RemotingStrategy strategy;

            public IPCRemoteService(RemotingStrategy strategy)
            {
                this.strategy = strategy;
            }

            public void DeliverArguments(string[] args)
            {
                bool remoteIsAdmin = new WindowsPrincipal((WindowsIdentity)Thread.CurrentPrincipal.Identity).IsInRole(WindowsBuiltInRole.Administrator);
                strategy.NotifyArgumentsReceived(args, remoteIsAdmin);
            }

            /// <summary>
            /// Remoting Object's ease expires after every 5 minutes by default. We need to override the InitializeLifetimeService class
            /// to ensure that lease never expires.
            /// </summary>
            /// <returns>Always null.</returns>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }

        #endregion
    }
}