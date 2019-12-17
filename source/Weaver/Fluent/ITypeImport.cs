﻿using Mono.Cecil;
using System;
using System.Linq.Expressions;

namespace Weaver.Fluent
{
    public interface ITypeImport<T>
    {
        /// <summary>
        /// Gets the type defintion for the type.
        /// </summary>
        ITypeImport<T> GetType(out TypeDefinition typeDefinition);

        /// <summary>
        /// Returns back the default constructor if defined.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        ITypeImport<T> GetConstructor(out MethodDefinition constructor);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        ITypeImport<T> GetMethod(string methodName, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given in a lamda expression this returns back the property 
        /// </summary>
        /// <param name="accessor">The accessor.</param>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        ITypeImport<T> GetMethod(Expression<Func<T, Action>> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Given a lamda expression this will return a static function for the type
        /// </summary>
        ITypeImport<T> GetStaticMethod(Expression<Action> expression, out MethodDefinition methodDefinition);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        IPropertyImport<T> GetProperty(string name);

        /// <summary>
        /// Returns back the method with the given name.
        /// </summary>
        /// <param name="constructor">The constructor.</param>
        IPropertyImport<T> GetProperty(Expression<Func<T, object>> expression);
    }
}