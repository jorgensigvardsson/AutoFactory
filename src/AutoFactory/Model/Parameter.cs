using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory.Model
{
    public class Parameter
    {
        public Parameter(bool isPerInstance, ParameterSyntax parameterSyntax)
        {
            IsPerInstance = isPerInstance;
            ParameterSyntax = parameterSyntax;
        }

        public ParameterSyntax ParameterSyntax { get; }
        public bool IsPerInstance { get; }
    }
}