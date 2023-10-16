﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
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
                transform: TransformClasses
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
            var src =
                $@"
namespace {source.ClassDec.ContainingNamespace.ToDisplayString()}
{{
    partial class {source.ClassDec.Name}
    {{
        void GrabNodes()
        {{
{string.Join("\n", source.Fields.Select(EmitField))}
        }}
    }}
}}
";
            context.AddSource($"{source.ClassDec.ToDisplayString()}_Grabber.g.cs", src);
        }

        static string EmitField(ISymbol field)
        {
            var localName = field.Name;
            ITypeSymbol type;
            if (field is IFieldSymbol f)
                type = f.Type;
            else if (field is IPropertySymbol p)
                type = p.Type;
            else
                return "// nop";

            return $"// {localName} = GetNode<{type.ToDisplayString()}>(\"%{localName}\");";
        }

        static ClassAndFields TransformClasses(
            GeneratorSyntaxContext context,
            CancellationToken token
        )
        {
            var classNode = context.Node as ClassDeclarationSyntax;
            if (classNode == null)
                throw new Exception("huh?");

            var classDec = context.SemanticModel.GetDeclaredSymbol(classNode);
            var fields = classNode
                .DescendantNodes()
                .Where(n => n is FieldDeclarationSyntax || n is PropertyDeclarationSyntax)
                .Where(hasAttribute)
                .SelectMany(n => n.DescendantNodes())
                .OfType<VariableDeclaratorSyntax>()
                .Select(f => context.SemanticModel.GetDeclaredSymbol(f))
                .Where(f => f != null)
                .ToImmutableList();

            return new ClassAndFields(classDec, fields);

            bool hasAttribute(SyntaxNode node)
            {
                var atts = node.DescendantNodes().OfType<AttributeSyntax>();
                if (atts is null)
                    return false;
                foreach (var att in atts)
                {
                    var attSym = context.SemanticModel.GetSymbolInfo(att).Symbol as IMethodSymbol;
                    if (attSym == null)
                        continue;
                    if (attSym.ContainingType?.ToDisplayString() == "NodeGrabber.GrabAttribute")
                        return true;
                }
                return false;
            }
        }

        class ClassAndFields
        {
            public readonly ISymbol ClassDec;
            public readonly ImmutableList<ISymbol> Fields;

            public ClassAndFields(ISymbol classDec, ImmutableList<ISymbol> fields)
            {
                ClassDec = classDec;
                Fields = fields;
            }
        }
    }
}
