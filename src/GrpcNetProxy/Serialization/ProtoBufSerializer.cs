using ProtoBuf;
using System.IO;

namespace GrpcNetProxy.Serialization
{

    /// <summary>
    /// Protocol buffer serializer
    /// </summary>
    public class ProtoBufSerializer : ISerializer
    {

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] input)
        {
            using (var stream = new MemoryStream(input))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Serialize<T>(T input)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, input);
                return stream.ToArray();
            }
        }

    }
}
