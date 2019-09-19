using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GrpcNetProxy.Generics
{

    /// <summary>
    /// Application domain extensions
    /// </summary>
    public static class ReflectionExtensions
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

        /// <summary>
        /// Get base classes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetInheritanceHierarchy
            (this Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                yield return current;
            }
        }

        /// <summary>
        /// Get declared methods for class
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static MethodInfo[] GetDeclaredMethods(this Type classType)
        {
            return classType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

    }
}
