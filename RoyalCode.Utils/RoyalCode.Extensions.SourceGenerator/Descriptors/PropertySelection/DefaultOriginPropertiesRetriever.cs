namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

internal class DefaultOriginPropertiesRetriever : IOriginPropertiesRetriever
{
    public IReadOnlyList<PropertyDescriptor> GetProperties(TypeDescriptor origin)
    {
        return origin.CreateProperties(p => p.SetMethod is not null);
    }
}
