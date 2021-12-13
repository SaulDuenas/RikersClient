using NetLogger.Implementation;
using RikersProxy;
using RikersProxy.Entities;
using RikersProxy.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static RikersProxy.ClientProxyAsync;

namespace Service.Domian.Core
{
    public class ProxyCoreAsync
    {
        public string CompanyName { get; set; }
        public string IbmCustomerNumber { get; set; }

        private Logger _logger = null;
        private  IClientProxyAsync _client;

        public ProxyCoreAsync(IClientProxyAsync clientproxy, Logger logger ) 
        {
            _client = clientproxy;
            _logger = logger;
        }


        public async Task<bool> TokenisOk()
        {

            bool tokenOk = true;

            if (!(_client.Elapsed_Time_Token.Equals(null)) && _client.Elapsed_Time_Token.TotalSeconds <= 0)
            {
                _logger.Info("CREATE TOKEN", "token expired, requesting for token", 100);
                var result = await _client.ObtainToken();

                result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));

                if (result.Code.Equals(HttpStatusCode.OK))
                {
                //    result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));

                    _logger.WriteLog("CREATE TOKEN", "SuccessAudit", $"Request Code: {(int)result.Code} - {result.Message} - AccessToken: {_client.Token.AccessToken} - ExpiresIn: {_client.Token.ExpiresIn}", 100);

                    tokenOk = true;
                }
                else
                {
                  //  result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));
                    tokenOk = false;
                  
                }

            }

            return tokenOk;

        }

        
        public async Task<ProxyResult> CreateCase(CaseData casedata ) 
        {
           /*
            if (await TokenisOk())
            {
              

            }
           */

           // var casedata = getCaseData("Subject de creación de Caso", "REPORTING DEVICE", "MX", "123456");

            var result = await _client.CreateCase(casedata);

            result.Messages.ToList().ForEach(r => _logger.WriteLog(r.Category, r.Type, $"Request Code: {r.Code} - Message: {r.Reason}", 100));


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
                CompanyName =  this.CompanyName,    // ConfigurationManager.AppSettings["CompanyName"],
                IbmCustomerNumber =  this.IbmCustomerNumber   // ConfigurationManager.AppSettings["IbmCustomerNumber"]
            };

            casedata.CaseContact = new CaseContact() { GivenName = "Donald", FamilyName = "Duck", Phone = "+555555", Email = "donald@duck.false" };

            casedata.Asset = new Asset() { IbmMachineType = "MF32", IbmMachineModel = "000", Serial = "MX46709" }; // get wbservice catalog

            return casedata;
        }


    }
}
