using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory.Model
{
    public class Builder
    {
        public Factory Build(ClassDeclarationSyntax targetClass, SemanticModel semanticModel)
        {
            var constructedClass = BuildConstructedClass(semanticModel, targetClass);
            var @interface = BuildInterface(constructedClass);

            return new Factory(constructedClass, @interface);
        }

        private FactoryInterface BuildInterface(ConstructedClass constructedClass)
        {
            return new FactoryInterface(constructedClass, constructedClass.PublicConstructors.Select(BuildFactoryMethodDeclaration));
        }

        private FactoryMethodDeclaration BuildFactoryMethodDeclaration(Constructor constructor)
        {
            return new FactoryMethodDeclaration(constructor);
        }

        private ConstructedClass BuildConstructedClass(SemanticModel semanticModel, ClassDeclarationSyntax targetClass)
        {
            return new ConstructedClass(SyntaxFactory.IdentifierName(targetClass.Identifier), BuildConstructors(semanticModel, targetClass));
        }

        private IEnumerable<Constructor> BuildConstructors(SemanticModel semanticModel, ClassDeclarationSyntax targetClass)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var constructor in PublicConstructorsOf(targetClass))
            {
                yield return new Constructor(new ConstructorParameters(semanticModel, constructor.ParameterList.Parameters.ToArray()));
            }
        }

        private IEnumerable<ConstructorDeclarationSyntax> ConstructorsOf(ClassDeclarationSyntax targetClass)
        {
            var constructors = targetClass.Members
                                          .OfType<ConstructorDeclarationSyntax>()
                                          .ToArray();

            if (!constructors.Any())
            {
                // If no constructor is defined, the compiler will define a public default constructor
                constructors = new[]
                {
                    SyntaxFactory.ConstructorDeclaration(targetClass.Identifier)
                                 .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                };
            }

            return constructors;
        }

        private IEnumerable<ConstructorDeclarationSyntax> PublicConstructorsOf(ClassDeclarationSyntax classSyntax)
        {
            return ConstructorsOf(classSyntax).Where(IsPublic);
        }

        private bool IsPublic(ConstructorDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }
    }
}
