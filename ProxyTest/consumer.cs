using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using ProxyTest.DTOs;
using System.Threading;

namespace ProxyTest
{
    public class Consumer
    {
        public delegate void RegisterStatusEventHandler(InBoundFileDTO file, HttpStatusCode response);
        // Evento
        public event RegisterStatusEventHandler RegisterStatusEvent;

        Thread _thread;

        public string BaseUrl { get; set; }
        public List<InBoundFileExt> Buffer { get; }



        public Consumer() 
        {
            BaseUrl = "http://localhost:56500/api/Ticket";
           this.Buffer = new List<InBoundFileExt>();
        }

        public Consumer(string url)
        {
            BaseUrl = url;
            this.Buffer = new List<InBoundFileExt>();
        }

        

        public string getFiles() {

            var client = new RestClient(BaseUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            
            return response.Content;

        }

        public HttpStatusCode registerFile(InBoundFileDTO file) {

            var client = new RestClient(BaseUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddObject(file);
            //AddParameter("Name", file.Name);
            //request.AddParameter("Path", file.Path);
            //request.AddParameter("CreateDate", file.CreateDate);
            IRestResponse response = client.Execute(request);

            return response.StatusCode;

        }


        public void StartAutoSend()
        {
            _thread = new Thread(new ThreadStart(TAutomaticSend));
            _thread.Start();

        }


        public void TAutomaticSend() {

            while (true) {

                Buffer.Where(p => p.Processed == 0).ToList().ForEach(item => {

                    var result = registerFile(item.inboundfile);

                    Thread.Sleep(1000);

                    if (result == HttpStatusCode.Accepted || result == HttpStatusCode.OK) { item.Processed = 1; }

                    if (RegisterStatusEvent != null) { RegisterStatusEvent(item.inboundfile, result); }

                });

                if (Buffer.Where(p => p.Processed == 1).ToList().Count > 0) {

                    Buffer.RemoveAll(x => x.Processed == 1);

                }
                
            }    
        
        }

    }
}
