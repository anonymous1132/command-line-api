﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Threading;

namespace System.CommandLine.Invocation
{
    internal class ServiceProvider : IServiceProvider
    {
        private readonly BindingContext _bindingContext;

        private readonly Dictionary<Type, Func<IServiceProvider, object>> _services;

        public ServiceProvider(BindingContext bindingContext)
        {
            _bindingContext = bindingContext ?? throw new ArgumentNullException(nameof(bindingContext));

            _services = new Dictionary<Type, Func<IServiceProvider, object>>
                        {
                            [typeof(ParseResult)] = _ => _bindingContext.ParseResult,
                            [typeof(IConsole)] = _ => _bindingContext.Console,
                            [typeof(CancellationToken)] = _ => CancellationToken.None,
                            [typeof(IHelpBuilder)] = _ => _bindingContext.ParseResult.Parser.Configuration.HelpBuilderFactory(_bindingContext),
                            [typeof(BindingContext)] = _ => _bindingContext
                        };
        }

        public void AddService<T>(Func<IServiceProvider, T> factory) => _services[typeof(T)] = p => factory(p);

        public void AddService(Type serviceType, Func<IServiceProvider, object> factory) => _services[serviceType] = factory;

        public IReadOnlyCollection<Type> AvailableServiceTypes => _services.Keys;

        public object GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var factory))
            {
                return factory(this);
            }

            return null;
        }
    }
}
