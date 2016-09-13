using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory.Model
{
    public static class Builder
    {
        public static Factory Build(ClassDeclarationSyntax targetClass, SemanticModel semanticModel)
        {
            var constructedClass = BuildConstructedClass(semanticModel, targetClass);
            var @interface = BuildInterface(constructedClass);

            return new Factory(constructedClass, @interface);
        }

        private static FactoryInterface BuildInterface(ConstructedClass constructedClass)
        {
            return new FactoryInterface(constructedClass, constructedClass.PublicConstructors.Select(BuildFactoryMethodDeclaration));
        }

        private static FactoryMethodDeclaration BuildFactoryMethodDeclaration(Constructor constructor)
        {
            return new FactoryMethodDeclaration(constructor);
        }

        private static ConstructedClass BuildConstructedClass(SemanticModel semanticModel, ClassDeclarationSyntax targetClass)
        {
            return new ConstructedClass(SyntaxFactory.IdentifierName(targetClass.Identifier), BuildConstructors(semanticModel, targetClass));
        }

        private static IEnumerable<Constructor> BuildConstructors(SemanticModel semanticModel, ClassDeclarationSyntax targetClass)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var constructor in PublicConstructorsOf(targetClass))
            {
                yield return new Constructor(new ConstructorParameters(semanticModel, constructor.ParameterList.Parameters.ToArray()));
            }
        }

        private static IEnumerable<ConstructorDeclarationSyntax> ConstructorsOf(ClassDeclarationSyntax targetClass)
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

        private static IEnumerable<ConstructorDeclarationSyntax> PublicConstructorsOf(ClassDeclarationSyntax classSyntax)
        {
            return ConstructorsOf(classSyntax).Where(IsPublic);
        }

        private static bool IsPublic(ConstructorDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }
    }
}
