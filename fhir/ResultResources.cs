using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Utility;

namespace fhir
{
    class MyTransactionBuilder
    {
        private Bundle _bundle = new Bundle() { Type = Bundle.BundleType.Transaction };
        public Uri BaseUrl { get; set; }
        public void Create(Resource r)
        {
            var newEntry = new Bundle.EntryComponent();
            newEntry.Request = new Bundle.RequestComponent();
            newEntry.Request.Method = Bundle.HTTPVerb.POST;
            newEntry.Resource = r;
            newEntry.Request.Url = new RestUrl(BaseUrl).AddPath(r.TypeName).ToString();
            newEntry.FullUrl = r.Id.ToString();
            _bundle.Entry.Add(newEntry);
        }

        public Bundle ToBundle()
        {

            _bundle.Entry.ForEach(e =>
                {
                    //clear resources id if urn based in POST event
                    if (e.Request.Method == Bundle.HTTPVerb.POST && e.Resource.Id.StartsWith("urn")) e.Resource.Id = null;
                });
            return _bundle;
        }

        public AuditEvent generateAuditEvent()
        {
            var ret = new AuditEvent()
            {
                Id = Uuid.Generate().ToString(),
                Type = new Coding("http://terminology.hl7.org/CodeSystem/audit-event-type", "rest"),
                Subtype = new List<Coding> { new Coding("http://hl7.org/fhir/restful-interaction", "create") },
                Action = AuditEvent.AuditEventAction.C,
                Recorded = DateTime.Now,
                Outcome = AuditEvent.AuditEventOutcome.N0,
                OutcomeDesc = "Records created",
                Agent = new List<AuditEvent.AgentComponent>{new AuditEvent.AgentComponent
                {
                    Type=new CodeableConcept("http://sso-service","user|system|location|patient"),
                    Requestor=true,
                    AltId="ret_user",
                    Network=new AuditEvent.NetworkComponent(){
                        Address="10.1.2.1",
                    }
                }},
                Source = new AuditEvent.SourceComponent()
                {
                    Observer = new ResourceReference("https://dia.medicover.com/operational-fhir/Device/fhir-server"),
                    Type = new List<Coding> { new Coding { System = "https://dia.medicover.com/security/AuditEventType", Code = "internal-fhir-server-audit" } }
                },
                Entity = this._bundle.Entry.Select(x => new AuditEvent.EntityComponent
                {
                    What = x.Resource.BundleRef()
                }).ToList()
            };
            return ret;

        }
    }

    public class ResultResources
    {
        public SimpleClient client { get; set; }
        fhir.model.DraftJsonModel model { get; set; }

        public void LoadData(string name)
        {
            model = fhir.model.DraftJsonModel.FromJson(System.IO.File.ReadAllText(name));
        }
        public void run()
        {

            var now = new FhirDateTime(DateTime.Now);
            Console.Out.WriteLine("#Create Bundle");
            var builder = new MyTransactionBuilder() { BaseUrl = this.client.Endpoint };

            var patient = createPatient();
            builder.Create(patient);


            var doctor = createPractitioner(model.Result.Order.Doctor, Constants.PRACTITIONAL_TYPE_DOCTOR);
            builder.Create(doctor);



            var performer = createOrganization(model.Result.Filler, new CodeableConcept(Constants.ORGTYPE_CODING_SYSTEM, Constants.ORGTYPE_LAB));
            builder.Create(performer);

            var client = createOrganization(model.Result.Order.Placer, new CodeableConcept(Constants.ORGTYPE_CODING_SYSTEM, Constants.ORGTYPE_CLIENT));
            builder.Create(client);

            var clientUnit = createOrganization(model.Result.Order.ClientUnit, new CodeableConcept(Constants.ORGTYPE_CODING_SYSTEM, Constants.ORGTYPE_CLIENT_UNIT));
            clientUnit.PartOf = client.BundleRef();
            builder.Create(clientUnit);

            var contract = createContract();
            builder.Create(contract);


            var order = createOrder(patient, doctor, performer, new List<ResourceReference>(){
                                                            client.BundleRef(),clientUnit.BundleRef(),contract.BundleRef()
                                                             });
            builder.Create(order);

            var task = createTask(order, performer);
            builder.Create(task);

            List<Practitioner> labdoctors;
            var observations = createObservations(patient, order, performer, out labdoctors);

            labdoctors.ForEach(o => builder.Create(o));
            observations.ForEach(o => builder.Create(o));


            var diagnosticsReport = createDiagnosticsReport(
                order,
                patient,
                labdoctors.Select(x => x.BundleRef()).Append(performer.BundleRef()),
                new List<ResourceReference>() { client.BundleRef(), clientUnit.BundleRef(), doctor.BundleRef() },
                observations
            );
            builder.Create(diagnosticsReport);
            builder.Create(builder.generateAuditEvent());
            this.client.Transaction(builder.ToBundle());

        }

        private Task createTask(ServiceRequest order, Organization performer)
        {
            return new Task()
            {
                Id = Uuid.Generate().ToString(),
                Focus = order.BundleRef(),
                Intent = Task.TaskIntent.Order,
                Status = Task.TaskStatus.Completed,
                Owner = performer.BundleRef(),
                Identifier = model.Result.Order.Identifier.Where(x => x.System.Contains("OrderHeaderID")).Select(x => new Identifier(x.System, x.Value)).ToList(),
                ExecutionPeriod = new Period(new FhirDateTime(model.Result.Order.PlacementDateTime), new FhirDateTime(model.Result.DocumentDateTime)),
                AuthoredOn = model.Result.Order.PlacementDateTime.ToString(Constants.DATE_FORMAT),
                LastModified = DateTime.Now.ToString(Constants.DATE_FORMAT),
            };
        }

        private DiagnosticReport createDiagnosticsReport(ServiceRequest order, Patient p, IEnumerable<ResourceReference> producers, IEnumerable<ResourceReference> consumers, List<Observation> observations)
        {
            return new DiagnosticReport()
            {
                Id = Uuid.Generate().ToString(),
                Identifier = new List<Identifier>() { new Identifier(model.Event.Producer, $"{model.Result.Id}/{model.Result.Version}") },
                BasedOn = new List<ResourceReference> { order.BundleRef() },
                Code = Constants.ORDER_PROCEDURE_REQUEST_CODE,
                Subject = p.BundleRef(),
                Performer = producers.ToList(),
                ResultsInterpreter = consumers.ToList(),
                Result = observations.Select(x => x.BundleRef()).ToList(),
                PresentedForm = new List<Attachment>()
                    {
                        new Attachment(){
                            Language="en",
                            ContentType="application/pdf",
                            Size=100,
                            Data=new byte[]{1,2,3,4,5,6,7},
                            Creation=model.Result.DocumentDateTime.ToString(Constants.DATE_FORMAT),
                            Hash=new byte[]{1,2,3},
                            Title="Report"
                        },
                        new Attachment(){
                            Language="ro",
                            ContentType="application/pdf",
                            Size=100,
                            Data=new byte[]{1,2,3,4,5,6,7},
                            Creation=model.Result.DocumentDateTime.ToString(Constants.DATE_FORMAT),
                            Hash=new byte[]{1,2,3},
                            Title="Report"
                        },
                    }
            };
        }

        private List<Observation> createObservations(Patient patient, ServiceRequest order, Organization performer, out List<Practitioner> validationDoctors)
        {
            var ret = new List<Observation>();
            var doctorsDict = new Dictionary<string, Practitioner>();

            foreach (var x in model.Result.ServiceResults)
            {

                var o = new Observation()
                {
                    Id = Uuid.Generate().ToString(),
                    Code = new CodeableConcept(Constants.SERVICES_CODING_SYSTEM, x.Id, $"[{x.Code}] {x.Name}"),
                    Status = ObservationStatus.Final,
                    Subject = patient.BundleRef(),
                    BasedOn = new List<ResourceReference>() { order.BundleRef() },
                    Category = new List<CodeableConcept>() { new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "laboratory") },
                    Issued = x.ResultDateTime,
                    Performer = new List<ResourceReference>(){
                    performer.BundleRef()
                    //TODO: validation responsible lab doctor
                },
                    DataAbsentReason = model.Result.IsPaid ? null : new CodeableConcept("http://terminology.hl7.org/CodeSystem/data-absent-reason", "masked"),
                    Component = model.Result.IsPaid ?
                    x.TestResultParameters.Select(p => buildValue(x.Id, p)).ToList()
                    : null
                };

                if (x.MedicalValidator != null)
                {
                    var labDoctorKey = JsonConvert.SerializeObject(x.MedicalValidator.Identifier);
                    if (!doctorsDict.ContainsKey(labDoctorKey))
                    {
                        var p = createPractitioner(x.MedicalValidator, Constants.PRACTITIONAL_TYPE_LAB);
                        doctorsDict.Add(labDoctorKey, p);
                    }
                    o.Performer.Add(doctorsDict[labDoctorKey].BundleRef());
                }
                ret.Add(o);
            }
            //update out list
            validationDoctors = new List<Practitioner>(doctorsDict.Values);
            return ret;
        }

        private List<Observation.ReferenceRangeComponent> buildReferenceRanges(fhir.model.ReferenceRange referenceRange)
        {
            if (referenceRange == null) return null;
            var ret = new Observation.ReferenceRangeComponent();
            ret.Low = referenceRange.NumericLower.HasValue ? new SimpleQuantity() { Value = referenceRange.NumericLower.Value } : null;
            ret.High = referenceRange.NumericUpper.HasValue ? new SimpleQuantity() { Value = referenceRange.NumericUpper.Value } : null;
            ret.Text = referenceRange.Text;
            return new List<Observation.ReferenceRangeComponent>() { ret };

        }

        private Observation.ComponentComponent buildValue(string serviceid, fhir.model.TestResultParameter p)
        {
            var ret = new Observation.ComponentComponent()
            {
                Code = new CodeableConcept(string.Format(Constants.SERVICE_PARAMETERS_CODING_SYSTEM, serviceid), p.Id, $"[{p.Code}] {p.Name}"),
                //Value = buildValue(p),
                Interpretation = p.Flags?.Select(f => new CodeableConcept(Constants.RESULT_FLAGS_SYSTEM, f.Code, f.Name)).ToList(),
                ReferenceRange = buildReferenceRanges(p.ReferenceRange)
            };


            if (p.Value.NumericValue.HasValue)
                ret.Value = new Quantity(p.Value.NumericValue.Value, p.Unit);
            else if (p.Value.TextValue.StartsWith("<") || p.Value.TextValue.StartsWith(">"))
            {
                try
                {
                    var nv = decimal.Parse(p.Value.TextValue.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
                    ret.Value = new Quantity(nv, p.Unit)
                    {
                        Comparator =
                        p.Value.TextValue.StartsWith("<") ? Quantity.QuantityComparator.LessThan : Quantity.QuantityComparator.GreaterThan
                    };
                }
                catch (Exception) { }
            }
            if (ret.Value == null)
                ret.Value = new FhirString(p.Value.TextValue);

            return ret;
        }

        private Contract createContract()
        {
            var d = model.Result.Order.Contract;
            return new Contract
            {
                Id = Uuid.Generate().ToString(),
                Name = d.Name,
                Identifier = new List<Identifier>(){
                    new Identifier(Constants.CONTRACT_SILABHQ_CODING_SYSTEM,d.Id),
                    new Identifier(Constants.CONTRACT_CODING_SYSTEM,d.Code),
                }
            };
        }



        private Organization createOrganization(fhir.model.Organization org, CodeableConcept type)
        {


            return new Organization
            {
                Id = Uuid.Generate().ToString(),
                Identifier = org.Identifier.Select(x => new Identifier(x.System, x.Value)).ToList(),
                Name = org.Name,
                Address = org.Address.Select(fhir.model.Address.toFhir).ToList(),
                Type = new List<CodeableConcept> { type }
            };
        }


        private ServiceRequest createOrder(Patient patient, Practitioner doctor, Organization performer, List<ResourceReference> supportingInfo)
        {
            var mo = model.Result.Order;


            var fhirOrder = new ServiceRequest
            {
                Id = Uuid.Generate().ToString(),
                AuthoredOn = mo.PlacementDateTime.ToString(Constants.DATE_FORMAT),
                Status = RequestStatus.Completed,
                Intent = RequestIntent.Order,
                Code = Constants.ORDER_PROCEDURE_REQUEST_CODE,
                Subject = patient.BundleRef(),
                Requester = doctor.BundleRef(),
                SupportingInfo = supportingInfo,
                Identifier = mo.Identifier.Select(x => new Identifier(x.System, x.Value)).Append(new Identifier(Constants.BARCODE_CODING_SYSTEM, mo.BarCode)).ToList(),
                OrderDetail = model.Result.ServiceResults.Select(sc => new CodeableConcept(Constants.SERVICES_CODING_SYSTEM, sc.Id)).ToList(),
                Performer = new List<ResourceReference> { performer.BundleRef() },
                Note = new List<Annotation> {
                    new Annotation { Text= mo.Comment}
                }
            };

            return fhirOrder;
        }




        private Patient createPatient()
        {
            var d = model.Result.Order.Patient;
            var p = new Patient()
            {
                Id = Uuid.Generate().ToString(),
                Gender = d.Gender.Equals("M") ? AdministrativeGender.Male : d.Gender.Equals("F") ? AdministrativeGender.Female : AdministrativeGender.Unknown,
                Name = new List<HumanName> { new HumanName { Family = d.Surname, Given = new[] { d.Name } } },
                Identifier = d.Identifier.Select(x => new Identifier(x.System, x.Value)).ToList(),
                BirthDate = d.Birthdate.ToString(Constants.DATE_FORMAT),
                Address = d.Address.Select(fhir.model.Address.toFhir).ToList(),
                Telecom = new List<ContactPoint>{
                    new ContactPoint(){System=ContactPoint.ContactPointSystem.Email,Value=d.Email},
                    new ContactPoint(){System=ContactPoint.ContactPointSystem.Phone,Value=d.PhoneNo},
                }
            };
            return p;
        }

        private Practitioner createPractitioner(fhir.model.Person d, String practitionerType)
        {
            var p = new Practitioner()
            {
                Id = Uuid.Generate().ToString(),
                Gender = d.Gender.Equals("M") ? AdministrativeGender.Male : d.Gender.Equals("F") ? AdministrativeGender.Female : AdministrativeGender.Unknown,
                Name = new List<HumanName> { new HumanName { Family = d.Surname, Given = new[] { d.Name } } },
                Identifier = d.Identifier.Select(x => new Identifier(x.System, x.Value)).ToList(),
                Extension = new List<Extension>() { new Extension(Constants.PRACTITIONAL_TYPE_EXTENSION, new CodeableConcept(Constants.PRACTITIONAL_TYPE_EXTENSION, practitionerType)) },
                BirthDate = d.Birthdate.ToString(Constants.DATE_FORMAT),
                Address = d.Address.Select(fhir.model.Address.toFhir).ToList(),
                Telecom = new List<ContactPoint>{
                    new ContactPoint(){System=ContactPoint.ContactPointSystem.Email,Value=d.Email},
                    new ContactPoint(){System=ContactPoint.ContactPointSystem.Phone,Value=d.PhoneNo},
                }
            };
            return p;
        }




    }
}
