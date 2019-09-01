namespace GrpcNetProxyTest.Setup
{

    /// <summary>
    /// Server setup
    /// </summary>
    public class ServerSetup
    {

        /// <summary>
        /// Enable status service 
        /// </summary>
        public bool EnableStatus { get; set; }

        /// <summary>
        /// Enable stats
        /// </summary>
        public bool EnableStats { get; set; }

        /// <summary>
        /// Server name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

    }
}
