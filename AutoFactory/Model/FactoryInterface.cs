using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory.Model
{
    public class FactoryInterface
    {
        private readonly ConstructedClass m_constructedClass;

        public FactoryInterface(ConstructedClass constructedClass, IEnumerable<FactoryMethodDeclaration> factoryMethodDeclarations)
        {
            m_constructedClass = constructedClass;
            FactoryMethodDeclarations = factoryMethodDeclarations.ToArray();
        }

        public IdentifierNameSyntax Name => SyntaxFactory.IdentifierName("I" + ImplementationClassName.Identifier.Text);
        public IdentifierNameSyntax ImplementationClassName => SyntaxFactory.IdentifierName(m_constructedClass.Name.Identifier.Text + "Factory");
        public FactoryMethodDeclaration[] FactoryMethodDeclarations { get; }
    }
}
