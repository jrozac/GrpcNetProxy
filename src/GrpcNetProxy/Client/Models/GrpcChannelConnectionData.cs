namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc channel connection data
    /// </summary>
    public class GrpcChannelConnectionData
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }
    }
}
