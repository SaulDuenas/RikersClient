using RikersProxy.Entities;
using System;
using System.Threading.Tasks;
using static RikersProxy.ClientProxy;

namespace RikersProxy
{
    public interface IClientProxy
    {
        string BaseUrl { get; set; }
        string Credentials { get; set; }
        string SerialCert { get; set; }
        string EndPointGetToken { get; set; }
        string EndPointCreateCase { get; set; }
        string EndPointFeedBack { get; set; }

        TimeSpan Elapsed_Time_Token { get; }
        Token Token { get; set; }

        ProxyResult CreateCase(CaseData data);
        ProxyResult SubmitFeedback(CommentData data);
        ProxyResult GetAccessToken();
    }
}