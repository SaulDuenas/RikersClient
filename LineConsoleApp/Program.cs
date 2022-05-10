using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LineConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                   | SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;
            string Credentials = ConfigurationManager.AppSettings["credential"];


            var client = prepareclient();


            var request = new RestRequest(Method.GET);

            if (!String.IsNullOrEmpty(Credentials)) request.AddHeader("Authorization", $"Basic {Credentials}");

            IRestResponse response = client.Execute(request);

            Console.WriteLine(response.Content);
            Console.ReadLine();


        }

        static RestClient prepareclient() {

            RestClient client = new RestClient();

            string baseurl = ConfigurationManager.AppSettings["BaseUrl"] + ConfigurationManager.AppSettings["EndPointAccessToken"];

            client.BaseUrl = baseurl;
            client.Timeout = -1;

            bool UseSSL = string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseSSL"]) ? false :
                        (ConfigurationManager.AppSettings["UseSSL"]).Equals("false") ? false : true;

           string SerialCert = UseSSL ? ConfigurationManager.AppSettings["SerialCert"] : "";

          
            if (UseSSL)
            {
                // validate certificate
                bool serial_passed = !(string.IsNullOrWhiteSpace(SerialCert)) && !SerialCert.Equals(string.Empty);
                X509Certificate2 certificate = serial_passed ? getCertificate(SerialCert) : null;


                if (serial_passed && certificate == null)
                {
                    Console.WriteLine($"certificate to authenticate to {baseurl} not found by certificate");

                }
                else if (certificate.NotAfter > DateTime.Now)
                {

                    Console.WriteLine($"certificate to authenticate to {baseurl} - Subject {certificate.Subject} - Issuer: {certificate.IssuerName} - Serial: {certificate.SerialNumber}");
                   
                    client.ClientCertificates = new X509CertificateCollection() { certificate };
                }
                else
                {

                    Console.WriteLine($"certificate to authenticate to {baseurl} - expire on {certificate.NotAfter.ToString("yyyy-MM-dd")}");

                }

            }

            else
            {
                Console.WriteLine($"connection without SSL, it's posibble what the connection for this site is not secure");
                Console.WriteLine($"certificate to authenticate to {baseurl} not have been provided");

               
            }

            return client;

        }


        static private X509Certificate2 getCertificate(string serial)
        {

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            certStore.Open(OpenFlags.ReadOnly);

            var certCollection = certStore.Certificates.OfType<X509Certificate2>().FirstOrDefault(x => !(string.IsNullOrWhiteSpace(x.SerialNumber)) && x.SerialNumber.Equals(serial));

            certStore.Close();

            return certCollection;

        }
        
        
    }
}
