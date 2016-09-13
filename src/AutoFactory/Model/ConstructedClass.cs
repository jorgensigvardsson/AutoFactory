using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory.Model
{
    public class ConstructedClass
    {
        public ConstructedClass(IdentifierNameSyntax name, IEnumerable<Constructor> publicConstructors)
        {
            Name = name;
            PublicConstructors = publicConstructors.ToArray();
        }

        public IdentifierNameSyntax Name { get; }
        public IdentifierNameSyntax InterfaceName => SyntaxFactory.IdentifierName("I" + Name.Identifier.Text);
        public Constructor[] PublicConstructors { get; }
    }
}