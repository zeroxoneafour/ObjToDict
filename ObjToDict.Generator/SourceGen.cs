using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ObjToDict.Generator;

internal readonly record struct FieldInfo(string Name, string Type)
{
    public readonly string Name = Name;
    public readonly string Type = Type;
}

internal readonly record struct ObjToDictClass(string Name, string Type, string Accessibility, string Namespace, FieldInfo[] Fields)
{
    public readonly string Name = Name;
    public readonly string Type = Type;
    public readonly string Accessibility = Accessibility;
    public readonly string Namespace = Namespace;
    public readonly FieldInfo[] Fields = Fields;
}

[Generator(LanguageNames.CSharp)]
public class SourceGen : IIncrementalGenerator
{
    private static string GetAccessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => "internal"
        };
    }

    private static string GetDeclaredType(TypeDeclarationSyntax syntax)
    {
        return syntax switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax r => $"record {r.ClassOrStructKeyword.Text}",
            _ => ""
        };
    }

    private static string ObjToDictBeginning(ObjToDictClass c) => @$"using System.Collections.Generic;
using ObjToDict;
{(c.Namespace != null ? "namespace " + c.Namespace + ";" : "")}
{c.Accessibility} partial {c.Type} {c.Name} : IObjToDict
{{
    public IDictionary<string, dynamic> AsDictionary
    {{
        get => ObjToDict();
        set => ObjFromDict(value);
    }}
";

    private static string ObjToDictMethod(ObjToDictClass c)
    {
        var sb = new StringBuilder();
        sb.Append(@"
    public IDictionary<string, dynamic> ObjToDict()
    {
        var ret = new Dictionary<string, dynamic>();");
        foreach (var field in c.Fields)
        {
            sb.Append(@$"
        ret[""{field.Name}""] = {field.Name};");
        }
        sb.Append(@"
        return ret;
    }");
        return sb.ToString();
    }
    
    private static string ObjFromDictMethod(ObjToDictClass c)
    {
        var sb = new StringBuilder();
        sb.Append(@"
    public IDictionary<string, dynamic> ObjFromDict(IDictionary<string, dynamic> dict)
    {");
        foreach (var field in c.Fields)
        {
            sb.Append(@$"
        {field.Name} = ({field.Type})dict[""{field.Name}""];");
        }
        sb.Append(@"
    }");
        return sb.ToString();
    }
    
    private static ObjToDictClass? CreateObjToDict (GeneratorAttributeSyntaxContext context)
    {
        var node = (TypeDeclarationSyntax)context.TargetNode;
        // class must be partial
        if (!node.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return null;
        }
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(node);
        if (classSymbol is null)
        {
            return null;
        }
        var fieldInfo = new List<FieldInfo>();
        var name = classSymbol.Name;
        var accessibility = GetAccessibility(classSymbol.DeclaredAccessibility);
        var type = GetDeclaredType(node);
        var ns = classSymbol.ContainingNamespace.IsGlobalNamespace ? "" : classSymbol.ContainingNamespace.ToDisplayString();
        foreach (var member in classSymbol.GetMembers())
        {
            if (member.GetAttributes().Any(static a => a.AttributeClass.Name == "ObjToDictIgnoreAttribute")
                || (member.DeclaredAccessibility != Accessibility.Public
                    && !member.GetAttributes().Any(static a => a.AttributeClass.Name == "ObjToDictIncludeAttribute")))
                continue;
            if (member is IFieldSymbol fieldSymbol)
            {
                fieldInfo.Add(new FieldInfo(fieldSymbol.Name, fieldSymbol.Type.ToDisplayString()));
            }
            if (member is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.IsReadOnly || propertySymbol.IsWriteOnly) continue;
                fieldInfo.Add(new FieldInfo(propertySymbol.Name, propertySymbol.Type.ToDisplayString()));
            }
        }

        return new ObjToDictClass(name, type, accessibility, ns,fieldInfo.ToArray());
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ObjToDictClass?> objs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "ObjToDict.ObjToDictAttribute",
                predicate: static (_, _) => true,
                transform: static (ctx, _) => CreateObjToDict(ctx))
            .Where(static x => x is not null);
        
        context.RegisterSourceOutput(objs,
            static (spc, source) => Execute(source, spc));
    }

    private static void Execute(ObjToDictClass? nullableSource, SourceProductionContext spc)
    {
        if (nullableSource is null) return;
        var source = nullableSource.Value;
        var sb = new StringBuilder();
        sb.Append(ObjToDictBeginning(source));
        sb.Append(ObjToDictMethod(source));
        sb.Append(ObjFromDictMethod(source));
        sb.Append("\n}");
        spc.AddSource($"ObjToDict.{source.Namespace}.{source.Name}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}