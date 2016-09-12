using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
// ReSharper disable LoopCanBeConvertedToQuery

namespace AutoFactory
{
    public class AutoFactoryGenerator : ICodeGenerator
    {
        // ReSharper disable once UnusedParameter.Local
        public AutoFactoryGenerator(AttributeData attributeData)
        {
            // Must have this constructor...
        }

        public async Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(MemberDeclarationSyntax applyTo, Document document, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            try
            {
                CheckValidity(applyTo);

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                return new SyntaxList<MemberDeclarationSyntax>().AddRange(new Ast.Generator().Generate(new Model.Builder().Build((ClassDeclarationSyntax)applyTo, semanticModel)));
            }
            catch (Exception ex)
            {
                progress.LogError(applyTo, ex.Message);
                return new SyntaxList<MemberDeclarationSyntax>();
            }
        }

        private void CheckValidity(MemberDeclarationSyntax applyTo)
        {
            // 1. Is it a class?
            var targetClass = applyTo as ClassDeclarationSyntax;
            if (targetClass == null)
                throw new Exception("Not a class.");

            // 2. Is it public?
            if (!IsPublic(targetClass))
                throw new Exception("Not a public class.");

            // 3. Is it abstract?
            if (IsAbstract(targetClass))
                throw new Exception("Abstract class cannot be instantiated.");

            // 4. Does it inherit from an interface name I<class name>?
            var hasInterface = targetClass.BaseList != null && targetClass.BaseList.Types.Select(t => t.Type).OfType<IdentifierNameSyntax>().Any(t => t.Identifier.Text.EndsWith($"I{targetClass.Identifier.Text}"));

            if (!hasInterface)
                throw new Exception($"{targetClass.Identifier.Text} does not implement an interface called I{targetClass.Identifier.Text}");

            // 5. Is it publically constructible?
            var constructors = ConstructorsOf(targetClass);
            var publicConstructors = PublicConstructorsOf(targetClass);

            if (!constructors.Any())
            {
                // No constructor means that it is default constructible
            }
            else
            {
                // This means that there is at least one constructor. At least one of those must be public!
                if (!publicConstructors.Any())
                    throw new Exception("Class does not have any public constructors.");
            }

            // Well, it seems legit so far!
        }

        private bool IsPublic(ClassDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        private bool IsAbstract(ClassDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        }

        private bool IsPublic(ConstructorDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        private IEnumerable<ConstructorDeclarationSyntax> ConstructorsOf(ClassDeclarationSyntax classSyntax)
        {
            return classSyntax.Members
                              .OfType<ConstructorDeclarationSyntax>()
                              .ToArray();
        }

        private IEnumerable<ConstructorDeclarationSyntax> PublicConstructorsOf(ClassDeclarationSyntax classSyntax)
        {
            return ConstructorsOf(classSyntax).Where(IsPublic);
        }

    }
}