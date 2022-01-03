using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace WebHttpCors
{
    public class CorsWebServiceHostFactory : WebServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var host = base.CreateServiceHost(serviceType, baseAddresses);
            host.Opening += host_Opening;
            return host;
        }

        void host_Opening(object sender, EventArgs e)
        {
            var endpoints = (sender as ServiceHost).Description.Endpoints.Where(se => se.Binding is WebHttpBinding);

            foreach (var endpoint in endpoints)
            {
                // Add support for cross-origin resource sharing
                endpoint.Behaviors.Add(new CorsSupportBehavior());
            }
        }
    }
}