using System.ServiceModel;

namespace WebHttpCors
{
    public class PreflightDetected : IExtension<OperationContext>
    {
        string _requestedHeaders;

        public PreflightDetected(string requestedHeaders)
        {
            RequestedHeaders = requestedHeaders;
        }

        public string RequestedHeaders
        {
            get
            {
                return _requestedHeaders ?? string.Empty;
            }
            set
            {
                _requestedHeaders = value;
            }
        }

        public void Attach(OperationContext owner)
        {
        }

        public void Detach(OperationContext owner)
        {
        }
    }
}