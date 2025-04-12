using System.Diagnostics;

namespace RoyalCode.DependencyInjection;

/// <summary>
/// The Service attribute is used in classes that must be registered in the Microsoft.Extensions.DependencyInjection.IServiceCollection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[Conditional("COMPILE_TIME_ONLY")]
public class ServiceAttribute : Attribute
{
    /// <summary>
    /// <para>
    ///     Interfaces that are not eligible as a service.
    /// </para>
    /// <para>
    ///     By default, <see cref="IDisposable"/> are not eligible.
    /// </para>
    /// </summary>
    public static ICollection<Type> IneligibleServicesInterfaces { get; } =
    [
        typeof(IDisposable)
    ];

    /// <summary>
    /// Create as Default.
    /// </summary>
    public ServiceAttribute() { }

    /// <summary>
    /// Create with specific services.
    /// </summary>
    /// <param name="specificServices">The specified services.</param>
    public ServiceAttribute(params Type[] specificServices) { }
}