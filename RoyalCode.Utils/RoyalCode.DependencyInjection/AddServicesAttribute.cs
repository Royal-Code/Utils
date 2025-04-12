using System.Diagnostics;

namespace RoyalCode.DependencyInjection;

/// <summary>
/// <para>
///     Utilizado em métodos estáticos e partial para geração do método que adiciona os serviços.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("COMPILE_TIME_ONLY")]
public class AddServicesAttribute() : Attribute { }