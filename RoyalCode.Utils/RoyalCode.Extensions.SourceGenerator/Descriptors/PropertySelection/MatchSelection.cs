using Microsoft.CodeAnalysis;
using RoyalCode.Extensions.SourceGenerator.Descriptors.Assignments;

namespace RoyalCode.Extensions.SourceGenerator.Descriptors.PropertySelection;

/// <summary>
/// <para>
///     The result of matching the properties of an origin type against a target type.
/// </para>
/// <para>
///     This is the working model of the matching: it holds Roslyn symbols (which would keep the whole
///     <see cref="Compilation"/> alive) and mutable state, so it must not cross into an incremental generator
///     pipeline, nor be used as a cache key — it has no value equality.
/// </para>
/// <para>
///     Use it while resolving the match, then convert it with
///     <see cref="Snapshots.MatchSelectionSnapshotFactory.Create(MatchSelection)"/>. The resulting
///     <see cref="Snapshots.MatchSelectionSnapshot"/> is immutable, symbol-free and has value equality, which
///     is what the pipeline needs to cache and compare between builds:
/// </para>
/// <code>
///     var snapshot = MatchSelectionSnapshotFactory.Create(matchSelection);
///     // snapshot pode ser retornado de um provider e comparado entre builds
/// </code>
/// </summary>
public class MatchSelection
{
    #region Factory

    public static MatchSelection Create(
        TypeDescriptor origin, 
        TypeDescriptor target,
        SemanticModel model,
        MatchOptions? options = null)
    {
        options ??= MatchOptions.InternalDefault;

        var originProperties = options.OriginPropertiesRetriever.GetProperties(origin);
        var targetProperties = options.TargetPropertiesRetriever.GetProperties(target);

        return Create(origin, originProperties, target, targetProperties, model, options);
    }

    public static MatchSelection Create(
        TypeDescriptor origin, IReadOnlyList<PropertyDescriptor> originProperties,
        TypeDescriptor target, IReadOnlyList<PropertyDescriptor> targetProperties,
        SemanticModel model,
        MatchOptions options)
    {
        List<PropertyMatch> matches = [];

        var targetType = new MatchTypeInfo(target, targetProperties, options);

        foreach (var originProperty in originProperties)
        {
            // determina o nome da propriedade de origem a ser procurada no target
            string? originPropertyName = null;
            if (options.PropertyNameResolvers is not null && originProperty.Symbol is not null)
            {
                foreach (var resolver in options.PropertyNameResolvers)
                {
                    if (resolver.TryResolvePropertyName(originProperty.Symbol, out var resolvedName))
                    {
                        originPropertyName = resolvedName!;
                        break;
                    }
                }
            }
            originPropertyName ??= originProperty.Name;

            // para cada propriedade, seleciona a propriedade correspondente no target
            var targetSelection = PropertySelection.Select(originPropertyName, targetType);

            // se a propriedade for encontrada, avalia os tipos entre elas e a forma de atribuíção.
            AssignDescriptor? assignDescriptor = targetSelection is not null
                ? AssignDescriptorFactory.Create(
                    originProperty.Type,
                    targetSelection.PropertyType.Type,
                    model,
                    options)
                : null;

            // por fim, cria o match entre as propriedades, mesmo que não tenha sido encontrado.
            matches.Add(new PropertyMatch(originProperty, targetSelection, assignDescriptor));
        }

        return new MatchSelection(origin, target, matches);
    }

    #endregion

    private readonly TypeDescriptor originType;
    private readonly TypeDescriptor targetType;
    private readonly IReadOnlyList<PropertyMatch> propertyMatches;

    public MatchSelection(
        TypeDescriptor originType, 
        TypeDescriptor targetType,
        IReadOnlyList<PropertyMatch> propertyMatches)
    {
        this.originType = originType;
        this.targetType = targetType;
        this.propertyMatches = propertyMatches;
    }

    public TypeDescriptor OriginType => originType;

    public TypeDescriptor TargetType => targetType;

    public IReadOnlyList<PropertyMatch> PropertyMatches => propertyMatches;

    public bool HasMissingProperties(out IReadOnlyList<PropertyDescriptor> missingProperties)
    {
        var missing = propertyMatches.Where(m => m.IsMissing).Select(m => m.Origin).ToList();
        missingProperties = missing.AsReadOnly();
        return missing.Count > 0;
    }

    public bool HasNotAssignableProperties(out IReadOnlyList<PropertyMatch> notAssignableProperties)
    {
        var notAssignable = propertyMatches.Where(m => !m.IsMissing && !m.CanAssign).ToList();
        notAssignableProperties = notAssignable.AsReadOnly();
        return notAssignable.Count > 0;
    }

    public void AddParentProperty(PropertySelection parent)
    {
        foreach (var propertyMatch in propertyMatches)
        {
            propertyMatch.Target?.WithParent(parent);
        }
    }

}
