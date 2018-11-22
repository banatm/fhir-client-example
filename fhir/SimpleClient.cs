using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fhir
{
    partial class Program
    {
        public class SimpleClient: FhirClient
        {
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
                Console.Out.WriteLine(
                    "curl -X {0} {1} -H \"Content-Type: application/json\" -d '{2}'", e.RawRequest.Method,e.RawRequest.Address,System.Text.Encoding.UTF8.GetString(e.Body??new byte[0]));
                Console.Out.WriteLine("```");
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
}
