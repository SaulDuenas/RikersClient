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


        public IRestResponse ObtainToken()
        {
            _client.BaseUrl = BaseUrl;
            _client.Timeout = -1;

            if (this.x509CertificateCollection != null) _client.ClientCertificates = x509CertificateCollection;

            var request = new RestRequest(Method.GET);
         
            if (this.Credentials != null) request.AddHeader("Authorization", $"Basic {Credentials}");

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

        public void Dispose()
        {
            if (_client != null) { _client = null; }
        }
    }
}

