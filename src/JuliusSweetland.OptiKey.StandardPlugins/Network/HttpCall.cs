using System.Net;
using System.Speech.Synthesis;

/**
 * This is a plugin that calls a HTTP request upon key selection.
 * 
 * It has the following methods:
 * 
 * 1) GET: This method calls a HTTP/HTTPS url using GET HTTP method. Arguments are:
 *    . URL: the url that will be called
 *    . Accepts: The value for 'Accept' HTTP header
 *    . Authorization: The value for 'Authorization' HTTP header
 * 2) POST: This method calls a HTTP/HTTPS url ugin POST HTTP method. Arguments are:
 *    . URL: the url that will be called
 *    . Accepts: The value for 'Accept' HTTP header
 *    . Authorization: The value for 'Authorization' HTTP header
 *    . ContentType: The value for 'ContentType' HTTP header
 *    . Payload: The Payload that will be posted
 * 
 * Please refer to OptiKey wiki for more information on registering and developing extensions.
 */

// TODO: Add Proxy Support
namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class HttpCall
    {
        // Getters for internal OptiKey ID, name and description
        public string GetPluginId() => "HttpCall";
        public string GetPluginName() => "HTTP/HTTPS Call Plugin";
        public string GetPluginDescription() => "This plugins enables OptiKey to call a external URL every time a plugin key is pressed";

        // GET HTTP method request
        public void GET(string url, string accepts, string authorization)
        {
            WebRequest webRequest = HandleHeaders(WebRequest.Create(url), accepts, authorization, null);
            HttpWebResponse webResp = (HttpWebResponse)webRequest.GetResponse();
            //TODO CHeck HTTP response code
            //if (webResp.StatusCode.)
        }

        // POST HTTP method request
        public void POST(string url, string accepts, string authorization, string contentType, string payload)
        {
            //TODO
        }

        private WebRequest HandleHeaders(WebRequest webRequest, string accepts, string authorization, string contentType)
        {
            if (!string.IsNullOrWhiteSpace(accepts))
            {
                webRequest.Headers.Add("Accepts", accepts);
            }
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                webRequest.Headers.Add("Authorization", authorization);
            }
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                webRequest.ContentType = contentType;
            }
            return webRequest;
        }
    }
}
