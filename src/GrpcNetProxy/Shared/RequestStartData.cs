namespace GrpcNetProxy.Shared
{

    /// <summary>
    /// Request start data
    /// </summary>
    public class RequestStartData
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Method name
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public object Request { get; set; }

    }
}
