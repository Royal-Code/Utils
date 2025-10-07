namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

public interface IOriginPropertiesRetriever
{
    IReadOnlyList<PropertyDescriptor> GetProperties(TypeDescriptor origin);
}
