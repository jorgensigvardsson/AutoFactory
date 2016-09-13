using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace AutoFactory
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute(typeof(AutoFactoryGenerator))]
    [Conditional("CodeGeneration")]
    public class AutoFactoryAttribute : Attribute
    {
    }
}
