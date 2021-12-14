using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.Diagnostics
{
    /// <summary>
    /// Factory de <see cref="EventArgumentGetter{TArgument}"/>.
    /// </summary>
    public static class EventArgumentGetterFactory
    {
        private static readonly ConcurrentDictionary<Tuple<Type, Type>, Delegate> typeFunctions = new();
        private static readonly ConcurrentDictionary<Tuple<Type, Type, string>, Delegate> propertyFunctions = new();

        /// <summary>
        /// Cria ou obtém um <see cref="EventArgumentGetter{TArgument}"/> para um tipo de objeto de argumentos
        /// de eventos de diagnóstico, podendo informar o nome da propriedade que será obtida.
        /// </summary>
        /// <typeparam name="TArgument">Tipo do argumento requerido.</typeparam>
        /// <param name="eventArgsType">Tipo do objeto de argumentos de eventos.</param>
        /// <param name="property">Nome da propriedade, opcional.</param>
        /// <returns>
        ///     Um <see cref="EventArgumentGetter{TArgument}"/> 
        ///     para obter o argumento requerido do objeto de argumentos de eventos.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Caso não seja possível encontrar uma propriedade par ao argumento requerido.
        /// </exception>
        public static EventArgumentGetter<TArgument> Get<TArgument>(Type eventArgsType, string? property = null)
        {
            if (property is null)
            {
                var key = new Tuple<Type, Type>(eventArgsType, typeof(TArgument));
                return (EventArgumentGetter<TArgument>)typeFunctions.GetOrAdd(key, k => Create<TArgument>(k));
            }
            else
            {
                var key = new Tuple<Type, Type, string>(eventArgsType, typeof(TArgument), property);
                return (EventArgumentGetter<TArgument>)propertyFunctions.GetOrAdd(key, k => Create<TArgument>(k));
            }
        }

        private static EventArgumentGetter<TArgument> Create<TArgument>(Tuple<Type, Type, string> key)
        {
            var isArgumentAdapter = typeof(TArgument).IsDefined(typeof(ArgumentAdapterAttribute));

            var member = key.Item1.GetTypeInfo()
                .GetRuntimeProperties()
                .Where(p => p.CanRead)
                .Where(p => p.Name.Equals(key.Item3, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(p => isArgumentAdapter || key.Item2.IsAssignableFrom(p.PropertyType));

            if (member == null)
                throw new InvalidOperationException(
                    $"The event arguments of type '{key.Item1.FullName}' do not have the property '{key.Item3}' of type '{key.Item2.FullName}'");

            return isArgumentAdapter
                ? CreateAdapter<TArgument>(key.Item1, member)
                : Create<TArgument>(key.Item1, member);
        }

        private static EventArgumentGetter<TArgument?> Create<TArgument>(Tuple<Type, Type> key)
        {
            var type = key.Item1;
            if (typeof(TArgument).IsAssignableFrom(type))
            {
                return eventArgs => (TArgument)eventArgs;
            }

            if (type == typeof(DiagnosticOperation))
            {
                return eventArgs => ((DiagnosticOperation)eventArgs).TryGetItem<TArgument>();
            }

            if (typeof(TArgument).IsDefined(typeof(ArgumentAdapterAttribute)))
            {
                return CreateAdapter<TArgument?>(type);
            }

            var member = type.GetTypeInfo()
                .GetRuntimeProperties()
                .Where(p => p.CanRead)
                .FirstOrDefault(p => typeof(TArgument).IsAssignableFrom(p.PropertyType));

            if (member == null)
                throw new InvalidOperationException(
                    $"The event arguments of type '{type.FullName}' do not have an property of type '{typeof(TArgument).FullName}'");

            return Create<TArgument?>(type, member);
        }

        private static EventArgumentGetter<TArgument> Create<TArgument>(Type type, PropertyInfo member)
        {
            var param = Expression.Parameter(typeof(object), "eventArgs");
            var cast = Expression.Convert(param, type);
            var access = Expression.MakeMemberAccess(cast, member);

            Expression body = member.PropertyType.IsEnum && typeof(TArgument) != type
                ? Expression.Convert(access, typeof(TArgument))
                : (Expression)access;

            return Expression.Lambda<EventArgumentGetter<TArgument>>(body, param).Compile();
        }

        private static EventArgumentGetter<TArgument> CreateAdapter<TArgument>(Type type, PropertyInfo? member = null)
        {
            var argAttr = typeof(TArgument).GetCustomAttribute<ArgumentAdapterAttribute>();
            if (argAttr.GetFromTypes != null)
            {
                if (argAttr.GetFromTypes.Length == 0)
                    throw new InvalidOperationException("The array of property 'GetFromTypes' must contain an element.");

                var types = new Type[argAttr.GetFromTypes.Length];
                for (int i = 0; i < argAttr.GetFromTypes.Length; i++)
                {
                    types[i] = Type.GetType(argAttr.GetFromTypes[i], false);
                }

                return CreateAdapterFromTypes<TArgument>(types, member);
            }
            else
            {
                return CreateAdapterOfType<TArgument>(type, member);
            }
        }

        private static EventArgumentGetter<TArgument> CreateAdapterOfType<TArgument>(Type type, PropertyInfo? member)
        {
            var varType = member?.PropertyType ?? type;

            var param = Expression.Parameter(typeof(object), "eventArgs");
            var cast = Expression.Convert(param, type);
            var variable = Expression.Variable(varType, "variable");

            var assignVariable = Expression.Assign(
                variable,
                member == null
                    ? cast
                    : (Expression)Expression.MakeMemberAccess(cast, member));

            var newExpression = GetNewExpression<TArgument>(varType, variable);

            var body = Expression.Block(
                typeof(TArgument),
                new ParameterExpression[] { variable },
                assignVariable, newExpression);

            return Expression.Lambda<EventArgumentGetter<TArgument>>(body, param).Compile();
        }

        private static EventArgumentGetter<TArgument> CreateAdapterFromTypes<TArgument>(Type[] types, PropertyInfo? member)
        {
            var param = Expression.Parameter(typeof(object), "eventArgs");
            var variables = new List<ParameterExpression>();
            var commands = new List<Expression>();

            ParameterExpression variable;
            var value = Expression.Variable(typeof(TArgument), "value");
            variables.Add(value);
            if (member != null)
            {
                variable = Expression.Variable(member.PropertyType, "variable");
                variables.Add(variable);

                var cast = Expression.Convert(param, member.DeclaringType);
                var assignVariable = Expression.Assign(variable, Expression.MakeMemberAccess(cast, member));
                commands.Add(assignVariable);
            }
            else
            {
                variable = param;
            }

            Expression lastCommand = Expression.Assign(value, Expression.New(typeof(TArgument)));
            foreach (var type in types.Reverse().Where(t => t != null))
            {
                var ifCommands = new List<Expression>();

                var castVar = Expression.Parameter(type, type.Name);
                variables.Add(castVar);

                var assign = Expression.Assign(castVar, Expression.Convert(variable, type));
                ifCommands.Add(assign);

                var newExpression = GetNewExpression<TArgument>(type, castVar);
                assign = Expression.Assign(value, newExpression);
                ifCommands.Add(assign);

                var command = Expression.IfThenElse(
                    Expression.TypeIs(variable, type),
                    Expression.Block(ifCommands),
                    lastCommand);

                lastCommand = command;
            }

            commands.Add(lastCommand);
            commands.Add(value); // return

            var body = Expression.Block(
                typeof(TArgument),
                variables,
                commands);

            return Expression.Lambda<EventArgumentGetter<TArgument>>(body, param).Compile();
        }

        private static Expression GetNewExpression<TArgument>(Type varType, Expression variable)
        {
            var properties = typeof(TArgument).GetTypeInfo()
                .GetRuntimeProperties()
                .Where(p => p.CanWrite)
                .Where(p => p.IsDefined(typeof(GetFromArgumentAttribute)));

            var binds = new List<MemberBinding>();

            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<GetFromArgumentAttribute>();
                var selection = PropertySelection.Select(varType, attr.PropertyName ?? property.Name, attr.Required);
                if (selection != null)
                {
                    var memberAccess = selection.GetAccessExpression(variable);
                    var init = Expression.Bind(property, memberAccess);
                    binds.Add(init);
                }
            }

            return Expression.MemberInit(
                Expression.New(typeof(TArgument)),
                binds);
        }
    }
}
