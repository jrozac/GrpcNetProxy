namespace GrpcNetProxy.Serialization
{

    /// <summary>
    /// Serializer interface 
    /// </summary>
    public interface ISerializer
    {

        /// <summary>
        /// Serialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        byte[] Serialize<T>(T input);

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        T Deserialize<T>(byte[] input);
    }
}
