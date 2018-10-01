using System.ServiceModel;
using System.ServiceModel.Web;

namespace JuliusSweetland.OptiKey.Services
{
    [ServiceContract]
    public interface IWebApiService
    {
        //[OperationContract]
        //[WebGet]
        //string GetSuggestions(string s);

        //PostMan sample - call this method with POST to http://localhost:8733/OptiKey/suggestions
        //Header: Content-Type:[{"key":"Content-Type","value":"application/json","description":"","enabled":true}]
        //Body (raw): {"suggestions":["value1","value2"]}
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "suggestions",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SetSuggestions(string[] suggestions);
    }
}
