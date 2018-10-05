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
        SimpleClient fapiClient = new SimpleClient(new Uri("http://192.168.9.109:8889/baseDstu3"))
        {
            PreferredFormat = ResourceFormat.Json,
            AllowExternalReferences = false
        };


        SimpleClient vonkClient =   new SimpleClient(new Uri("http://192.168.9.109:8888"))
                {
                    PreferredFormat = ResourceFormat.Json                    
                };
                
        SimpleClient client { get { return fapiClient; } }
                
        public void run()
        {
            var now = new FhirDateTime(DateTime.Now);

            var patient= createPatient();
            var doctor = createPractitioner();
            var patientQuestions = createPatientQuestionReponses(now,patient);
            

            var order = createProcedureRequest(now,patient,doctor,patientQuestions,new[] {"AST","ALT", "Super profile"});
            createSpecimenAndTubes(order,patient,now);

            //list patients
            Console.WriteLine("\n\nPatients:");
            client.ResourcesToStringAsync<Patient>(
                    p => $"ID: {p.Id}, Surname {p?.Name.FirstOrDefault()?.Family}, Name: {string.Join(" ", p?.Name.FirstOrDefault()?.Given)}"
                ).Result.ToList().ForEach(Console.WriteLine);

            //list doctors
            Console.WriteLine("\n\nDoctors:");
           client.ResourcesToStringAsync<Practitioner>(
                     p => $"ID: {p.Id}, Surname {p?.Name.FirstOrDefault()?.Family}, Name: {string.Join(" ", p?.Name.FirstOrDefault()?.Given)}"
                 ).Result.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\n\nPatient Questions:");
            client.ResourcesToStringAsync<Observation>(
                      p => $"ID: {p.Id}, Patient: {p?.Subject.Reference}, Question: {p?.Code.Coding?.FirstOrDefault()?.Code}, Answer: {p.Value}"
                  ).Result.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\n\nOrders:");
            client.ResourcesToStringAsync<ProcedureRequest>(
                      p => $"ID: {p.Id}, Intent:{p.Intent},  Patient: {p?.Subject.Reference}, Test: {p?.Code.Coding?.FirstOrDefault()?.Code}"
                  ).Result.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\n\nSpecimens:");
            client.ResourcesToStringAsync<Specimen>(
                      p => $"ID: {p.Id},   Patient: {p?.Subject.Reference}"
                  ).Result.ToList().ForEach(Console.WriteLine);



            Console.Read();
        }

        private void createSpecimenAndTubes(ProcedureRequest order, Patient patient, FhirDateTime collectionDate)
        {
            var specimens = new List<Specimen>
            {
                new Specimen
                {
                    Status=Specimen.SpecimenStatus.Available,
                    Type = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/specimen","EDTA", "EDTA blood"),
                    Collection = new Specimen.CollectionComponent{Collected=collectionDate,Quantity=new SimpleQuantity{Value=1} },
                    Container = new List<Specimen.ContainerComponent>
                    {
                        new Specimen.ContainerComponent
                        {
                            Type = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/tube","EDTA", "EDTA tube"),
                            Identifier = new List<Identifier>{new Identifier{System="https://dia.medicover.com/sampleID",Value="999999999901" } }
                        }
                    }
                },
                new Specimen
                {
                    Status=Specimen.SpecimenStatus.Available,
                    Type = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/specimen","SER", "Serum"),
                    Collection = new Specimen.CollectionComponent{Collected=collectionDate,Quantity=new SimpleQuantity{Value=3} },
                    Container = new List<Specimen.ContainerComponent>
                    {
                        new Specimen.ContainerComponent
                        {
                            Type = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/tube","SER-B", "Serum Bio"),
                            Identifier = new List<Identifier>{new Identifier{System="https://dia.medicover.com/sampleID",Value="999999999902" } }
                        },
                        new Specimen.ContainerComponent
                        {
                            Type = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/tube","SER-I", "Serum IM"),
                            Identifier = new List<Identifier>{new Identifier{System= "https://dia.medicover.com/sampleID", Value="999999999903" } }
                        },
                        new Specimen.ContainerComponent
                        {
                            Type = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/tube","SER-D", "Serum DE"),
                            Identifier = new List<Identifier>{new Identifier{System= "https://dia.medicover.com/sampleID", Value="999999999904" } }
                        }
                    }
                },
            };

            specimens.ToList().ForEach(x =>
                {
                    x.Request = new List<ResourceReference> { order.GlobalURLReference(client.AllowExternalReferences) };
                    x.Subject = patient.GlobalURLReference(client.AllowExternalReferences);                    
                    client.Create(x);
                });
        }

        private IEnumerable<Observation> createPatientQuestionReponses(FhirDateTime date, Patient patient)
        {
            var patientQuestions = new List<Observation>
            {
                new Observation
                {
                    Effective =date,
                    Subject=patient.GlobalURLReference(client.AllowExternalReferences),
                    Code=new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/question","LastMeal","Time from last meal"),
                    Status=ObservationStatus.Registered,
                    Category=Constants.PATIENT_QUESTION_OBSERVATION_CATEGORY,
                    Value = new Quantity(2,"h")
                },
                new Observation
                {
                    Effective = date,
                    Subject=patient.GlobalURLReference(client.AllowExternalReferences),
                    Code=new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/question","Was nice?","bla bla bla"),
                    Status=ObservationStatus.Registered,
                    Category=Constants.PATIENT_QUESTION_OBSERVATION_CATEGORY,
                    Value = new FhirBoolean(true)
                }
            };

            return patientQuestions.Select(x => client.Create(x)).ToList();
        }

        private ProcedureRequest createProcedureRequest(FhirDateTime date, Patient patient, Practitioner doctor, IEnumerable<Observation> patientQuestions,  string[] serviceCodes)
        {
            var order = new ProcedureRequest
            {
                Identifier = new List<Identifier> { new Identifier { System = "https://dia.medicover.com/sampleID", Value = "999999999900" } },
                AuthoredOn = date.ToString(),
                Status = RequestStatus.Active,
                Intent = RequestIntent.Order,
                Code = Constants.ORDER_PROCEDURE_REQUEST_CODE,
                Subject = patient.GlobalURLReference(client.AllowExternalReferences),
                Requester = new ProcedureRequest.RequesterComponent
                {
                    Agent = doctor.GlobalURLReference(client.AllowExternalReferences),
                    OnBehalfOf = Constants.CURRENT_BDP_REFERENCE,
                },
                Performer = Constants.CURRENT_LAB_REFERENCE,
                SupportingInfo = patientQuestions.Select(x=>x.GlobalURLReference(client.AllowExternalReferences)).ToList(),
                Note = new List<Annotation> {
                    new Annotation { Text="Very important comment 1"},
                    new Annotation { Text="Very important comment 2"}
                }
            };

            var fhirOrder = client.Create(order);

            var orderServices = serviceCodes.Select(serviceCode =>
                 new ProcedureRequest
                 {
                     Requisition = fhirOrder.GlobalURLIdentifier(),
                     AuthoredOn = date.ToString(),
                     Status = RequestStatus.Active,    
                     Intent=RequestIntent.InstanceOrder,
                     Subject=patient.GlobalURLReference(client.AllowExternalReferences),
                     Code = new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/service", serviceCode)                                                           
                 });
            orderServices.ToList().ForEach(x=>client.Create(x));

            return fhirOrder;
        }


        private Patient createPatient()
        {
            var p = new Patient()
            {
                Gender = AdministrativeGender.Male,
                Name = new List<HumanName> { new HumanName { Family = "Kowalski", Given = new[] { "Jan","Janusz" } } },
                Identifier = new List<Identifier> { new Identifier {
                    System = "https://www.gov.pl/cyfryzacja/rejestr-pesel",
                    Value ="12345678901"
                } },
                BirthDate = "1980-01-01",
                Address = new List<Address>(){ new Address {
                    City ="Dąbrowa Dolna",
                    Country ="PL",
                    PostalCode ="95-000",
                    Line =new[] {"Do lasu 31"}
                } }                                             
            };

            return client.Create(p);
        }

        private Practitioner createPractitioner()
        {
            var p = new Practitioner()
            {
                Gender = AdministrativeGender.Male,
                Name = new List<HumanName> { new HumanName { Family = "Abacki", Given = new[] { "Henryk" } } },
                Identifier = new List<Identifier> { new Identifier {
                    System = "http://rejestr.nil.org.pl", Value = "1111" } },                
                Address = new List<Address>() { new Address {
                    City = "Łódź",
                    Country = "PL",
                    PostalCode = "90-001",
                    Line = new[] { "Cacackiego 5" }
                } }
            };

            return client.Create(p);
        }

        static void Main(string[] args)
        {
            new Program().run();
        }


        
    }
}
