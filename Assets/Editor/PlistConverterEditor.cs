using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using PlistCS;
using ProtoBuf;
using Newtonsoft.Json;

public class PlistConverterEditor : Editor {
    
    //生成类定义文件
    [MenuItem("Tools/Define DataObjects")]
    public static void DefineDataObjects(){

        var plistFiles = Directory.GetFiles("Data/");

        //命名空间
        var namespaceDefinition = new NamespaceMaker("Data");

        //遍历Data文件夹中的所有plist文件, 生成对应类定义
        foreach(var fileName in plistFiles){
            if(fileName.Contains("DS_Store")) continue;

            //取第一条数据来生成类定义
            var plistDict = (Dictionary<string, object>)Plist.readPlist(fileName);
            var dataOne = plistDict.First();

            //截取plist文件名作为类名
            var className = fileName.Replace("Data/", "").Replace(".plist", "");

            //生成类定义
            var classDefinition = new ClassMaker(className, dataOne.Value);

            //加入命名空间
            namespaceDefinition.addClass(classDefinition);
        }

        //脚本写入Assets/DataObjects.cs
        File.WriteAllText("Assets/DataObjects.cs", namespaceDefinition.code);
    }

    //序列化
    [MenuItem("Tools/ToProtobuf")]
    public static void ToProtobuf(){
        //plist data
        var plistFiles = Directory.GetFiles("Data/");

        foreach(var fileName in plistFiles){
            if(fileName.Contains("DS_Store")) continue;

            var plistDict = (Dictionary<string, object>)Plist.readPlist(fileName);

            var className = fileName.Replace("Data/", "").Replace(".plist", "");

            //反射获取类定义的 getItems 方法
            Type t = Type.GetType("Data." + className + ", Assembly-CSharp"); 
            MethodInfo mf = t.GetMethod("getItems");

            //存储类实例

            var classDict = new Dictionary<string, object>();
            foreach(var d in plistDict){
                var classInstance = new ClassMaker(className, d.Value);

                //dictForJson 把成员变量存在dict中, 用于json转换成类实例
                classDict.Add(d.Key, classInstance.dictForJson);
            }

            //为了获取完整的类实例,需要借助json库的反射功能, 所以要先把数据转换成json
            var json = JsonConvert.SerializeObject(classDict);

            //通过反射调用getItems获取到完整的类实例, 最终protobuf序列化并存储
            using (var file = File.Create(Application.dataPath + "/ProtobufOutput/" + className + ".bin")) {
                Serializer.Serialize(file, mf.Invoke(null, new object[]{json}));
            }
        }
    }

    [MenuItem("Tools/ProtobufTest")]
    public static void ProtobufTest(){
        var data = Data.test2.GetData("1");
        Debug.Log("data:" + data.name);
        Debug.Log("key: " + data.dictstr.First().Key + " value: " + data.dictstr.First().Value[0]);
    }
}