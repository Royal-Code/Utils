using System.Diagnostics;

namespace RoyalCode.DependencyInjection;

/// <summary>
/// <para>
///     Utilizado em m�todos est�ticos e partial para gera��o do m�todo que adiciona os servi�os.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("COMPILE_TIME_ONLY")]
public class AddServicesAttribute() : Attribute { }