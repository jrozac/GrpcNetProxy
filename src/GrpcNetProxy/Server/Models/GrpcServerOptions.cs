namespace GrpcNetProxy.Server
{
    /// <summary>
    /// Grpc server options
    /// </summary>
    public class GrpcServerOptions
    {

        /// <summary>
        /// Log requests 
        /// </summary>
        public bool LogRequests { get; set; } = true;

        /// <summary>
        /// Stats enable
        /// </summary>
        public bool StatsEnabled { get; set; } = true;

        /// <summary>
        /// Context key
        /// </summary>
        public string ContextKey { get; set; } = "X-ContextId";
    }
}
