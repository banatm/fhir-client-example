using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fhir
{
    partial class Program
    {

        SimpleClient client { get; set; }
                
        public void run(string[] run)
        {
            if (run.Length > 1 && !string.IsNullOrEmpty(run[0]))
                client = new SimpleClient(new Uri(run[0]));
            else
                client = SimpleClient.localfapiClient;

            var order = new OrderResources(){client=client};
            //order.run();

            var result = new ResultResources(){client=client};
            result.LoadData("data/draftJsonModel.json");
            result.run();

        }

        static void Main(string[] args)
        {
            new Program().run(args);
        }

    }
}
