using Newtonsoft.Json;
using RestSharp;
using RikersProxy.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy
{
    public class ClientProxy : IClientProxy
    {

        private readonly static IClientProxy _in = new ClientProxy();

        public string BaseUrl { get; set; }
        public string Credentials { get; set; }
        public bool UseSSL { get; set; }
        public string SerialCert { get; set; }
        public string EndPointGetToken { get; set; }
        public string EndPointCreateCase { get; set; }
        public string EndPointFeedBack { get; set; }

        public Token Token { get; set; }

        public TimeSpan Elapsed_Time_Token
        {
            get
            {
                return (Token != null) ? (_token_date_end - DateTime.Now) : TimeSpan.FromTicks(0);
            }
        }


        public static IClientProxy Instance
        {
            get {

                return _in;
            }
        }


        private DateTime _token_date_end;

        private ClientProxy()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                   | SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;

            this.BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            this.Credentials = ConfigurationManager.AppSettings["credential"];
            this.EndPointGetToken = ConfigurationManager.AppSettings["EndPointAccessToken"];
            this.EndPointCreateCase = ConfigurationManager.AppSettings["EndPointCreateCase"];
            this.EndPointFeedBack = ConfigurationManager.AppSettings["EndPointFeedBack"];

            this.UseSSL = string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseSSL"]) ? false :
                          (ConfigurationManager.AppSettings["UseSSL"]).Equals("false") ? false:true;

            this.SerialCert = this.UseSSL ? ConfigurationManager.AppSettings["SerialCert"]:"";

        }

        /*

        public ClientProxy(string url, string credentials)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                   | SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;

            this.BaseUrl = url;
            this.Credentials = credentials;

        }
        */

        private List<Message> prepareClient(ClientApi client, string endpoint)
        {

            bool serial_passed = false;
            List<Message> message = new List<Message>();

            client.BaseUrl = this.BaseUrl + endpoint;
            client.Credentials = this.Credentials;

            message.Add(new Message() { Code = null, //Code = HttpStatusCode.UpgradeRequired.ToString(),
                                        Category = "API CONF",
                                        Type = "SuccessAudit",
                                        Reason = $"EndPoint: {client.BaseUrl}"
                                      }
                       );

            if (this.UseSSL)
            {
                // validate certificate
                serial_passed = !(string.IsNullOrWhiteSpace(this.SerialCert)) && !this.SerialCert.Equals(string.Empty);
                X509Certificate2 certificate = serial_passed ? getCertificate(this.SerialCert) : null;

                if (serial_passed && certificate == null)
                {
                    message.Add(new Message()
                    {
                        Code=null, //Code = HttpStatusCode.UpgradeRequired.ToString(),
                        Category = "API CONF",
                        Type = "Warning",
                        Reason = $"certificate to authenticate to {this.BaseUrl} not found by certificate"
                    });
                }
                else if (certificate.NotAfter > DateTime.Now)
                {
                    message.Add(new Message()
                    {
                        Code = null, //Code = HttpStatusCode.Accepted.ToString(),
                        Category = "API CONF",
                        Type = "SuccessAudit",
                        Reason = $"certificate to authenticate to {this.BaseUrl} - Subject {certificate.Subject} - Issuer: {certificate.IssuerName} - Serial: {certificate.SerialNumber}"
                    });

                    client.x509CertificateCollection = new X509CertificateCollection() { certificate };
                }
                else
                {
                    message.Add(new Message()
                    {
                        Code = null, //Code = HttpStatusCode.UpgradeRequired.ToString(),
                        Category = "API CONF",
                        Type = "Error",
                        Reason = $"certificate to authenticate to {this.BaseUrl} - expire on {certificate.NotAfter.ToString("yyyy-MM-dd")}"
                    });
                }

            }

            else
            {
                message.Add(new Message()
                {
                    Code = null, //Code = HttpStatusCode.UpgradeRequired.ToString(),
                    Category = "API CONF",
                    Type = "Warning",
                    Reason = $"connection without SSL, it's posibble what the connection for this site is not secure"
                });
                message.Add(new Message()
                {
                    Code = null, //Code = HttpStatusCode.UpgradeRequired.ToString(),
                    Category = "API CONF",
                    Type = "Warning",
                    Reason = $"certificate to authenticate to {this.BaseUrl} not have been provided"
                });
            }

            return message;

        }


        private X509Certificate2 getCertificate(string serial)
        {

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            certStore.Open(OpenFlags.ReadOnly);

            var certCollection = certStore.Certificates.OfType<X509Certificate2>().FirstOrDefault(x => !(string.IsNullOrWhiteSpace(x.SerialNumber)) && x.SerialNumber.Equals(serial));

            certStore.Close();

            return certCollection;

        }


        public ProxyResult GetAccessTokenByClientSecret()
        {
            return null; 
        }

        public ProxyResult GetAccessToken()
        {
            IRestResponse response;

            List<Message> MessageLts = new List<Message>();
           
            if (!(string.IsNullOrEmpty(this.Credentials) && string.IsNullOrEmpty(this.BaseUrl)  && String.IsNullOrEmpty(this.EndPointGetToken) ))
            {
                using (var client = new ClientApi())
                {
                    try
                    {
                        MessageLts = prepareClient(client, this.EndPointGetToken);

                        response = client.GetAccessToken();

                        if ((response != null) && response.StatusCode == HttpStatusCode.OK)

                        {
                            this.Token = JsonConvert.DeserializeObject<Token>(response.Content);
                            _token_date_end = DateTime.Now.AddSeconds(this.Token.ExpiresIn);

                            MessageLts.Add(new Message { Code = ((int)response.StatusCode).ToString(), Category = "API TOKEN", Type = "Information", Reason = $"successfull generated token" });

                            return new ProxyResult() { Code = response.StatusCode, Message = $"successfull generated token", Messages = MessageLts };
                        }

                        else
                        {
                            //  var result = JsonConvert.DeserializeObject<ResponseMessage>(response.Content);

                            // var msg = result.Messages.Select(p => new Message() { Category = "API TOKEN", Code = p.Code, Type = p.Type, Reason = p.MessageMessage }).ToList();

                            MessageLts.Add(new Message()
                            {
                                Code = string.IsNullOrEmpty(response.Content) ? null:((int)response.StatusCode).ToString() ,
                                Category = "API TOKEN",
                                Type = "Information",
                                Reason = string.IsNullOrEmpty(response.Content) ? "Respuesta no esperada - Response NULL" : response.Content
                            }) ;

                            //MessageLts.AddRange(MessageLts);

                            return new ProxyResult() { Code = response.StatusCode, Message = $"Unauthorized", Messages = MessageLts };

                        }

                        /*

                        else
                        {
                            var message = response == null ? "response is null" :
                                          !string.IsNullOrEmpty(response.Content) ? response.Content :
                                          !string.IsNullOrEmpty(response.StatusDescription) ? response.StatusDescription :
                                          !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : Newtonsoft.Json.JsonConvert.SerializeObject(response);

                            MessageLts.Add(new Message { Code = ((int)response.StatusCode).ToString(), Category = "API TOKEN", Type = "Error", Reason = message });

                            return new ProxyResult() { Code = response.StatusCode, Message = message, Messages = MessageLts };

                        }

                        */


                    }
                    catch (Exception ex)
                    {
                        StackTrace trace = new StackTrace(ex, true);

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.SeeOther).ToString(),
                            Category = "API TOKEN",
                            Type = "Error",
                            Reason = ex.Message
                        });

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.SeeOther).ToString(),
                            Category = "API TOKEN",
                            Type = "FaultAudit",
                            Reason = trace.ToString()
                        });

                        return new ProxyResult { Code = HttpStatusCode.SeeOther, Message = "Falta Error", Messages = MessageLts };

                    }

                }

            }
 
            else 
            {
                if (string.IsNullOrEmpty(this.Credentials))
                {
                    MessageLts.Add(new Message()
                    {
                        Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                        Category = "API TOKEN",
                        Type = "Error",
                        Reason = "Parameter Credentials is Empty or not exist, check the parameter Credentials on App.Config. "
                    });
                }

                if (string.IsNullOrEmpty(this.BaseUrl))
                {
                    MessageLts.Add(new Message()
                    {
                        Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                        Category = "API TOKEN",
                        Type = "Error",
                        Reason = "Parameter BaseUrl is Empty or not exist, check the parameter BaseUrl on App.Config"
                    });
                }

                if (string.IsNullOrEmpty(this.EndPointGetToken))
                {
                    MessageLts.Add(new Message()
                    {
                        Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                        Category = "API TOKEN",
                        Type = "Error",
                        Reason = "Parameter EndPointAccessToken is Empty or not exist, check the parameter EndPointAccessToken on App.Config"
                    });
                }

                return new ProxyResult { Code = HttpStatusCode.NotAcceptable, Message = "Cannot bind API, BaseUrl not provided", Messages = MessageLts };
            }
        }

        public ProxyResult CreateCase(CaseData data)
        {
            ProxyResult result = null;
            List<Message> MessageLts = new List<Message>();
            string message = "";


            if (Elapsed_Time_Token.TotalMinutes > 0 && !String.IsNullOrEmpty(this.EndPointCreateCase))
            {
                
                IRestResponse response;

                using (var client = new ClientApi())
                {
                    try
                    {
                        MessageLts = prepareClient(client, this.EndPointCreateCase);

                        response = client.CreateCase(this.Token.AccessToken, data);

                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            var casecreated = JsonConvert.DeserializeObject<ResponseCaseCreated>(response.Content);
                            
                            var msg = $"successfull case created id:{casecreated.CaseNumber} - TransactionID: {casecreated.CommonArea.TransactionId} - TransactionDate: {casecreated.CommonArea.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")}";
                            
                            MessageLts.Add(new Message { Code = ((int)response.StatusCode).ToString(), Category = "API CREATE CASE", Type = "Information", Reason = msg });

                            result = new ProxyResult {  Code = response.StatusCode, Message = msg, Messages = MessageLts, CaseCreate = casecreated };

                        }
                        else 
                        {
                            message = !string.IsNullOrEmpty(response.Content) ? response.Content : "";

                            var responseMessage = JsonConvert.DeserializeObject<ResponseMessage>(response.Content);

                            if (responseMessage.Messages != null)
                            {
                                var msg = responseMessage.Messages.Select(p => new Message() { Category = "API CREATE CASE", Code = p.Code, Type = p.Type, Reason = p.MessageMessage }).ToList();
                                MessageLts.AddRange(msg);
                            }
                            else {

                                if (response == null)
                                {
                                    MessageLts.Add(new Message() { Code = response.StatusCode.ToString(), Category = "API CREATE CASE", Type = "Error", Reason = "response is null" });
                                }
                                else {
                                    if (!string.IsNullOrEmpty(response.StatusDescription)) MessageLts.Add(new Message() { Code = ((int)response.StatusCode).ToString(), Category = "API CREATE CASE", Type = "Error", Reason = response.StatusDescription });
                                    if (!string.IsNullOrEmpty(response.ErrorMessage)) MessageLts.Add(new Message() { Code = ((int)response.StatusCode).ToString(), Category = "API CREATE CASE", Type = "Error", Reason = response.ErrorMessage });
                                    if (!string.IsNullOrEmpty(response.Content)) MessageLts.Add(new Message() { Code = ((int)response.StatusCode).ToString(), Category = "API CREATE CASE", Type = "Error", Reason = response.Content });
                                }

                            }
                            result = new ProxyResult { Code = response.StatusCode, Message = $"", Messages = MessageLts, ResponseMessage = responseMessage };

                        }

                    }
                    catch (Exception ex)
                    {
                     
                        StackTrace trace = new StackTrace(ex, true);

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.NotImplemented).ToString(),
                            Category = "API CREATE CASE",
                            Type = "Error",
                            Reason = message
                        }); ;

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.NotImplemented).ToString(),
                            Category = "API CREATE CASE",
                            Type = "Error",
                            Reason = ex.Message
                        });

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.NotImplemented).ToString(),
                            Category = "API CREATE CASE",
                            Type = "FaultAudit",
                            Reason = trace.ToString()
                        });

                        result = new ProxyResult { Code = HttpStatusCode.NotImplemented, Message = "Falta Error", Messages = MessageLts };
                    }

                }

            }
            else if (Elapsed_Time_Token.TotalMinutes <= 0)
            {
                MessageLts.Add(new Message()
                {
                    Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                    Category = "API CREATE CASE",
                    Type = "Warning",
                    Reason = "token expired, get a new token"
                });

                result = new ProxyResult() { Code = HttpStatusCode.NotAcceptable, Message = $"token expired", Messages = MessageLts };
            }
            else if (String.IsNullOrEmpty(this.EndPointCreateCase))
            {
                MessageLts.Add(new Message()
                {
                    Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                    Category = "API CREATE CASE",
                    Type = "Error",
                    Reason = "Parameter EndPointCreateCase is Empty or not exist, check the parameter EndPointCreateCase on App.Config"
                });

                result = new ProxyResult() { Code = HttpStatusCode.NotAcceptable, Message = $"Parameter EndPointCreateCase is Empty or not exist", Messages = MessageLts };
            }

            return result;
        }


        public ProxyResult SubmitFeedback(CommentData data)
        {
            ProxyResult result = null;
            List<Message> MessageLts = new List<Message>();
            string message = "";

            if (Elapsed_Time_Token.TotalMinutes > 0 && !String.IsNullOrEmpty(this.EndPointFeedBack))
            {
                IRestResponse response;

                using (var client = new ClientApi())
                {
                    try
                    {
                        var endpoint = this.EndPointFeedBack.Replace("{casenumber}",data.CaseNumber);
                        MessageLts = prepareClient(client, endpoint);

                        response = client.SubmitFeedback(this.Token.AccessToken, data);

                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            var FeedBackResponse = JsonConvert.DeserializeObject<ResponseFeedBack>(response.Content);

                            var msg = $"successfull submit feedback:{FeedBackResponse.CaseNumber} - TransactionID: {FeedBackResponse.CommonArea.TransactionId} - TransactionDate: {FeedBackResponse.CommonArea.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")}";

                            MessageLts.Add(new Message { Code = ((int)response.StatusCode).ToString(), Category = "API SUBMIT FEEDBACK", Type = "Information", Reason = msg });

                            result = new ProxyResult { Code = response.StatusCode, Message = msg, Messages = MessageLts, FeedBack = FeedBackResponse };

                        }
                        else
                        {
                            message = !string.IsNullOrEmpty(response.Content) ? response.Content : "";

                            var resultResponse = JsonConvert.DeserializeObject<ResponseMessage>(response.Content);

                            if (resultResponse.Messages != null)
                            {
                                var msg = resultResponse.Messages.Select(p => new Message() { Category = "API SUBMIT FEEDBACK", Code = p.Code, Type = p.Type, Reason = p.MessageMessage }).ToList();
                                MessageLts.AddRange(msg);
                            }
                            else
                            {

                                if (response == null)
                                {
                                    MessageLts.Add(new Message() { Code = response.StatusCode.ToString(), Category = "API SUBMIT FEEDBACK", Type = "Error", Reason = "response is null" });
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(response.StatusDescription)) MessageLts.Add(new Message() { Code = ((int)response.StatusCode).ToString(), Category = "API SUBMIT FEEDBACK", Type = "Error", Reason = response.StatusDescription });
                                    if (!string.IsNullOrEmpty(response.ErrorMessage)) MessageLts.Add(new Message() { Code = ((int)response.StatusCode).ToString(), Category = "API SUBMIT FEEDBACK", Type = "Error", Reason = response.ErrorMessage });
                                    if (!string.IsNullOrEmpty(response.Content)) MessageLts.Add(new Message() { Code = ((int)response.StatusCode).ToString(), Category = "API SUBMIT FEEDBACK", Type = "Error", Reason = response.Content });
                                }

                            }
                            result = new ProxyResult { Code = response.StatusCode, Message = $"", Messages = MessageLts, ResponseMessage = resultResponse };

                        }

                    }
                    catch (Exception ex)
                    {

                        StackTrace trace = new StackTrace(ex, true);

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.NotImplemented).ToString(),
                            Category = "API SUBMIT FEEDBACK",
                            Type = "Error",
                            Reason = message
                        }); ;

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.NotImplemented).ToString(),
                            Category = "API SUBMIT FEEDBACK",
                            Type = "Error",
                            Reason = ex.Message
                        });

                        MessageLts.Add(new Message()
                        {
                            Code = ((int)HttpStatusCode.NotImplemented).ToString(),
                            Category = "API SUBMIT FEEDBACK",
                            Type = "FaultAudit",
                            Reason = trace.ToString()
                        });

                        result = new ProxyResult { Code = HttpStatusCode.NotImplemented, Message = "Falta Error", Messages = MessageLts };
                    }

                }

            }
            else if (Elapsed_Time_Token.TotalMinutes <= 0)
            {
                MessageLts.Add(new Message()
                {
                    Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                    Category = "API SUBMIT FEEDBACK",
                    Type = "Warning",
                    Reason = "token expired, get a new token"
                });

                result = new ProxyResult() { Code = HttpStatusCode.NotAcceptable, Message = $"token expired", Messages = MessageLts };
            }
            else if (String.IsNullOrEmpty(this.EndPointFeedBack))
            {
                MessageLts.Add(new Message()
                {
                    Code = ((int)HttpStatusCode.NotAcceptable).ToString(),
                    Category = "API CREATE CASE",
                    Type = "Error",
                    Reason = "Parameter EndPointCreateCase is Empty or not exist, check the parameter EndPointCreateCase on App.Config"
                });

                result = new ProxyResult() { Code = HttpStatusCode.NotAcceptable, Message = $"Parameter EndPointCreateCase is Empty or not exist", Messages = MessageLts };
            }

            return result;
        }


        public class ProxyResult
        {
            public HttpStatusCode Code { get; set; }
            public string Message { get; set; }
            public List<Message> Messages { get; set; }
            public ResponseCaseCreated CaseCreate { get; set; }
            public ResponseFeedBack FeedBack { get; set; }
            public ResponseMessage ResponseMessage { get; set; }
        }

        public class Message
        {
            public string Type { get; set; }
            public string Code { get; set; }
            public string Reason { get; set; }
            public string Category { get; set; }
        }

    }

}
