using RestSharp;
using RikersProxy.Entities;
using System.Security.Cryptography.X509Certificates;

namespace RikersProxy
{
    public interface IClientApi
    {
        string BaseUrl { get; set; }
        string Credentials { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }

        X509CertificateCollection x509CertificateCollection { get; set;}

        IRestResponse GetAccessToken();
        IRestResponse GetAccessTokenbyClientSecret();
        IRestResponse CreateCase(string token, CaseData data);
        IRestResponse SubmitFeedback(string token, CommentData data);
    }
}