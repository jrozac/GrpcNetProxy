using System;
using System.Linq;
using System.Reflection;

namespace GrpcNetProxy.Generics
{

    /// <summary>
    /// Application domain extensions
    /// </summary>
    public static class AppDomainExtensions
    {

        /// <summary>
        /// Get assemlby by name 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetAssemblyByName(this AppDomain domain, string assemblyName)
        {
            return domain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
        }

        /// <summary>
        /// Get or load assmbly
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetOrLoadAssembly(this AppDomain domain, string assemblyName)
        {
            var assembly = domain.GetAssemblyByName(assemblyName);
            if(assembly != null)
            {
                return assembly;
            }
            return domain.Load(assemblyName);
        }

        /// <summary>
        /// Resolve
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="assemblyQualifiedTypeName"></param>
        /// <returns></returns>
        public static Type ResolveType(this AppDomain domain, string assemblyQualifiedTypeName)
        {
            return Type.GetType(assemblyQualifiedTypeName, (name) => domain.GetOrLoadAssembly(name.FullName), null, true);
        } 

    }
}
