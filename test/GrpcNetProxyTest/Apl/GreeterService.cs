using Grpc.Core;
using GrpcNetProxyTest;
using System.Threading.Tasks;
using static GrpcNetProxyTest.Greeter;

namespace GrpcNetProxyTestApp.Apl
{

    /// <summary>
    /// Greeter service 
    /// </summary>
    public class GreeterService : GreeterBase
    {

        /// <summary>
        /// Sends a greeting
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        { 
            return Task.FromResult(new HelloReply
            {
                Message = "Great success!"
            });  
        }

    }
}
