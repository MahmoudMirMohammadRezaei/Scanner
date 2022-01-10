using System.ServiceModel;
using System.ServiceModel.Web;

namespace SelfHost
{
    [ServiceContract]
    public interface IScannerService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
           BodyStyle = WebMessageBodyStyle.Wrapped,
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           UriTemplate = "GetScan")]
        //public string GetScan();
        string GetScan(string logPath);
    }
}
