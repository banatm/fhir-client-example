using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace fhir
{
    class Constants
    {
        public static readonly List<CodeableConcept> PATIENT_QUESTION_OBSERVATION_CATEGORY =
              new List<CodeableConcept> { new CodeableConcept("http://hl7.org/fhir/ValueSet/observation-category", "social-history") };
        public static readonly CodeableConcept ORDER_PROCEDURE_REQUEST_CODE =
            new CodeableConcept("http://snomed.info/sct", "108252007", "Laboratory procedure");



        internal static readonly ResourceReference CURRENT_BDP_REFERENCE =
             new ResourceReference("https://dia.medicover.com/retail/Organization/BDP-PL-GPP1");
        internal static readonly ResourceReference CURRENT_LAB_REFERENCE =
            new ResourceReference("https://dia.medicover.com/logistics/Organization/LAB-PL-GDANSK");

            
        internal static readonly ResourceReference BDP_REFERENCE =
             new ResourceReference("https://dia.medicover.com/retail/Organization/");
        internal static readonly string LABS_CODING_SYSTEM =
            "https://dia.medicover.com/logistics/Organization/";

        internal static readonly string ORGTYPE_CODING_SYSTEM =
            "https://dia.medicover.com/OrganizationType/";

        internal static readonly string ORGTYPE_LAB ="Laboratory";

        public static readonly string ORGTYPE_CLIENT ="Client";
        internal static readonly string ORGTYPE_CLIENT_UNIT = "ClientUnit";
        internal static readonly string CONTRACT_SILABHQ_CODING_SYSTEM = "https://dia.medicover.com/Silab/RO/HQ";
        internal static readonly string CONTRACT_CODING_SYSTEM = "https://dia.medicover.com/retail/Contract";
        internal static readonly string BARCODE_CODING_SYSTEM = "https://dia.medicover.com/barcode";

        internal static readonly string SERVICES_CODING_SYSTEM = "https://dia.medicover.com/serviceknowledgebase/service";
        internal static readonly string SERVICE_PARAMETERS_CODING_SYSTEM = "https://dia.medicover.com/serviceknowledgebase/service/{0}/parameters";
        internal static readonly string RESULT_FLAGS_SYSTEM = "https://dia.medicover.com/serviceknowledgebase/interpretation_flags";

        internal static readonly string PRACTITIONAL_TYPE_EXTENSION = "https://dia.medicover.com/organization/practitioner/type";
        internal static readonly string PRACTITIONAL_TYPE_DOCTOR = "RefferingDoctor";
        internal static readonly string PRACTITIONAL_TYPE_LAB = "LaboratoryDoctor";
        internal static readonly string DATE_FORMAT="yyyy-MM-ddTHH:mm:ss";
    }
}
