using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fhir
{
    public class OrderResources
    {
        public SimpleClient client { get; set; }
                
        public void run()
        {
            var now = new FhirDateTime(DateTime.Now);

            Console.Out.WriteLine("#Create Patient");
            var patient= createPatient();
            
            Console.Out.WriteLine("#Create Doctor = Practitioner");
            var doctor = createPractitioner();
                        


            Console.Out.WriteLine("#Create Order = ServiceRequest in draft state");
            Console.Out.WriteLine("see http://hl7.org/fhir/2018Sep/servicerequest.html");
            var order = createOrder(now,patient,doctor,new[] {"AST","ALT", "Super profile"});
            
            Console.Out.WriteLine("#Create specimen and tubes = Specimen");
            Console.Out.WriteLine("see http://hl7.org/fhir/2018Sep/specimen.html");
            var tubes= createSpecimenAndTubes(order,patient,now);
            
            Console.Out.WriteLine("#Create patient questions = QuestionaireResponse resource");
            Console.Out.WriteLine("see http://hl7.org/fhir/2018Sep/QuestionnaireResponse.html");
            var patientResponses = createPatientQuestionReponses(now, patient,order);
            

            //finish order and change its status
            order.SupportingInfo = new List<ResourceReference>{patientResponses.GlobalURLReference(client.UseFullResourcePath)};
            order.Specimen=tubes.Select(t => t.GlobalURLReference(client.UseFullResourcePath)).ToList();
            
            order.Status = RequestStatus.Active;
            
            Console.Out.WriteLine("#Save active order - set link to related resources");
            client.Update(order);
                
            var preorder = createPreorder(now, patient, doctor, new[] { "AST", "ALT", "CBC+5DIFF" },true );
            var preorder2 = createPreorder(now, patient, doctor, new[] { "25OH-D3", "ALB", "CA" },false, "EKOPERN");
            var preorder3 = createPreorder(now, patient, doctor, new[] { "25OH-D3", "ALB", "CA" }, false, "EGGLEK");


        }

        private IEnumerable<Specimen> createSpecimenAndTubes(ServiceRequest order, Patient patient, FhirDateTime collectionDate)
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


            return specimens.Select(x =>
            {
                x.Request = new List<ResourceReference> { order.GlobalURLReference(client.UseFullResourcePath) };
                x.Subject = patient.GlobalURLReference(client.UseFullResourcePath);
                return client.Create(x);
            });
        }

        private QuestionnaireResponse createPatientQuestionReponses(FhirDateTime date, Patient patient, ServiceRequest order)
        {
            var patientQuestions = new QuestionnaireResponse
            {
                Authored =date.ToString(),
                Subject=order.GlobalURLReference(client.UseFullResourcePath),
                Source=patient.GlobalURLReference(client.UseFullResourcePath),
                Status=QuestionnaireResponse.QuestionnaireResponseStatus.Completed,
                Item = new List<QuestionnaireResponse.ItemComponent>()
                { 
                    new QuestionnaireResponse.ItemComponent()
                    {
                        LinkId = $"https://dia.medicover.com/serviceknowledgebase/question/LastMeal",
                        Definition = $"https://dia.medicover.com/serviceknowledgebase/question/LastMeal",
                        Text="Time from last meal",
                        Answer = new List<QuestionnaireResponse.AnswerComponent>{
                            new QuestionnaireResponse.AnswerComponent{Value=new Quantity(2,"h")}
                        }
                    },
                    new QuestionnaireResponse.ItemComponent{
                        LinkId = $"https://dia.medicover.com/serviceknowledgebase/question/Nice",
                        Definition = $"https://dia.medicover.com/serviceknowledgebase/question/Nice",
                        Text="Nice enviroment?",                        
                        Answer = new List<QuestionnaireResponse.AnswerComponent>{
                            new QuestionnaireResponse.AnswerComponent{Value=new FhirBoolean(true)}
                        }
                    }
                }
            };
            return client.Create(patientQuestions);             
        }

        private ServiceRequest createOrder(FhirDateTime date, Patient patient, Practitioner doctor, string[] serviceCodes)
        {
            var order = new ServiceRequest
            {
                Identifier = new List<Identifier> { new Identifier { System = "https://dia.medicover.com/sampleID", Value = "999999999900" } },
                AuthoredOn = date.ToString(),
                Status = RequestStatus.Draft,
                Intent = RequestIntent.Order,
                Code = Constants.ORDER_PROCEDURE_REQUEST_CODE,
                Subject = patient.GlobalURLReference(client.UseFullResourcePath),
                Requester = doctor.GlobalURLReference(client.UseFullResourcePath),
                OrderDetail = serviceCodes.Select(sc => new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/service",sc )).ToList(),
                Performer = new List<ResourceReference> { Constants.CURRENT_LAB_REFERENCE },                
                Note = new List<Annotation> {
                    new Annotation { Text="Very important comment 1"},
                    new Annotation { Text="Very important comment 2"}
                }
            };

            var fhirOrder = client.Create(order);
            return fhirOrder;
        }


        private ServiceRequest createPreorder(FhirDateTime date, Patient patient, Practitioner doctor, string[] serviceCodes, bool webstorePreorder, string contractID=null)
        {
            var order = new ServiceRequest
            {
                AuthoredOn = date.ToString(),
                Status = RequestStatus.Active,
                Intent = RequestIntent.Order,
                Code = Constants.ORDER_PROCEDURE_REQUEST_CODE,
                Category =
                webstorePreorder ?
                new List<CodeableConcept> {
                    new CodeableConcept("https://dia.medicover.com/dictionaries/requestcategory","FFS"),
                    new CodeableConcept("https://dia.medicover.com/dictionaries/requestcategory/ffs", "UA.WebOnline")
                } :
                new List<CodeableConcept> {
                    new CodeableConcept("https://dia.medicover.com/dictionaries/requestcategory","Contract"),
                    new CodeableConcept("https://dia.medicover.com/contract", contractID)
                },
                Subject = patient.GlobalURLReference(client.UseFullResourcePath),
                Requester = doctor.GlobalURLReference(client.UseFullResourcePath),
                OrderDetail = serviceCodes.Select(sc => new CodeableConcept("https://dia.medicover.com/serviceknowledgebase/service", sc)).ToList(),
                Note = new List<Annotation> {
                    new Annotation { Text="Very important comment 1"},
                    new Annotation { Text="Very important comment 2"}
                }
            };

            var fhirOrder = client.Create(order);
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
                    City ="Ksawerow",
                    Country ="PL",
                    PostalCode ="95-000",
                    Line =new[] {"Do lasu 31/24"}
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
                    City = "Pabianice",
                    Country = "PL",
                    PostalCode = "90-001",
                    Line = new[] { "Cacackiego 5" }
                } }
            };

            return client.Create(p);
        }


        
    }
}
