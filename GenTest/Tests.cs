using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeGrabber;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VerifyMSTest;
using VerifyTests;

namespace GenTest;

[TestClass]
public class Tests : VerifyBase
{
    [TestMethod]
    public Task TestMethod1()
    {
        var src =
            @"
namespace Foobar.Biz
{
    internal partial class Bloop
    {
        int boop;
    }
}
";

        var syntax = CSharpSyntaxTree.ParseText(src);
        var compilation = CSharpCompilation.Create("Tests", syntaxTrees: new[] { syntax });
        var generator = new GrabberGen();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [ModuleInitializer]
    internal static void Initialize() => VerifySourceGenerators.Initialize();
}
