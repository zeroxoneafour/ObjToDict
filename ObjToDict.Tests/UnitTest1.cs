using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyNUnit;
using NUnit;

namespace ObjToDict.Tests;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        // Create references for assemblies we require
        // We could add multiple references if required
        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references); // 👈 pass the references to the compilation


        // Create an instance of our EnumGenerator incremental source generator
        var generator = new SourceGen();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver).UseDirectory("Snapshots");
    }
}

public class Tests
{
    [Test]
    public Task Test1()
    {
        var source = @"
namespace Test;

[ObjToDict.ObjToDict]
internal partial class TestClass
{
    public int A { get; set; } = 1;
    public double B = Math.PI;
    [ObjToDict.ObjToDictIgnore]
    public char C = 'a';
    [ObjToDict.ObjToDictInclude]
    private string _d = ""Hello World"";
    private bool _e = false;
}
";
        return TestHelper.Verify(source);
    }
}

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}