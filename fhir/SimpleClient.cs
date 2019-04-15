using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fhir
{
   
        public class SimpleClient: FhirClient
        {
            public static SimpleClient fapiClient = new SimpleClient(new Uri("http://192.168.9.109:8889/baseDstu3"))
            {
                PreferredFormat = ResourceFormat.Json,
                UseFullResourcePath = false
            };

            public static SimpleClient localfapiClient = new SimpleClient(new Uri("http://localhost:8081/R4"))
            {
                PreferredFormat = ResourceFormat.Json,
                UseFullResourcePath = false
            };
            public static SimpleClient vonkClient =   new SimpleClient(new Uri("http://192.168.9.109:8888"))
            {
                PreferredFormat = ResourceFormat.Json                    
            };


            public bool UseFullResourcePath { get; set; }
            public SimpleClient(Uri endpoint, bool verifyFhirVersion = false) : base(endpoint, verifyFhirVersion)
            {
                this.OnBeforeRequest += SimpleClient_OnBeforeRequest;
                this.OnAfterResponse += SimpleClient_OnAfterResponse;
            }

            private void SimpleClient_OnAfterResponse(object sender, AfterResponseEventArgs e)
            {
                Console.Out.WriteLine("##Response");
                Console.Out.WriteLine("Headers:");
                Console.Out.WriteLine("```");
                foreach (var h in e.RawResponse.Headers.AllKeys)
                    Console.WriteLine("{0}: {1}", h, e.RawResponse.Headers.Get(h));
                Console.Out.WriteLine("```");
                Console.Out.WriteLine("Body:");
                Console.Out.WriteLine("```json");
                Console.Out.WriteLine(System.Text.Encoding.UTF8.GetString(e.Body??new byte[0]));

                Console.Out.WriteLine("```");
            }

            private void SimpleClient_OnBeforeRequest(object sender, BeforeRequestEventArgs e)
            {
                Console.Out.WriteLine("##Request");
                Console.Out.WriteLine("```sh");                
                var json = System.Text.Encoding.UTF8.GetString(e.Body??new byte[0]);
                Console.Out.WriteLine(
                    "curl -X {0} {1} -H \"Content-Type: application/json\" -d '{2}'", e.RawRequest.Method,e.RawRequest.Address,json);
                Console.Out.WriteLine("```");

                System.IO.File.WriteAllText("data/out.json",json);
            }

            public async Task<IEnumerable<string>> ResourcesToStringAsync<T>(Func<T,string> projection) 
                where T : Resource
            {
                var bundle = await SearchAsync(typeof(T).GetCollectionName());

                return  from entry in bundle.Entry
                let r = entry?.Resource as T
                where r != null
                select projection(r);
            }
        }


        
    
}
