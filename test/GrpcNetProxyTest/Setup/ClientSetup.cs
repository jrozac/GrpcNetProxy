namespace GrpcNetProxyTest.Setup
{

    /// <summary>
    /// Client setup
    /// </summary>
    public class ClientSetup
    {

        /// <summary>
        /// Enable status service 
        /// </summary>
        public bool EnableStatus { get; set; }

        /// <summary>
        /// Server name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int[] Ports { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;

    }
}
