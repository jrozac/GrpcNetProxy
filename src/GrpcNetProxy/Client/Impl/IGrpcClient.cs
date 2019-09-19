namespace GrpcNetProxy.Client
{
    /// <summary>
    /// Grpc client base interface
    /// </summary>
    public interface IGrpcClient
    {
        /// <summary>
        /// Configuration name
        /// </summary>
        string Name { get; }
    }
}
