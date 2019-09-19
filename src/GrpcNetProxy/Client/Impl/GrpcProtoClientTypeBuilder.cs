using GrpcNetProxy.Generics;
using System.Reflection;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Type builder for proto client 
    /// </summary>
    internal class GrpcProtoClientTypeBuilder
    {
        /// <summary>
        /// Create type
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TypeInfo Create<TService>() where TService : class
        {
            var typeBuilder = typeof(TService).NewBuilderForNestedType(typeof(TService) + "Internal");
            typeBuilder.AddInterfaceImplementation(typeof(IGrpcClient));
            typeBuilder.AddProperty(typeof(string), "Name");
            var type = typeBuilder.CreateTypeInfo();
            return type;
        }
    }

}
