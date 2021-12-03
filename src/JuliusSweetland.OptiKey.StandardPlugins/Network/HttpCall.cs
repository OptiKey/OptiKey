// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Net.Http;
using System.Text;

/**
 * This is a plugin that calls a HTTP request upon key selection.
 * 
 * It has the following methods:
 * 
 * 1) GET: This method calls a HTTP/HTTPS uri using GET HTTP method. Arguments are:
 *    . uri: the uri that will be called
 *    . accept: The value for 'Accept' HTTP header
 *    . authorization: The value for 'Authorization' HTTP header
 *    . timeout: How many miliseconds the plugin will wait for answer when calling the provided uri. Default value is 3s
 * 2) POST: This method calls a HTTP/HTTPS uri ugin POST HTTP method. Arguments are:
 *    . uri: the uri that will be called
 *    . accepts: The value for 'Accept' HTTP header
 *    . authorization: The value for 'Authorization' HTTP header
 *    . timeout: How many miliseconds the plugin will wait for answer when calling the provided uri. Default value is 3s
 *    . contentType: The value for 'ContentType' HTTP header
 *    . payload: The Payload that will be posted
 * 
 * Please refer to OptiKey wiki for more information on registering and developing extensions.
 */

// TODO: Add Proxy Support
namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class HttpCall
    {
        // GET HTTP method request
        public void GET(string uri, string accept, string authorization, string timeout)
        {
            HttpClient client = new HttpClient();
            client = Configure(client, accept, authorization, timeout);
            HttpResponseMessage response = client.GetAsync(uri).Result;
            response.EnsureSuccessStatusCode();
        }

        // POST HTTP method request
        public void POST(string uri, string accept, string authorization, string timeout, string contentType, string payload)
        {
            HttpClient client = new HttpClient();
            client = Configure(client, accept, authorization, timeout);
            HttpResponseMessage response = client.PostAsync(uri, new StringContent(payload, Encoding.UTF8, contentType)).Result;
            response.EnsureSuccessStatusCode();
        }

        private HttpClient Configure(HttpClient client, string accept, string authorization, string timeout)
        {
            if (!string.IsNullOrWhiteSpace(accept))
            {
                client.DefaultRequestHeaders.Add("Accept", accept);
            }
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                client.DefaultRequestHeaders.Add("Authorization", authorization);
            }

            // Default timeout is 3s
            int iTimeout = 3000;
            if (!string.IsNullOrWhiteSpace(timeout))
            {
                iTimeout = Int32.Parse(timeout);
            }

            client.Timeout = TimeSpan.FromMilliseconds(iTimeout);

            return client;
        }
    }
}
