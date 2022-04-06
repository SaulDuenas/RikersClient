using NetLogger.Implementation;
using RikersProxy;
using RikersProxy.Entities;
using RikersProxy.Entities.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static RikersProxy.ClientProxy;

namespace Service.Domian.Core.Proxy
{
    public class ProxyCore
    {
        public string CompanyName { get; set; }
        public string IbmCustomerNumber { get; set; }

        private ILogger _logger = null;
        private  IClientProxy _client;

        public ProxyCore(IClientProxy clientproxy, ILogger logger ) 
        {
            _client = clientproxy;
            _logger = logger;

            this.CompanyName = ConfigurationManager.AppSettings["CompanyName"];
            this.IbmCustomerNumber = ConfigurationManager.AppSettings["IbmCustomerNumber"];
        }

        public bool TokenExpired() 
        {
            return !(_client.Elapsed_Time_Token.Equals(null)) && _client.Elapsed_Time_Token.TotalSeconds <= 0;
        }

        public bool TokenisAvailable()
        {
            bool retval = true;
            
            if (TokenExpired())
            {
                _logger.Info("CREATE TOKEN", "token expired, requesting for token", 100);
                var result = _client.GetAccessToken();

                result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));

                if (result.Code.Equals(HttpStatusCode.OK))
                {
                //    result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));

                    _logger.SuccessAudit("CREATE TOKEN", $"Request Code: {(int)result.Code} - {result.Message} - AccessToken: {_client.Token.AccessToken} - ExpiresIn: {_client.Token.ExpiresIn}", 100);

                    retval = true;
                }
                else
                {
                  //  result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));
                    retval = false;
                  
                }

            }

            return retval;

        }

        
        public ProxyResult CreateCase(CaseData casedata ) 
        {
         
            var result = _client.CreateCase(casedata);

           // result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));

            return result;

        }

        public ProxyResult SubmitFeedback(CommentData commentdata) 

        {
            var result = _client.SubmitFeedback(commentdata);

            // result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));

            return result;
        }


        public CaseData getCaseData(string subject, string description, string country, string customerproblemnumber)
        {

            var casedata = new CaseData();

            casedata.Subject = subject;
            casedata.Description = description;
            casedata.Country = country;
            casedata.CustomerProblemNumber = customerproblemnumber;

            casedata.Customer = new Customer()
            {
                CompanyName = string.IsNullOrEmpty(this.CompanyName) ? "" : this.IbmCustomerNumber,
                IbmCustomerNumber = string.IsNullOrEmpty(this.IbmCustomerNumber) ? "" : this.IbmCustomerNumber

            };

            casedata.CaseContact = new CaseContact() { GivenName = "Donald", FamilyName = "Duck", Phone = "+555555", Email = "donald@duck.false" };

            return casedata;
        }


        public CommentData getCommentData(string Nticket,string caseNumber, string body)
        {

            var commentdata = new CommentData() { CaseNumber = caseNumber, Body = body, ExternalProblemNumber = Nticket };

            return commentdata;
        }


    }
}
