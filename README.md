# fhir-client-example
Fhir-client-example showing how to create and store laboratory procedure request in R4 of HL7 FHIR:
- Patient
- Doctor (Practitioner)
- PatientQuestions (as QuestionaireResponse)
- Order and Services (as ServiceRequest and ServiceRequest.orderdetail)
- Specimen and sample/tubes data (as Specimen resource Specimen.containers)

# Using client
1. Change the server address
```
  SimpleClient fapiClient = new SimpleClient(new Uri("http://<ip here>/R4"))
        {
            PreferredFormat = ResourceFormat.Json,
            AllowExternalReferences = false
        };      
        
  SimpleClient client { get { return fapiClient; } }
```
2. Run it
