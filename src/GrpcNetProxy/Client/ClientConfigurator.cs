using Grpc.Core;
using GrpcNetProxy.Generics;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Client configurator
    /// </summary>
    public class ClientConfigurator
    {
        #region Setup

        /// <summary>
        /// Client configuration
        /// </summary>
        internal GrpcClientConfiguration ClientConfiguration { get; set; } = new GrpcClientConfiguration();

        /// <summary>
        /// Add host 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ClientConfigurator AddHost(GrpcChannelConnectionData options)
        {
            // add 
            ClientConfiguration.Hosts.Add(options);
            return this;
        }

        /// <summary>
        /// Options set
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ClientConfigurator SetClientOptions(GrpcClientOptions options)
        {
            ClientConfiguration.Options = options;
            return this;
        }

        /// <summary>
        /// Set on request start action
        /// </summary>
        /// <param name="onRequestStart"></param>
        /// <returns></returns>
        public ClientConfigurator SetOnRequestStartAction(Action<ILogger, RequestStartData> onRequestStart)
        {
            ClientConfiguration.DataHandlers.OnRequestStart = onRequestStart;
            return this;
        }

        /// <summary>
        /// Set on request end action
        /// </summary>
        /// <param name="onRequestEnd"></param>
        /// <returns></returns>
        public ClientConfigurator SetOnRequestEndAction(Action<ILogger, RequestEndData> onRequestEnd)
        {
            ClientConfiguration.DataHandlers.OnRequestEnd = onRequestEnd;
            return this;
        }

        /// <summary>
        /// Set context data
        /// </summary>
        /// <param name="contextDataGetter"></param>
        /// <returns></returns>
        public ClientConfigurator SetContext(Func<string> contextGeter)
        {
            ClientConfiguration.DataHandlers.ContextGetter = contextGeter;
            return this;
        }

        /// <summary>
        /// Set name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ClientConfigurator SetName(string name)
        {
            ClientConfiguration.Name = name;
            return this;
        }

        #endregion

        #region Services

        /// <summary>
        /// Registered services
        /// </summary>
        internal List<Type> RegisteredServices { get; private set;} = new List<Type>();

        /// <summary>
        /// Add service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public ClientConfigurator AddService<TService>() where TService : class
        {
            RegisteredServices.Add(typeof(TService));
            return this;
        }

        /// <summary>
        /// Add service by full qualified type name 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public ClientConfigurator AddService(string service)
        {

            // resolve type
            var type = AppDomain.CurrentDomain.ResolveType(service);
            if(type == null || (!type.IsInterface && !type.GetInheritanceHierarchy().Any(t => t == typeof(ClientBase))))
            {
                throw new ArgumentException($"Service interface type {service} is not vaild.");
            }

            // add service
            RegisteredServices.Add(type);

            // return 
            return this;
        }

        #endregion
    }
}
