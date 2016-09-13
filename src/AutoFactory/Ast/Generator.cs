using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
// ReSharper disable LoopCanBeConvertedToQuery

namespace AutoFactory.Ast
{
    public static class Generator
    {
        public static IEnumerable<MemberDeclarationSyntax> Generate(Model.Factory factory)
        {
            yield return GenerateFactoryInterface(factory);
            yield return GenerateFactoryImplementation(factory);
        }

        private static InterfaceDeclarationSyntax GenerateFactoryInterface(Model.Factory factory)
        {
            var factoryInterface = SyntaxFactory.InterfaceDeclaration(factory.Interface.Name.Identifier)
                                                .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var factoryMethod in factory.Interface.FactoryMethodDeclarations)
            {
                var createMethodDeclaration = SyntaxFactory.MethodDeclaration(factory.ConstructedClass.InterfaceName, "Create")
                                                           .WithParameterList(SyntaxFactory.ParameterList().AddParameters(factoryMethod.Constructor.ConstructorParameters.PerInstanceParameters.ToArray()))
                                                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                factoryInterface = factoryInterface.WithMembers(factoryInterface.Members.Add(createMethodDeclaration));
            }

            return factoryInterface;
        }

        private static ClassDeclarationSyntax GenerateFactoryImplementation(Model.Factory factory)
        {
            var factoryCachedParameters = factory.ConstructedClass
                                                 .PublicConstructors
                                                 .SelectMany(c => c.ConstructorParameters.FactoryCachedParameters)
                                                 .DistinctBy(p => p.Identifier.Text)
                                                 .ToArray();

            var fieldDeclarations = new List<FieldDeclarationSyntax>();
            var fieldInitializations = new SyntaxList<StatementSyntax>();
            foreach (var factoryCachedParameter in factoryCachedParameters)
            {
                var field = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(factoryCachedParameter.Type, new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(SyntaxFactory.VariableDeclarator("m_" + factoryCachedParameter.Identifier))))
                                         .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                                                       .Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
                fieldDeclarations.Add(field);

                var fieldInit = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                   SyntaxFactory.IdentifierName("m_" + factoryCachedParameter.Identifier.Text),
                                                                   SyntaxFactory.IdentifierName(factoryCachedParameter.Identifier));

                fieldInitializations = fieldInitializations.Add(SyntaxFactory.ExpressionStatement(fieldInit));
            }

            var factoryConstructor = SyntaxFactory.ConstructorDeclaration(factory.Interface.ImplementationClassName.Identifier)
                                                  .WithModifiers(new SyntaxTokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                                  .WithParameterList(SyntaxFactory.ParameterList().AddParameters(factoryCachedParameters))
                                                  .WithBody(SyntaxFactory.Block().WithStatements(new SyntaxList<StatementSyntax>().AddRange(fieldInitializations)));

            var factoryClass = SyntaxFactory.ClassDeclaration(factory.Interface.ImplementationClassName.Identifier)
                                            .WithModifiers(new SyntaxTokenList().Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                            .WithMembers(new SyntaxList<MemberDeclarationSyntax>().AddRange(fieldDeclarations)
                                                                                                  .Add(factoryConstructor))
                                            .WithBaseList(SyntaxFactory.BaseList().AddTypes(SyntaxFactory.SimpleBaseType(factory.Interface.Name)));


            foreach (var factoryMethod in factory.Interface.FactoryMethodDeclarations)
            {
                var constructorCallArgumentList = SyntaxFactory.ArgumentList();

                foreach (var param in factoryMethod.Constructor.ConstructorParameters.AllParameters)
                {
                    var paramOrField = param.IsPerInstance
                                           // Value comes from parameter to Create()
                                           ? SyntaxFactory.IdentifierName(param.ParameterSyntax.Identifier)
                                           // Value comes from cached factory field
                                           : SyntaxFactory.IdentifierName("m_" + param.ParameterSyntax.Identifier.Text);

                    constructorCallArgumentList = constructorCallArgumentList.AddArguments(SyntaxFactory.Argument(paramOrField));
                }

                var constructorCall = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(factory.ConstructedClass.Name.Identifier))
                                                   .WithArgumentList(constructorCallArgumentList);
                var returnStatement = SyntaxFactory.ReturnStatement(constructorCall);

                var createMethod = SyntaxFactory.MethodDeclaration(factory.ConstructedClass.InterfaceName, "Create")
                                                .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                                .WithParameterList(SyntaxFactory.ParameterList().AddParameters(factoryMethod.Constructor.ConstructorParameters.PerInstanceParameters.ToArray()))
                                                .WithBody(SyntaxFactory.Block()
                                                .WithStatements(new SyntaxList<StatementSyntax>().Add(returnStatement)));

                factoryClass = factoryClass.WithMembers(factoryClass.Members.Add(createMethod));
            }

            return factoryClass;
        }
    }
}
