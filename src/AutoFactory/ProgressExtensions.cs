﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoFactory
{
    internal static class ProgressExtensions
    {
        internal static void LogError(this IProgress<Diagnostic> progress, MemberDeclarationSyntax syntax, string message, params object[] args)
        {
            progress.Report(Diagnostic.Create(new DiagnosticDescriptor("JSAF0001", "Error", message, "", DiagnosticSeverity.Error, true), syntax.GetLocation(), args));
        }
    }
}
