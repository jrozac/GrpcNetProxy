using Grpc.Core;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Invoker bundle
    /// </summary>
    internal class InvokerBundle
    {

        /// <summary>
        /// Constructor with channel and invoker
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="invoker"></param>
        /// <param name="connectionData"></param>
        public InvokerBundle(Channel channel, CallInvoker invoker, GrpcChannelConnectionData connectionData)
        {
            Channel = channel;
            Invoker = invoker;
            ConnectionData = connectionData;
        }

        /// <summary>
        /// Grpc channel
        /// </summary>
        public Channel Channel { get; private set; }

        /// <summary>
        /// Invoker
        /// </summary>
        public CallInvoker Invoker { get; private set; }

        /// <summary>
        /// Connection data
        /// </summary>
        public GrpcChannelConnectionData ConnectionData { get; set; }
    }
}
