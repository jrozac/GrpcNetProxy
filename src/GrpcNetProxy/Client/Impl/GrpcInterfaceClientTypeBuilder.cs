using GrpcNetProxy.Generics;
using System.Linq;
using System.Reflection;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Client type builder. Used to build client class type according to interface definition
    /// </summary>
    internal static class GrpcInterfaceClientTypeBuilder
    {

        /// <summary>
        /// Create grpc client type 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TypeInfo Create<TService>() where TService : class
        {
            // create type builder with extend GrpcInterfaceClientBase class
            var serviceType = typeof(TService);
            var typeBuilder = typeof(GrpcInterfaceClientBase)
                .NewBuilderForNestedType(serviceType.Name + "Client", BindingFlags.NonPublic | BindingFlags.Instance);
 
            // implement service  interface 
            typeBuilder.AddInterfaceImplementation(serviceType);

            // add methods
            serviceType.GetMethods().ToList().ForEach(method => {

                // get service and method names 
                var serviceName = method.DeclaringType.Name;
                var methodName = method.Name;
                var additionalCallParams = new string[] { serviceName, methodName };

                // get call method ;
                var methodToCall = typeof(GrpcInterfaceClientBase)
                    .GetMethod("CallUnaryMethodAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(new[] { method.GetParameters()[0].ParameterType, method.ReturnType.GetGenericArguments()[0] });

                typeBuilder.AddMethod(method, methodToCall, additionalCallParams);
            });

            // create type adn return 
            return typeBuilder.CreateTypeInfo();
        }

    }
}
