//HintName: ObjToDict.g.cs
using System;
using System.Collections.Generic;

namespace ObjToDict;

/// <summary>
/// Implement <c>IObjToDict</c> automatically on target class/struct
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ObjToDictAttribute : Attribute
{
    public ObjToDictAttribute() {}
}

/// <summary>
/// Include field/property in ObjToDict serialization
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ObjToDictIncludeAttribute : Attribute
{
    public ObjToDictIncludeAttribute() {}
}

/// <summary>
/// Exclude field/property from ObjToDict serialization
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ObjToDictIgnoreAttribute : Attribute
{
    public ObjToDictIgnoreAttribute() {}
}

/// <summary>
/// Interface showing an object can be serialized/deserialized with ObjToDict
/// </summary>
public interface IObjToDict
{
    public IDictionary<string, dynamic> AsDictionary { get; set; }
    public IDictionary<string, dynamic> ObjToDict();
    public void ObjFromDict(IDictionary<string, dynamic> dictionary);
}