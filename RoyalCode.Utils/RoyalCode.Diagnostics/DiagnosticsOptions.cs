namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// Opções de observação de diagnóstico.
    /// </summary>
    public class DiagnosticsOptions
    {
        private bool _enabled = true;

        /// <summary>
        /// Determina se a configuração é padrão.
        /// </summary>
        public bool IsDefaultConfiguration { get; private set; } = true;

        /// <summary>
        /// Se está habilitado.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                IsDefaultConfiguration = false;
            }
        }
    }
}
