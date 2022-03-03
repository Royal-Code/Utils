using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// <para>
    ///     Este componente encapsula uma instância de <see cref="System.Diagnostics.Activity"/> e de
    ///     <see cref="DiagnosticListener"/>, realizando a inicialização e parada da operação.
    /// </para>
    /// <para>
    ///     Também é possível adicionar itens para compartilhar com os listeners.
    /// </para>
    /// </summary>
    public sealed class DiagnosticOperation : IDisposable
    {
        private readonly bool isDiagnosticsEnabled;
        private readonly Dictionary<Type, object> items;
        private bool isStarted;
        private bool isDisposed;

        /// <summary>
        /// Instância de <see cref="System.Diagnostics.Activity"/> da operação.
        /// </summary>
        public Activity Activity { get; }

        /// <summary>
        /// Escuta de diagnóstico.
        /// </summary>
        public DiagnosticListener Listener { get; }

        /// <summary>
        /// Nome da operação.
        /// </summary>
        public string OperationName => Activity.OperationName;

        /// <summary>
        /// Cria nova operação.
        /// </summary>
        /// <param name="listener">Escuta de diagnóstico.</param>
        /// <param name="operationName">Nome da operação.</param>
        public DiagnosticOperation(DiagnosticListener listener, string operationName)
        {
            Listener = listener ?? throw new ArgumentNullException(nameof(listener));

            Activity = new Activity(operationName);

            isDiagnosticsEnabled = listener.IsEnabled(operationName);

            items = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Dispara um evento.
        /// </summary>
        /// <param name="name">Nome do evento.</param>
        public void FireEvent(string name)
        {
            if (isDiagnosticsEnabled)
            {
                Listener.Write(name, this);
            }
        }

        /// <summary>
        /// Dispara um evento de erro a partir de uma exception que, se informada, será adicionada aos itens.
        /// </summary>
        /// <typeparam name="TException">Tipo da exception capturada.</typeparam>
        /// <param name="ex">Exception, opcional.</param>
        public void FireError<TException>(TException? ex = null)
            where TException : Exception
        {
            if (isDiagnosticsEnabled)
            {
                if (ex is not null)
                    if (typeof(Exception) == typeof(TException))
                        AddItem(ex);
                    else
                        With(ex).Add().AddAs<Exception>();

                Listener.Write($"{Activity.OperationName}.Error", this);
            }
        }

        /// <summary>
        /// Add the object as item of type <typeparamref name="TItem"/>.
        /// </summary>
        /// <typeparam name="TItem">The item type.</typeparam>
        /// <param name="item">The item object.</param>
        public void AddItem<TItem>(TItem item) => items.Add(typeof(TItem), item ?? throw new ArgumentNullException(nameof(item)));

        /// <summary>
        /// Get the item of the type, when not found, returns default.
        /// </summary>
        /// <typeparam name="TItem">The item type.</typeparam>
        /// <returns>The item object or default</returns>
        public TItem? TryGetItem<TItem>()
            => (TItem?)(items.TryGetValue(typeof(TItem), out var item) ? item : InternalTryGetItem(typeof(TItem)));

        /// <summary>
        /// Get the item of the type, when not found, returns default.
        /// </summary>
        /// <param name="type">The item type.</param>
        /// <returns>The item object or default</returns>
        public object? TryGetItem(Type type)
            => items.TryGetValue(type, out var item) ? item : InternalTryGetItem(type);

        

        /// <summary>
        /// Get the item of the type, when not found, create.
        /// </summary>
        /// <typeparam name="TItem">The item type.</typeparam>
        /// <param name="creation">The item object.</param>
        /// <returns></returns>
        public TItem GetOrCreateItem<TItem>(Func<TItem> creation)
        {
            if (!items.TryGetValue(typeof(TItem), out var item))
            {
                item = creation();

                if (item is null)
                    throw new InvalidOperationException("The item returned by creation function is null");

                items.Add(typeof(TItem), item);
            }

            return (TItem)item;
        }

        /// <summary>
        /// Cria um objeto de <see cref="WithItem{TItem}"/> para adicionar uma instância como vários tipos de itens.
        /// </summary>
        /// <typeparam name="TItem">Tipo da instância.</typeparam>
        /// <param name="item">Instância de item.</param>
        /// <returns>Componente para adicionar a instância de item como vários tipos.</returns>
        public WithItem<TItem> With<TItem>(TItem item)
        {
            return new WithItem<TItem>(item, items);
        }

        /// <summary>
        /// Cria um operação filha, com opções para compartilhar os items e inicializá-la.
        /// </summary>
        /// <param name="operationName">Nome da operação.</param>
        /// <param name="copyItems">Se deve copiar os itens, por padrão é verdadeiro.</param>
        /// <param name="start">Se deve iniciar a operação.</param>
        /// <returns>Nova instância de <see cref="DiagnosticOperation"/>.</returns>
        public DiagnosticOperation Child(string operationName, bool copyItems = true, bool start = false)
        {
            var newOperation = new DiagnosticOperation(Listener, operationName);
            if (copyItems)
                foreach (var item in items)
                    newOperation.items.Add(item.Key, item.Value);

            if (start)
                newOperation.Start();

            return newOperation;
        }

        /// <summary>
        /// Cria um operação filha de erro, utilizando o mesmo nome da operação com o sufixo "Error".
        /// </summary>
        /// <param name="ex">Exception do erro.</param>
        /// <param name="start">Se deve iniciar a operação, por padrão é verdadeiro.</param>
        /// <returns>Nova instância de <see cref="DiagnosticOperation"/>.</returns>
        public DiagnosticOperation ChildError<TException>(TException ex, bool start = true)
            where TException : Exception
        {
            if (ex is null)
                throw new ArgumentNullException(nameof(ex));

            var newOperation = new DiagnosticOperation(Listener, $"{OperationName}Error");
            foreach (var item in items)
                newOperation.items.Add(item.Key, item.Value);

            if (typeof(Exception) == typeof(TException))
                newOperation.AddItem(ex);
            else
                newOperation.With(ex).Add().AddAs<Exception>();

            if (start)
                newOperation.Start();

            return newOperation;
        }

        /// <summary>
        /// Inicia a <see cref="Activity"/> utilizando o <see cref="DiagnosticListener"/> quando habilitado.
        /// </summary>
        public void Start()
        {
            GuardDisposed();

            if (!isStarted)
            {
                isStarted = true;

                if (isDiagnosticsEnabled)
                {
                    Listener.StartActivity(Activity, this);
                }
                else
                {
                    Activity.Start();
                }
            }
        }

        /// <summary>
        /// <para>
        ///     Para (stop) a <see cref="Activity"/> quando iniciada,
        ///     utilizando o <see cref="DiagnosticListener"/> quando habilitado.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>
        ///     Quando já descartada anteriormente.
        /// </para>
        /// </exception>
        public void Dispose()
        {
            GuardDisposed();

            isDisposed = true;

            if (isStarted)
            {
                if (isDiagnosticsEnabled)
                {
                    Listener.StopActivity(Activity, this);
                }
                else
                {
                    Activity.Stop();
                }
            }
        }

        private void GuardDisposed()
        {
            if (isDisposed)
            {
                throw new InvalidOperationException("The operation was discarded and stopped previously.");
            }
        }

        private object? InternalTryGetItem(Type type)
        {
            foreach (var item in items.Values)
            {
                if (type.IsAssignableFrom(item.GetType()))
                    return item;
            }
            return default;
        }

        /// <summary>
        /// Componente para adicionar uma instância como vários tipos de itens a um <see cref="DiagnosticOperation"/>.
        /// </summary>
        /// <typeparam name="TItem">Tipo da instância do item.</typeparam>
        public sealed class WithItem<TItem>
        {
            private readonly TItem item;
            private readonly Dictionary<Type, object> items;

            /// <summary>
            /// Cria novo componente.
            /// </summary>
            /// <param name="item">Instância do item.</param>
            /// <param name="items">Itens do <see cref="DiagnosticOperation"/>.</param>
            internal WithItem(TItem item, Dictionary<Type, object> items)
            {
                this.item = item ?? throw new ArgumentNullException(nameof(item));
                this.items = items ?? throw new ArgumentNullException(nameof(items));
            }

            /// <summary>
            /// Adiciona o item como próprio tipo.
            /// </summary>
            /// <returns>A mesma instância para chamadas encadeadas.</returns>
            public WithItem<TItem> Add()
            {
                items.Add(typeof(TItem), item!);
                return this;
            }

            /// <summary>
            /// Adiciona o item como o tipo especificado.
            /// </summary>
            /// <typeparam name="TItemType">Tipo a ser adicionado o item.</typeparam>
            /// <returns>A mesma instância para chamadas encadeadas.</returns>
            public WithItem<TItem> AddAs<TItemType>()
            {
                if (!typeof(TItemType).IsAssignableFrom(item!.GetType()))
                {
                    throw new ArgumentException(
                        $"The current item is of type '{item.GetType().FullName}' " +
                        $"and does not implement the other type informed that is '{typeof(TItemType).FullName}'.");
                }

                items.Add(typeof(TItemType), item);
                return this;
            }
        }
    }
}
