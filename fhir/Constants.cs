using Hl7.Fhir.Model;
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
    }
}
