# fhir-client-example
Fhir-client-example showing how to create and store laboratory procedure request:
- Patient
- Doctor (Practitioner)
- PatientQuestions (as Observations)
- Order and Services (related ProcedureRequests)
- Specimen and sample/tubes data

# How to make the server

1. Clone version allowing external references: 
```
git clone https://github.com/banatm/hapi-fhir-docker.git
```
2. Build in docker: 
```
cd hapi-fhir-1/
docker build . -t my:fhir-hapi-server 
```
3. Run container: 
```
docker run -d --name fhir-hapi-server -p 8889:8080 my:fhir-hapi-server
```
# Using client
1. Change the server address
```
  SimpleClient fapiClient = new SimpleClient(new Uri("http://<ip here>:8889/baseDstu3"))
        {
            PreferredFormat = ResourceFormat.Json,
            AllowExternalReferences = false
        };      
        
  SimpleClient client { get { return fapiClient; } }
```
2. Run it
