using RestSharp;
using RikersProxy.Entities;

namespace RikersProxy
{
    public interface IClientApi
    {
        string BaseUrl { get; set; }
        string Credentials { get; set; }

        IRestResponse ObtainToken();
        IRestResponse CreateCase(string token, CaseData data);
    }
}