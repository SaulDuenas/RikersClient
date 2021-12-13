using RikersProxy.Entities;
using System;
using System.Threading.Tasks;
using static RikersProxy.ClientProxyAsync;

namespace RikersProxy
{
    public interface IClientProxyAsync
    {
        string BaseUrl { get; set; }
        string Credentials { get; set; }
        string SerialCert { get; set; }
        string EndPointGetToken { get; set; }
        string EndPointCreateCase { get; set; }
        string EndPointSendComment { get; set; }

        TimeSpan Elapsed_Time_Token { get; }
        Token Token { get; set; }

        Task<ProxyResult> CreateCase(CaseData data);
        Task<ProxyResult> ObtainToken();
    }
}