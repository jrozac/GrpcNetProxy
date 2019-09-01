using Grpc.Core;
using GrpcNetProxy.Generics;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.Logging;
using System;

namespace GrpcNetProxy.Server
{

    /// <summary>
    /// Server configurator
    /// </summary>
    public class ServerConfigurator
    {

        /// <summary>
        /// Server configuration
        /// </summary>
        internal GrpcServerConfiguration Configuration { get; } = new GrpcServerConfiguration();

        /// <summary>
        /// Options set
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ServerConfigurator SetOptions(GrpcServerOptions options)
        {
            Configuration.Options = options;
            return this;
        }

        /// <summary>
        /// Set on request start action
        /// </summary>
        /// <param name="onRequestStart"></param>
        /// <returns></returns>
        public ServerConfigurator SetOnRequestStartAction(Action<ILogger, ServerCallContext, RequestStartData> onRequestStart)
        {
            Configuration.OnRequestStart = onRequestStart;
            return this;
        }

        /// <summary>
        /// Set on request end action
        /// </summary>
        /// <param name="onRequestEnd"></param>
        /// <returns></returns>
        public ServerConfigurator SetOnRequestEndAction(Action<ILogger, ServerCallContext, RequestEndData> onRequestEnd)
        {
            Configuration.OnRequestEnd = onRequestEnd;
            return this;
        }

        /// <summary>
        /// Set context data
        /// </summary>
        /// <param name="contextSetter"></param>
        /// <returns></returns>
        public ServerConfigurator SetContext(Action<string> contextSetter)
        {
            Configuration.ContextSetter = contextSetter;
            return this;
        }

        /// <summary>
        /// Add service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public ServerConfigurator AddService<TService>() where TService : class
        {
            Configuration.ServicesTypes.Add(typeof(TService));
            return this;
        }

        /// <summary>
        /// Add service by full qualified type name 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public ServerConfigurator AddService(string service)
        {

            // resolve type
            var type = AppDomain.CurrentDomain.ResolveType(service);
            if (type == null || !type.IsInterface)
            {
                throw new ArgumentException($"Service interface type {service} is not vaild.");
            }

            // add service
            Configuration.ServicesTypes.Add(type);

            // return 
            return this;
        }

        /// <summary>
        /// Add status service
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public ServerConfigurator AddStatusService()
        {
            Configuration.ServicesTypes.Add(typeof(IStatusService));
            return this;
        }

        /// <summary>
        /// set host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public ServerConfigurator SetConnection(GrpcServerConnectionData host)
        {
            Configuration.Connection = host;
            return this;
        }

        /// <summary>
        /// Set configuration name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ServerConfigurator SetName(string name)
        {
            Configuration.Name = name;
            return this;
        }

    }
}
