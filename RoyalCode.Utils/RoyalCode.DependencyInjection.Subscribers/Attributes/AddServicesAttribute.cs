namespace RoyalCode.DependencyInjection.Subscribers.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AddServicesAttribute(string methodName) : Attribute { }