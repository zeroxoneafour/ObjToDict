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
    public IDictionary<string, dynamic> ObjFromDict(IDictionary<string, dynamic> dict)
    {
        A = (int)dict["A"];
        B = (double)dict["B"];
        _d = (string)dict["_d"];
    }
}