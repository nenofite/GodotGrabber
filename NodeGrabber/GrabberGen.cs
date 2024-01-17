﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NodeGrabber
{
    [Generator(LanguageNames.CSharp)]
    public class GrabberGen : IIncrementalGenerator
    {
        public const string AttributeCs =
            @"
// <auto-generated />
using System;

namespace NodeGrabber
{
    public sealed class GrabAttribute : Attribute
    {
        public readonly string Path;
        public GrabAttribute(string path = null)
        {
            Path = path;
        }
    }
}
";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(
                x => x.AddSource("GrabberGen.g.cs", AttributeCs)
            );

            var ls = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: HasClassWithAttribute,
                transform: TransformClass
            );
            context.RegisterSourceOutput(ls, EmitClasses);
        }

        static bool HasClassWithAttribute(SyntaxNode node, CancellationToken token)
        {
            return node is ClassDeclarationSyntax
                && node.DescendantNodes()
                    .Where(n => n is FieldDeclarationSyntax || n is PropertyDeclarationSyntax)
                    .SelectMany(n => n.DescendantNodes())
                    .Where(n => n is AttributeSyntax)
                    .Any();
        }

        static void EmitClasses(SourceProductionContext context, ClassAndFields source)
        {
            if (source == null)
                return;

            const string indent = "        ";

            var src =
                $@"
partial class {source.ClassDec.Name}
{{
    /// <summary>
    /// Set the values of all fields marked with <c>[Grab]</c>. You should usually call this in <c>_Ready()</c>.
    /// 
    /// The current value of each field will be overwritten.
    /// </summary>
    void GrabNodes()
    {{
{formatLines(source.Fields.Select(EmitField))}
    }}
}}
";

            var showNs = !source.ClassDec.ContainingNamespace.IsGlobalNamespace;
            if (showNs)
            {
                src =
                    $@"
namespace {source.ClassDec.ContainingNamespace.ToDisplayString()}
{{
{src}
}}
";
            }

            context.AddSource($"{source.ClassDec.ToDisplayString()}_Grabber.g.cs", src);

            string formatLines(IEnumerable<string> lines)
            {
                return string.Join("\n", lines.Select(ln => indent + ln));
            }
        }

        static string EmitField((ISymbol, string path) t)
        {
            var (field, path) = t;
            var localName = field.Name;
            ITypeSymbol type;
            if (field is IFieldSymbol f)
                type = f.Type;
            else if (field is IPropertySymbol p)
                type = p.Type;
            else
                return "// nop";

            return $"{localName} = GetNode<{type.ToDisplayString()}>(\"{path}\");";
        }

        static ClassAndFields TransformClass(
            GeneratorSyntaxContext context,
            CancellationToken token
        )
        {
            var classNode = context.Node as ClassDeclarationSyntax;
            if (classNode == null)
                throw new Exception($"Unexpected node {context.Node}");

            var classDec = context.SemanticModel.GetDeclaredSymbol(classNode);
            var fields = classNode
                .DescendantNodes()
                .Where(n => n is FieldDeclarationSyntax || n is PropertyDeclarationSyntax)
                .SelectMany(extractFields)
                .ToImmutableList();

            if (!fields.Any())
                return null;

            return new ClassAndFields(classDec, fields);

            IEnumerable<(ISymbol field, string path)> extractFields(SyntaxNode node)
            {
                var atts = node.DescendantNodes().OfType<AttributeSyntax>();
                string pathOverride = null;

                foreach (var att in atts)
                {
                    var attSym = context.SemanticModel.GetSymbolInfo(att).Symbol as IMethodSymbol;
                    if (attSym == null)
                        continue;
                    if (attSym.ContainingType?.ToDisplayString() != "NodeGrabber.GrabAttribute")
                        continue;

                    pathOverride = ExtractPathFromAttribute(att);
                    goto foundAtt;
                }
                yield break;

                foundAtt:
                foreach (var n in node.DescendantNodes())
                {
                    if (n is VariableDeclaratorSyntax f)
                    {
                        var sym = context.SemanticModel.GetDeclaredSymbol(f);
                        if (sym == null)
                            continue;

                        yield return (sym, pathOverride ?? "%" + sym.Name);
                    }
                }
            }
        }

        static string ExtractPathFromAttribute(AttributeSyntax att)
        {
            var argNodes = att.ArgumentList?.Arguments.FirstOrDefault()?.DescendantNodes();
            if (argNodes == null)
                return null;

            foreach (var n in argNodes)
            {
                if (
                    n is LiteralExpressionSyntax lit
                    && lit.Kind() == SyntaxKind.StringLiteralExpression
                )
                {
                    if (lit.Token.Value is string name)
                        return name;
                }
            }

            return null;
        }

        class ClassAndFields
        {
            public readonly ISymbol ClassDec;
            public readonly ImmutableList<(ISymbol, string path)> Fields;

            public ClassAndFields(ISymbol classDec, ImmutableList<(ISymbol, string path)> fields)
            {
                ClassDec = classDec;
                Fields = fields;
            }
        }
    }
}
