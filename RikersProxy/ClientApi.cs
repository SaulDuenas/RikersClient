using Newtonsoft.Json;
using RestSharp;
using RikersProxy.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace RikersProxy
{
    public class ClientApi :IDisposable, IClientApi
    {
        public string BaseUrl { get; set; }
        public string Credentials { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public X509CertificateCollection x509CertificateCollection { get; set; }

        private RestClient _client = new RestClient();

        public ClientApi()
        {
            BaseUrl = "http://localhost:56500/api/Ticket";
        }

        public ClientApi(string url, string credentials)
        {
            BaseUrl = url;
            Credentials = credentials;

        }

        public ClientApi(string url, string credentials, string clientid, string clientsecret)
        {
            BaseUrl = url;
            Credentials = credentials;
            ClientId = clientid;
            ClientSecret = clientsecret;
        }

        public IRestResponse GetAccessTokenbyClientSecret()
        {
            _client.BaseUrl = BaseUrl;
            _client.Timeout = -1;

            if (this.x509CertificateCollection != null) _client.ClientCertificates = x509CertificateCollection;

            var request = new RestRequest(Method.POST);

            request.AddParameter("grant_type", "client_credentials");
            if (!String.IsNullOrEmpty(this.ClientId)) request.AddHeader("client_id", $"Basic {ClientId}");
            if (!String.IsNullOrEmpty(this.ClientSecret)) request.AddHeader("client_id", $"Basic {ClientSecret}");

            IRestResponse response = _client.Execute(request);
            return response;
        }

        public IRestResponse GetAccessToken()
        {
            _client.BaseUrl = BaseUrl;
            _client.Timeout = -1;

            if (this.x509CertificateCollection != null) _client.ClientCertificates = x509CertificateCollection;

            var request = new RestRequest(Method.GET);
         
            if (!String.IsNullOrEmpty(this.Credentials)) request.AddHeader("Authorization", $"Basic {Credentials}");

            IRestResponse response = _client.Execute(request);
            return response;
        }

        public IRestResponse CreateCase(string token,CaseData data)
        {
            _client.BaseUrl = BaseUrl;
            _client.Timeout = -1;

            if (this.x509CertificateCollection != null) _client.ClientCertificates = x509CertificateCollection;


            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);

            // request.AddBody(JsonConvert.SerializeObject(data));

            request.AddParameter("application/json", JsonConvert.SerializeObject(data), ParameterType.RequestBody);

            IRestResponse response = _client.Execute(request);

            return response;
        }

        public IRestResponse SubmitFeedback(string token, CommentData data)
        {
            _client.BaseUrl = BaseUrl;
            _client.Timeout = -1;

            if (this.x509CertificateCollection != null) _client.ClientCertificates = x509CertificateCollection;

            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);

            // request.AddBody(JsonConvert.SerializeObject(data));

            request.AddParameter("application/json", JsonConvert.SerializeObject(data), ParameterType.RequestBody);

            IRestResponse response = _client.Execute(request);

            return response;
        }


        public void Dispose()
        {
            if (_client != null) { _client = null; }
        }
    }
}

