using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace WebHttpCors
{
    public class CorsMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var httpRequest = request.Properties["httpRequest"] as HttpRequestMessageProperty;

            // Check if the client sent an "OPTIONS" request
            if (httpRequest == null ||
                httpRequest.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                return null;

            // Store the requested headers
            OperationContext.Current.Extensions.Add(new PreflightDetected(
                httpRequest.Headers["Access-Control-Request-Headers"]));
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            HttpResponseMessageProperty property;

            if (reply == null)
            {
                // This will usually be for a preflight response
                reply = Message.CreateMessage(MessageVersion.None, null);
                property = new HttpResponseMessageProperty();
                reply.Properties[HttpResponseMessageProperty.Name] = property;
                property.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                property = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            }

            var preflightRequest = OperationContext.Current.Extensions.Find<PreflightDetected>();
            if (preflightRequest != null)
            {
                // Add allow HTTP headers to respond to the preflight request
                if (preflightRequest.RequestedHeaders == string.Empty)
                    property.Headers.Add("Access-Control-Allow-Headers", "Accept");
                else
                    property.Headers.Add("Access-Control-Allow-Headers", preflightRequest.RequestedHeaders + ", Accept");

                property.Headers.Add("Access-Control-Allow-Methods", "POST, PUT, GET, OPTIONS, DELETE");

            }

            // Add allow-origin header to each response message, because client expects it
            property.Headers.Add("Access-Control-Allow-Origin", "*");

            // the browser will not make pre-flight requests everytime.
            property.Headers.Add("Access-Control-Max-Age", "1728000");
        }
    }
}