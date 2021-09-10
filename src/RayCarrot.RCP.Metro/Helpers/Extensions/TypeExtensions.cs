using System;
using System.Reflection;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Extension methods for a <see cref="Type"/>
    /// </summary>
    public static class TypeExtensions
    {
        #pragma warning disable CS1574 // // XML comment has cref attribute that could not be resolved

        /// <summary>
        /// Creates a new instance of the specified <see cref="Type"/>
        /// </summary>
        /// <param name="type">The type to create a new instance of</param>
        /// <returns>The new instance</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">type is not a RuntimeType. -or- type is an open generic type (that is, the <see cref="Type.ContainsGenericParameters"/> property returns true)</exception>
        /// <exception cref="NotSupportedException">type cannot be a <see cref="System.Reflection.Emit.TypeBuilder"/>. -or- Creation of <see cref="TypedReference"/>,
        /// <see cref="System.ArgIterator"/>, <see cref="Void"/>, and <see cref="RuntimeArgumentHandle"/> types, or arrays
        /// of those types, is not supported. -or- The assembly that contains type is a dynamic assembly that was created with <see cref="System.Reflection.Emit.AssemblyBuilderAccess.Save"/>.</exception>
        /// <exception cref="TargetInvocationException">The constructor being called throws an exception</exception>
        /// <exception cref="MethodAccessException">In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        /// or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        /// catch the base class exception, <see cref="MemberAccessException"/>, instead. The caller
        /// does not have permission to call this constructor.</exception>
        /// <exception cref="MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism.</exception>
        /// <exception cref="System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through Overload:System.Type.GetTypeFromProgID or Overload:System.Type.GetTypeFromCLSID</exception>
        /// <exception cref="MissingMethodException">In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        /// or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        /// catch the base class exception, System.MissingMemberException, instead. No matching public constructor was found.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">type is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered</exception>
        /// <exception cref="TypeLoadException">type is not a valid type</exception>
        public static object CreateInstance(this Type type) => Activator.CreateInstance(type);

        /// <summary>
        /// Creates a new instance of the specified <see cref="Type"/>
        /// </summary>
        /// <param name="type">The type to create a new instance of</param>
        /// <param name="args">Arguments to pass in</param>
        /// <returns>The new instance</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">type is not a RuntimeType. -or- type is an open generic type (that is, the <see cref="Type.ContainsGenericParameters"/> property returns true)</exception>
        /// <exception cref="NotSupportedException">type cannot be a <see cref="System.Reflection.Emit.TypeBuilder"/>. -or- Creation of <see cref="TypedReference"/>,
        /// <see cref="System.ArgIterator"/>, <see cref="Void"/>, and <see cref="RuntimeArgumentHandle"/> types, or arrays
        /// of those types, is not supported. -or- The assembly that contains type is a dynamic assembly that was created with <see cref="System.Reflection.Emit.AssemblyBuilderAccess.Save"/>.
        /// -or- The constructor that best matches args has varargs arguments.</exception>
        /// <exception cref="TargetInvocationException">The constructor being called throws an exception</exception>
        /// <exception cref="MethodAccessException">In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        /// or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        /// catch the base class exception, <see cref="MemberAccessException"/>, instead. The caller
        /// does not have permission to call this constructor.</exception>
        /// <exception cref="MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism.</exception>
        /// <exception cref="System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through Overload:System.Type.GetTypeFromProgID or Overload:System.Type.GetTypeFromCLSID</exception>
        /// <exception cref="MissingMethodException">In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        /// or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        /// catch the base class exception, System.MissingMemberException, instead. No matching public constructor was found.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">type is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered</exception>
        /// <exception cref="TypeLoadException">type is not a valid type</exception>
        public static object CreateInstance(this Type type, params object[] args) => Activator.CreateInstance(type, args);

        /// <summary>
        /// Creates a new instance of the specified <see cref="Type"/>
        /// </summary>
        /// <param name="type">The type to create a new instance of</param>
        /// <param name="args">Arguments to pass in</param>
        /// <param name="activationAttributes">Activation attributes to pass in</param>
        /// <returns>The new instance</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">type is not a RuntimeType. -or- type is an open generic type (that is, the <see cref="Type.ContainsGenericParameters"/> property returns true)</exception>
        /// <exception cref="NotSupportedException">type cannot be a <see cref="System.Reflection.Emit.TypeBuilder"/>. -or- Creation of <see cref="TypedReference"/>,
        /// <see cref="System.ArgIterator"/>, <see cref="Void"/>, and <see cref="RuntimeArgumentHandle"/> types, or arrays
        /// of those types, is not supported. -or- activationAttributes is not an empty array, and the type being created does not derive from <see cref="MarshalByRefObject"/>. -or-
        /// The assembly that contains type is a dynamic assembly that was created with <see cref="System.Reflection.Emit.AssemblyBuilderAccess.Save"/>.
        /// -or- The constructor that best matches args has varargs arguments.</exception>
        /// <exception cref="TargetInvocationException">The constructor being called throws an exception</exception>
        /// <exception cref="MethodAccessException">The caller does not have permission to call this constructor</exception>
        /// <exception cref="MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism.</exception>
        /// <exception cref="System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through Overload:System.Type.GetTypeFromProgID or Overload:System.Type.GetTypeFromCLSID</exception>
        /// <exception cref="MissingMethodException">No matching public constructor was found</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">type is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered</exception>
        /// <exception cref="TypeLoadException">type is not a valid type</exception>
        public static object CreateInstance(this Type type, object[] args, object[] activationAttributes) => Activator.CreateInstance(type, args, activationAttributes);

        /// <summary>
        /// Creates a new instance of the specified <see cref="Type"/> and casts it to the expected type
        /// </summary>
        /// <typeparam name="T">The type to cast the new instance to</typeparam>
        /// <param name="type">The type to create a new instance of</param>
        /// <returns>The new instance</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">type is not a RuntimeType. -or- type is an open generic type (that is, the <see cref="Type.ContainsGenericParameters"/> property returns true)</exception>
        /// <exception cref="NotSupportedException">type cannot be a <see cref="System.Reflection.Emit.TypeBuilder"/>. -or- Creation of <see cref="TypedReference"/>,
        /// <see cref="System.ArgIterator"/>, <see cref="Void"/>, and <see cref="RuntimeArgumentHandle"/> types, or arrays
        /// of those types, is not supported. -or- The assembly that contains type is a dynamic assembly that was created with <see cref="System.Reflection.Emit.AssemblyBuilderAccess.Save"/>.</exception>
        /// <exception cref="TargetInvocationException">The constructor being called throws an exception</exception>
        /// <exception cref="MethodAccessException">In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        /// or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        /// catch the base class exception, <see cref="MemberAccessException"/>, instead. The caller
        /// does not have permission to call this constructor.</exception>
        /// <exception cref="MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism.</exception>
        /// <exception cref="System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through Overload:System.Type.GetTypeFromProgID or Overload:System.Type.GetTypeFromCLSID</exception>
        /// <exception cref="MissingMethodException">In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        /// or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        /// catch the base class exception, System.MissingMemberException, instead. No matching public constructor was found.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">type is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered</exception>
        /// <exception cref="TypeLoadException">type is not a valid type</exception>
        /// <exception cref="InvalidCastException">The type instance can not be converted to the specified type</exception>
        public static T CreateInstance<T>(this Type type) => (T)Activator.CreateInstance(type);

        #pragma warning restore CS1574
    }
}