using RoyalCode.Core.Inject.Components;
using RoyalCode.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions de <see cref="IServiceCollection"/> e <see cref="IDiagnosticEventListenerBuilder"/>.
    /// </summary>
    public static class DiagnosticsServiceCollectionExtensions
    {
        /// <summary>
        /// Adiciona os serviços de observação de diagnostics e retorna um builder para configurar observadores de eventos de diagnóstico.
        /// </summary>
        /// <param name="services">Coleção dos serviços.</param>
        /// <returns>Uma instância de <see cref="IDiagnosticEventListenerBuilder"/> para configurar observadores de eventos de diagnóstico.</returns>
        public static IDiagnosticEventListenerBuilder AddDiagnosticListenerObserver(this IServiceCollection services)
        {
            if (!services.Any(d => d.ServiceType == typeof(DiagnosticListenerObserver)))
            {
                services.AddSingleton<DiagnosticListenerObserver>();
                services.AddSingleton<IBackgroundProcess, DiagnosticListenerObserverBackgroundProcessStartup>();
            }

            return new DefaultDiagnosticEventListenerBuilder(services);
        }

        /// <summary>
        /// <para>
        ///     Adicina um serviços para observar eventos de diagnóstico.
        /// </para>
        /// <para>
        ///     O ciclo de vida do serviço será singleton.
        /// </para>
        /// </summary>
        /// <typeparam name="TService">Tipo do serviço.</typeparam>
        /// <param name="builder"><see cref="IDiagnosticEventListenerBuilder"/>.</param>
        /// <returns>A mesma instância de <paramref name="builder"/> para chamadas encadeadas.</returns>
        public static IDiagnosticEventListenerBuilder AddObserver<TService>(this IDiagnosticEventListenerBuilder builder)
            where TService : class, IDiagnosticEventObserver
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticEventObserver, TService>());
            return builder;
        }

        /// <summary>
        /// Aplica configurações ao <see cref="DiagnosticsOptions"/>.
        /// </summary>
        /// <param name="services">Coleção de serviços.</param>
        /// <param name="configure">Action para configurações.</param>
        /// <returns>A mesma instância de <paramref name="services"/> para chamadas encadeadas.</returns>
        public static IServiceCollection ConfigureDiagnosticsOptions(this IServiceCollection services,
            Action<DiagnosticsOptions> configure)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            return services.Configure(configure);
        }
    }
}
