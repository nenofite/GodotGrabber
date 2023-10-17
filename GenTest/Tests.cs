using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeGrabber;
using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VerifyMSTest;
using VerifyTests;

namespace GenTest;

[TestClass]
public class Tests : VerifyBase
{
    [TestMethod]
    public Task Basics() =>
        GenerateAndVerify(
            @"
using System;
using NodeGrabber;

namespace Foobar.Biz
{
    internal partial class Blop
    {
        [Grab]
        int ShouldInclude;

        [Grab]
        bool ShouldWorkWithProperties {get; private set;}
        
        public string ShouldSkipNoAttribute;

        [Grab]
        System.Text.StringBuilder ShouldIncludeQualType;

        [Grab(""special path"")]
        bool withPath;
    }
}
"
        );

    Task GenerateAndVerify(string src)
    {
        var syntax = CSharpSyntaxTree.ParseText(src);
        var compilation = CSharpCompilation.Create(
            "Tests",
            syntaxTrees: new[] { syntax },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
        );
        var generator = new GrabberGen();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [ModuleInitializer]
    internal static void Initialize() => VerifySourceGenerators.Initialize();
}
