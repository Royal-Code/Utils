namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

internal class DefaultTargetPropertiesRetriever : ITargetPropertiesRetriever
{
    public IReadOnlyList<PropertyDescriptor> GetProperties(TypeDescriptor target)
    {
        return target.CreateProperties(p => p.GetMethod is not null);
    }
}