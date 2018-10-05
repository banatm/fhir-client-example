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
