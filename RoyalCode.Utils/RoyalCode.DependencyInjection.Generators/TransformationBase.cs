using Microsoft.CodeAnalysis;

namespace RoyalCode.DependencyInjection.Generators;

public class TransformationBase
{
    protected List<Diagnostic>? Errors { get; set; }

    protected void AddError(Diagnostic error)
    {
        Errors ??= [];
        Errors.Add(error);
    }

    protected void AddErrors(IEnumerable<Diagnostic> errors)
    {
        Errors ??= [];
        Errors.AddRange(errors);
    }

    protected bool EqualErrors(TransformationBase other)
    {
        if (Errors is null)
            return other.Errors is null;

        if (other.Errors is null)
            return false;

        return Errors.SequenceEqual(other.Errors);
    }

    public void ReportDiagnostic(SourceProductionContext spc)
    {
        bool hasErrors = Errors is not null && Errors.Count > 0;
        if (hasErrors)
            Errors!.ForEach(spc.ReportDiagnostic);
    }
}
