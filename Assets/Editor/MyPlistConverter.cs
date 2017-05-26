using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using ProtoBuf;
using System.Reflection;

using UnityEditor;
using PlistCS;
using System.IO;
using System.Linq;

public class MyDataConvertEditor : Editor {
    
    [MenuItem("Tools/Define DataObjects")]
    public static void MyDefineDataObjects(){

        //取其中一个class来构造类
        var dataFiles = Directory.GetFiles("data/");
        var _namespace = new NamespaceMaker("Data");
        foreach(var name in dataFiles){
            if(name.Contains("DS_Store")) continue;

            var data = (Dictionary<string, object>)Plist.readPlist(name);
            var dataList = data.ToList();
            var data1 = dataList[0];

            var className = name.Replace("data/", "").Replace(".plist", "");
            var _class = new ClassMaker(className, data1.Value);
            _namespace.addClass(_class);
        }

        File.WriteAllText("Assets/DataObjects.cs", _namespace.code);
    }

    [MenuItem("Tools/ToProtobufOne")]
    public static void MyProtobufOne(){
        //plist data
        var dataFiles = Directory.GetFiles("data/");

        foreach(var name in dataFiles){
            if(name.Contains("DS_Store")) continue;

            var data = (Dictionary<string, object>)Plist.readPlist(name);
            var className = name.Replace("data/", "").Replace(".plist", "");
            Debug.Log(name + " " + className);

            Type t = Type.GetType("Data." + className + ", Assembly-CSharp"); 
            MethodInfo mf = t.GetMethod("getItems");

            var classDict = new Dictionary<string, object>();
            foreach(var d in data){
                var _class = new ClassMaker(className, d.Value);
                classDict.Add(d.Key, _class.dictForJson);
            }

            var json = JsonConvert.SerializeObject(classDict);

            using (var file = File.Create(className + ".bin")) {
                Serializer.Serialize(file, mf.Invoke(null, new object[]{json}));
                Debug.Log("save success");
            }
        }
    }


    [MenuItem("Tools/ProtobufTest")]
    public static void MyProtobufTest(){
        // var data = Data.test2.GetData("1");
        // Debug.Log("!!!" + data.dictstr.Count);
        // Debug.Log("~~~" + data.name);
    }
}

public class CodeMaker{
    public virtual string name {get;set;}
    public virtual string type {get;set;}
    public virtual string code {get{return "";}}
}

public class NamespaceMaker : CodeMaker{
    public override string code{
        get{
            var str = "using System.Collections.Generic;\nusing ProtoBuf;\nusing Newtonsoft.Json;\n";
            str += "namespace {0} {{\n{1}\n}}";
            var classesStr = "";
            foreach(var c in classes){
                classesStr += "\n" + c.code;
            }
            return string.Format(str, name, classesStr);
        }
    }

    public NamespaceMaker(string name){
        this.name = name;
    }

    public List<ClassMaker> classes = new List<ClassMaker>();
    public void addClass(ClassMaker Class){
        if(Class == null){
            Debug.LogError("addClass.Class null");
        }
        classes.Add(Class);
    }
}

public class ClassMaker : CodeMaker{
    public override string code{
        get{
            var str = "\t[ProtoContract]\n" + "\tpublic class {0} {{\n {1} \n{2} \n\t}}\n";
            var memberStr = "";
            for(int i = 0; i < members.Count; ++i){
                memberStr += "\t\t" + "[ProtoMember(" + (i+1) +")]" + "\n";
                memberStr += "\t\t" + members[i].code + "\n";
            }

            return string.Format(str, name, memberStr, additionalCode);
        }
    }

    //string.format 中的花括号{}, 要用两个来转义 {{ }}}
    //@字符串中的引号", 要用两个来转义 ""
    public string additionalCode{
        get{
            var str = 
@"
		private static Dictionary<string, {0}> items;

        public static {0} GetData(string id){{
			var path = ""{0}.bin"";
			if(items == null){{
				if(System.IO.File.Exists(path)){{
					using (var file = System.IO.File.OpenRead(path)){{
						items = Serializer.Deserialize<Dictionary<string, {0}>>(file);
					}}
				}}
				else{{
					//can not find this file
				}}
			}}
			
			return items.ContainsKey(id) ? items[id] : null;
		}}

		public static Dictionary<string, {0}> getItems(string json){{
			items = JsonConvert.DeserializeObject<Dictionary<string, {0}>>(json);
			return items;
		}}

"
;

            return string.Format(str, name);
        }
    }

    public Dictionary<string, object> dictForJson{
        get{
            var dict = new Dictionary<string, object>();
            foreach(var m in members){
                dict.Add(m.name, m.value);
            }
            return dict;
        }
    }

    public ClassMaker(string name, object obj){
        this.name = name;
        foreach(var m in (Dictionary<string, object>)obj){
            var member = new MemberMaker(m.Key, m.Value);
            members.Add(member);
        }
    }

    private List<MemberMaker> members = new List<MemberMaker>();
 }

public class MemberMaker : CodeMaker{
    public override string code{
        get{
            var c = "public " + type + " " + name;
            if(type.Contains("List<") || type.Contains("Dictionary<")){
                c += " = new " + type + "()";
            }
            return c + ";";
        }
    }

    public object value{
        get;
        set;
    }

    public MemberMaker(string name, object obj){
        this.name = name;
        type = GetType((string)obj);
        value = convertValue((string)obj);
    }

    private string GetType(string var1){
        var type = "";
        if(var1.Contains(":")){
            var item =  var1.Split(';')[0];
            var parts = item.Split(':');
            type = "Dictionary<" + GetType(parts[0]) + ", " + GetType(parts[1]) + ">";
        }
        else if(var1.Contains(",")){
            var item = var1.Split(',')[0];
            type = "List<" + GetType(item) + ">";
        }
        else{
            float f;
            int i;
            if(int.TryParse(var1, out i)) type = "int";
            else if(float.TryParse(var1, out f)) type = "float";
            else type = "string";
        }

        return type; 
    }

    public object convertValue(string var1){
        object obj;
        //dict
        if(var1.Contains(":")){
            var dict = new Dictionary<string, object>();
            System.Array.ForEach(var1.Split(';'), o => {
                var pair = o.Split(':');

                if(pair.Length > 1){
                    var k = pair[0];
                    var v = convertValue(pair[1]);
                    dict.Add(k, v);
                }
                else{
                    
                }

            });

            obj = dict;
        }
        //list
        else if(var1.Contains(",")){
            var list = new List<object>();
            System.Array.ForEach(var1.Split(','), o => {
                list.Add(o);
            });

            obj = list;
        }
        //simple
        else{
           obj = (object) var1;
        }

        return obj;
    }
}