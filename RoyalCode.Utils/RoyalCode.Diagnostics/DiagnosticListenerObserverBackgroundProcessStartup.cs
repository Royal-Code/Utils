using RoyalCode.Core.Inject.Components;
using System.Diagnostics;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// Background Process para inicialização do <see cref="DiagnosticListenerObserver"/>.
    /// </summary>
    public class DiagnosticListenerObserverBackgroundProcessStartup : IBackgroundProcess
    {
        /// <summary>
        /// Cria o Background Process inicializando o <see cref="DiagnosticListenerObserver"/>.
        /// </summary>
        /// <param name="observer"><see cref="DiagnosticListenerObserver"/>.</param>
        public DiagnosticListenerObserverBackgroundProcessStartup(DiagnosticListenerObserver observer)
        {
            DiagnosticListener.AllListeners.Subscribe(observer);
        }
    }
}
