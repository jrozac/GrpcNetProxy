using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GrpcNetProxy.Generics
{

    /// <summary>
    /// Type builder extensions
    /// </summary>
    public static class TypeBuilderExtensions
    {

        /// <summary>
        /// Create new builder for nested type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="constructorsBindings"></param>
        /// <returns></returns>
        public static TypeBuilder NewBuilderForNestedType(this Type type, string name, BindingFlags? constructorsBindings = null)
        {
            // create 
            var assemblyName = Guid.NewGuid().ToString();
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
            var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, type);

            // copy constructors
            var constructors = constructorsBindings.HasValue ? type.GetConstructors(constructorsBindings.Value) : type.GetConstructors();
            constructors.ToList().ForEach(c => typeBuilder.AddConstructor(c));

            // return
            return typeBuilder;

        }

        /// <summary>
        /// Add constructor to typebuidler
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public static TypeBuilder AddConstructor(this TypeBuilder typeBuilder, ConstructorInfo constructor)
        {

            // get constructor parameters 
            var paramsTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();

            // setup constructor
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramsTypes);
            var il = constructorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); //load this
            Enumerable.Range(1, paramsTypes.Length).ToList().ForEach(i => il.Emit(OpCodes.Ldarg_S, i));
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);

            // return builder
            return typeBuilder;
        }

        /// <summary>
        /// Add property to type builder
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="createGetter"></param>
        /// <param name="createSetter"></param>
        /// <returns></returns>
        public static TypeBuilder AddProperty(this TypeBuilder typeBuilder, Type type, string name, bool createGetter = true, bool createSetter = true)
        {
            // define property
            var fieldBuilder = typeBuilder.DefineField("_" + name, type, FieldAttributes.Private);
            var propertyBuilder = typeBuilder
                .DefineProperty(name, PropertyAttributes.None, CallingConventions.HasThis, type, null);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            // getter
            if(createGetter)
            {
                var getter = typeBuilder.DefineMethod("get_" + name, getSetAttr, type, Type.EmptyTypes);
                var getIL = getter.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getter);
            }

            // setter 
            if(createSetter)
            {
                var setter = typeBuilder.DefineMethod("set_" + name, getSetAttr, null, new[] { type });
                var setIL = setter.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setter);
            }

            // return
            return typeBuilder;
        }

        /// <summary>
        /// Implement interface properteis
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="interfaceType"></param>
        public static void ImplementInterfaceProperties(this TypeBuilder typeBuilder, Type interfaceType)
        {
            // checkc if interface type
            if(!interfaceType.IsInterface)
            {
                throw new NotSupportedException();
            }

            // add required properties
            interfaceType.GetProperties().ToList().ForEach(p => {
                typeBuilder.AddProperty(p.PropertyType, p.Name);
            });

        }

        /// <summary>
        /// Add method to typebuilder
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="method"></param>
        /// <param name="methodToCall"></param>
        /// <param name="additionalCallParams"></param>
        public static void AddMethod(this TypeBuilder typeBuilder, MethodInfo method, MethodInfo methodToCall, string[] additionalCallParams)
        {

            // set method properties
            var args = method.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                (from arg in args select arg.ParameterType).ToArray()
            );
            var il = methodBuilder.GetILGenerator();

            // set this 
            il.Emit(OpCodes.Ldarg_0);

            // set regular arguments
            Enumerable.Range(1, args.Length).ToList().ForEach(i => il.Emit(OpCodes.Ldarg_S, i));

            // set additional params
            additionalCallParams.ToList().ForEach(value => il.Emit(OpCodes.Ldstr, value));

            // set call method 
            il.Emit(OpCodes.Call, methodToCall);

            // set return 
            il.Emit(OpCodes.Ret);

            // set method to builder
            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

    }
}
