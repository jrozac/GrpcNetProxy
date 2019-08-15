namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc client options
    /// </summary>
    public class GrpcClientOptions
    {

        /// <summary>
        /// Log requests 
        /// </summary>
        public bool LogRequests { get; set; } = true;

        /// <summary>
        /// Context key
        /// </summary>
        public string ContextKey { get; set; } = "X-ContextId";

        /// <summary>
        /// Request timeout in ms
        /// </summary>
        public int TimeoutMs { get; set; } = 10 * 1000;

    }
}
