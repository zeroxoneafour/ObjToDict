# ObjToDict

Quickly serialize objects with source generation.

This package only provides type declarations. For source generation,
install [ObjToDict.Generator](https://nuget.org/packages/ObjToDict.Generator).

## usage

```
using ObjToDict;

namespace Test;

// make sure to mark class "partial"
[ObjToDict]
internal partial class TestClass
{
    // serialize properties
    public int A { get; set; } = 1;

    // and fields
    public double B = Math.PI;
    
    // field ignored
    [ObjToDictIgnore]
    public char C = 'a';
    
    // private field included
    [ObjToDictInclude]
    private string _d = ""Hello World"";
    
    // private fields not included by default
    private bool _e = false;
}
```

This code results in

```
//HintName: ObjToDict.Test.TestClass.g.cs
using System.Collections.Generic;
using ObjToDict;
namespace Test;
internal partial class TestClass : IObjToDict
{
    public IDictionary<string, dynamic> AsDictionary
    {
        get => ObjToDict();
        set => ObjFromDict(value);
    }

    public IDictionary<string, dynamic> ObjToDict()
    {
        var ret = new Dictionary<string, dynamic>();
        ret["A"] = A;
        ret["B"] = B;
        ret["_d"] = _d;
        return ret;
    }
    public void ObjFromDict(IDictionary<string, dynamic> dict)
    {
        A = (int)dict["A"];
        B = (double)dict["B"];
        _d = (string)dict["_d"];
    }
}
```
