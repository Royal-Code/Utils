namespace RoyalCode.DependencyInjection;

/// <summary>
/// <para>
///     Utilizado em métodos estáticos e partial para geração do método que adiciona os serviços.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AddServicesAttribute() : Attribute { }