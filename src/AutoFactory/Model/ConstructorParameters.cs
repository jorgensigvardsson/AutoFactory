using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory.Model
{
    public class ConstructorParameters
    {
        private readonly SemanticModel m_semanticModel;

        public ConstructorParameters(SemanticModel semanticModel, ParameterSyntax[] allParameters)
        {
            m_semanticModel = semanticModel;
            AllParameters = allParameters.Select(p => new Parameter(IsPerInstance((ParameterSyntax) p), p))
                                         .ToArray();
        }

        public Parameter[] AllParameters { get; }

        public IEnumerable<ParameterSyntax> PerInstanceParameters => AllParameters.Where(p => p.IsPerInstance)
                                                                                  .Select(p => NewParameterWithoutPerInstanceAttribute(p.ParameterSyntax));

        public IEnumerable<ParameterSyntax> FactoryCachedParameters => AllParameters.Where(p => !p.IsPerInstance)
                                                                                    .Select(p => NewParameterWithoutPerInstanceAttribute(p.ParameterSyntax));

        private bool IsPerInstance(ParameterSyntax p)
        {
            return p.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Any(IsPerInstance);
        }

        private bool IsPerInstance(AttributeSyntax a)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(m_semanticModel, a.Name);
            return symbolInfo.Symbol.ContainingType.ContainingNamespace.Name == "AutoFactory" &&
                   symbolInfo.Symbol.ContainingType.Name == "PerInstanceAttribute";
        }

        private ParameterSyntax NewParameterWithoutPerInstanceAttribute(ParameterSyntax p)
        {
            var list = new SyntaxList<AttributeListSyntax>();
            foreach (var al in p.AttributeLists)
            {
                var alNew = SyntaxFactory.AttributeList();

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var a in al.Attributes.Where(a => !IsPerInstance(a)))
                {
                    alNew = alNew.AddAttributes(a);
                }

                if (alNew.Attributes.Any())
                    list = list.Add(alNew);
            }

            return p.WithAttributeLists(list);
        }
    }
}