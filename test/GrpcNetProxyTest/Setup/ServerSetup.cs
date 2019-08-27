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
        /// Server name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public int Port { get; set; }

    }
}
