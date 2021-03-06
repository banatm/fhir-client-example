// Generated by https://quicktype.io
//
// To change quicktype's target language, run command:
//
//   "Set quicktype target language"

namespace fhir.model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Hl7.Fhir.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class DraftJsonModel
    {
        [JsonProperty("event")]
        public Event Event { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public partial class Event
    {
        [JsonProperty("EmitedDateTime")]
        public DateTimeOffset EmitedDateTime { get; set; }

        [JsonProperty("ProcessedDateTime")]
        public DateTimeOffset ProcessedDateTime { get; set; }

        [JsonProperty("Producer")]
        public string Producer { get; set; }

        [JsonProperty("OrderHeaderId")]
        public long OrderHeaderId { get; set; }

        [JsonProperty("CorrelationID")]
        public string CorrelationId { get; set; }

        [JsonProperty("EventType")]
        public string EventType { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("filler")]
        public Organization Filler { get; set; }

        [JsonProperty("documentDateTime")]
        public DateTimeOffset DocumentDateTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("isPaid")]
        public bool IsPaid { get; set; }

        [JsonProperty("order")]
        public Order Order { get; set; }

        [JsonProperty("serviceResults")]
        public ServiceResult[] ServiceResults { get; set; }

        [JsonProperty("rawDocument")]
        public string RawDocument { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("identifier")]
        public Identifier[] Identifier { get; set; }
        [JsonProperty("barCode")]
        public string BarCode { get; set; }

        [JsonProperty("drawPoint")]
        public Organization DrawPoint { get; set; }

        [JsonProperty("placer")]
        public Organization Placer { get; set; }

        [JsonProperty("placementDateTime")]
        public DateTimeOffset PlacementDateTime { get; set; }

        [JsonProperty("patient")]
        public Person Patient { get; set; }

        [JsonProperty("contract")]
        public IDCodeValue Contract { get; set; }

        [JsonProperty("doctor")]
        public Person Doctor { get; set; }

        [JsonProperty("nurse")]
        public Person Nurse { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("clientUnit")]
        public Organization ClientUnit { get; set; }

    }

    public partial class IDCodeValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Person
    {
        [JsonProperty("identifier")]
        public Identifier[] Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("birthdate")]
        public DateTimeOffset Birthdate { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("address")]
        public Address[] Address { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNo")]
        public string PhoneNo { get; set; }
    }

    public partial class Address
    {
        [JsonProperty("streetName")]
        public string StreetName { get; set; }

        [JsonProperty("streetNo")]        
        public string StreetNo { get; set; }

        [JsonProperty("appartmentNo")]        
        public string AppartmentNo { get; set; }

        [JsonProperty("postCode")]
        public string PostCode { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        internal static Hl7.Fhir.Model.Address toFhir(Address x)
        {
           return new Hl7.Fhir.Model.Address{
                    City=x.City,
                    Country=x.CountryCode,
                    PostalCode=x.PostCode,
                    Line = new[]{$"{x.StreetName} {x.StreetNo} {x.AppartmentNo}"}                    
                };
        }
    }

    public partial class Identifier
    {
        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class Organization
    {
        [JsonProperty("identifier")]
        public Identifier[] Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

                [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("address")]
        public Address[] Address { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNo")]
        public string PhoneNo { get; set; }
    }

    public partial class ServiceResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("material")]
        public Material Material { get; set; }

        [JsonProperty("drawDateTime")]
        public DateTimeOffset DrawDateTime { get; set; }

        [JsonProperty("resultDateTime")]
        public DateTimeOffset ResultDateTime { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("medicalValidator")]
        public Person MedicalValidator { get; set; }

        [JsonProperty("testResultParameters")]
        public TestResultParameter[] TestResultParameters { get; set; }
    }

    public partial class Material
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("system")]
        public string System { get; set; }
    }

    public partial class TestResultParameter
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("value")]
        public Value Value { get; set; }

        [JsonProperty("referenceRange")]
        public ReferenceRange ReferenceRange { get; set; }

        [JsonProperty("flags")]
        public IDCodeValue[] Flags { get; set; }
    }

    public partial class ReferenceRange
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("numericLower")]
        public decimal? NumericLower { get; set; }

        [JsonProperty("numericUpper")]
        public decimal? NumericUpper { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("textValue")]
        public string TextValue { get; set; }

        [JsonProperty("numericValue")]
        public decimal? NumericValue { get; set; }
    }



    public partial class DraftJsonModel
    {
        public static DraftJsonModel FromJson(string json) => JsonConvert.DeserializeObject<DraftJsonModel>(json, fhir.model.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this DraftJsonModel self) => JsonConvert.SerializeObject(self, fhir.model.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    
    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
