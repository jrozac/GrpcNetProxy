using System.Collections.Generic;

namespace GrpcNetProxy.Server.Models
{
    /// <summary>
    /// Grpc server info
    /// </summary>
    public class GrpcServerInfo
    {
        
        /// <summary>
        /// Internal constructor prevers outer createion instances
        /// </summary>
        internal GrpcServerInfo() { }

        /// <summary>
        /// Connection info 
        /// </summary>
        public class ConnectionInfo
        {
            internal ConnectionInfo() { }
            public string Url { get; set; }
            public int Port { get; set; }
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Services registered
        /// </summary>
        public List<string> Services { get; set; }

        /// <summary>
        /// Connections 
        /// </summary>
        public List<ConnectionInfo> Connections { get; set; }
        
    }
}
