using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace fhir
{
    public static class ResourceExtension
    {
        public static ResourceReference GlobalURLReference<T>(this T source, bool useFullPath) where T : Resource
        {
            return
                useFullPath ?
                    new ResourceReference(ResourceIdentity.Build(source.ResourceBase, typeof(T).GetCollectionName(), source.Id).ToString())
                    : new ResourceReference($"{typeof(T).GetCollectionName()}/{source.Id}");


        }

        public static Identifier GlobalURLIdentifier<T>(this T source, bool useFullPath) where T : Resource
        {
            return
                useFullPath ?
                    new Identifier($"{ source.ResourceBase.ToString()}/{typeof(T).GetCollectionName()}", source.Id)
                    : new Identifier(typeof(T).GetCollectionName(),source.Id);
        }
    }
}
