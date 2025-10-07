namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

public interface ITargetPropertiesRetriever
{
    IReadOnlyList<PropertyDescriptor> GetProperties(TypeDescriptor target);
}
