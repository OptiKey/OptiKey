using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace JuliusSweetland.OptiKey.Services
{
    public class WebApiServiceHost : ServiceHost
    {
        public WebApiServiceHost(ISuggestionStateService suggestionService, params Uri[] baseAddresses)
            : base(typeof(WebApiService), baseAddresses)
        {
            if (suggestionService == null)
            {
                throw new ArgumentNullException(nameof(suggestionService));
            }

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new WcfEndpointServiceInstanceProvider(suggestionService));
            }
        }
    }

    public class WcfEndpointServiceInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly ISuggestionStateService suggestionService;

        public WcfEndpointServiceInstanceProvider(ISuggestionStateService suggestionService)
        {
            if (suggestionService == null)
            {
                throw new ArgumentNullException(nameof(suggestionService));
            }

            this.suggestionService = suggestionService;
        }

        #region IInstanceProvider Members

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new WebApiService(this.suggestionService);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        #endregion

        #region IContractBehavior Members

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
