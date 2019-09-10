using GrpcNetProxy.Client;
using GrpcNetProxy.Server;
using System;
using System.Collections.Generic;

namespace GrpcNetProxyTest.Scenarios
{
    /// <summary>
    /// Delegates scenario. 
    /// Use to test action delegats on server and client (context set, context get, on start event, etc.)
    /// </summary>
    public class DelegatesScenario : DefaultScenario
    {

        /// <summary>
        /// Server context list
        /// </summary>
        public List<string> ServerContextList { get; set; } = new List<string>();

        /// <summary>
        /// Clietn contexts ids list
        /// </summary>
        public List<string> ClientContextList { get; set; } = new List<string>();

        /// <summary>
        /// Server custom setup
        /// </summary>
        public override Action<string, ServerConfigurator> ServerCustomSetup => (name, cfg) =>
        {
            cfg.SetContext(ctxt => {
                ServerContextList.Add(ctxt);
            });
        };

        /// <summary>
        /// Client custom setup
        /// </summary>
        public override Action<string, ClientConfigurator> ClientCustomSetup => (name, cfg) => {
            cfg.SetContext(() => {
                var ctxt = Guid.NewGuid().ToString();
                ClientContextList.Add(ctxt);
                return ctxt;
            });
        };


    }
}
