using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WebHttpCors
{
	// Based on part of the Cross-Origin Resource Sharing documentation: http://www.w3.org/TR/cors/

	public class CorsSupportBehavior : IEndpointBehavior
	{
		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{

		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			// Register a message inspector, and an operation invoker for undhandled operations
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CorsMessageInspector());

			IOperationInvoker invoker = endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker;
			endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation.Invoker =
				new CustomOperationInvoker(invoker);
		}

		public void Validate(ServiceEndpoint endpoint)
		{
			// Make sure that the behavior is applied to an endpoing with WebHttp binding
			if (!(endpoint.Binding is WebHttpBinding))
				throw new InvalidOperationException("The CorsSupport behavior can only be used in WebHttpBinding endpoints");
		}
	}
}