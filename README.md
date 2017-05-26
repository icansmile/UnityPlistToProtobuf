# UnityPlistToProtobuf
从Plist到Protobuf, 根据Plist自动生成Protobuf结构的类定义
## 使用
1. Data目录中放入plist文件
2. 在编辑器工具条中,选择 Define DataObjects 生成 DataObject.cs 类定义脚本文件
![工具条](https://git.oschina.net/nick.c/nickimage/raw/master/Protobuf/ProtobufTool.png)
3. 在编辑器工具条中,选择 ToProtobuf 将plist中的数据序列化存储在 
> Assets/ProtobufOutput/[className].bin
4. 使用 Data.类名.GetData(id) 来读取Protobuf数据
  ```
  var data = Data.test2.GetData("1");
  Debug.Log("data:" + data.name);
  Debug.Log("key: " + data.dictstr.First().Key + " value: " + data.dictstr.First().Value[0]);
  ```
 
## 记录
1. 在把plist数据转换成类实例过程中, 需要用反射对实例中的成员变量赋值, 基本类型的数据可以简单的setValue解决, 但是泛型list, dictionary则较为复杂.</br> 于是借助了json库, 先将实例的成员变量值都存入dictionary, 再通过json库将这个dictionary转成json字符串, 然后把这个字符串转换成类实例, 最后用这个类实例来序列化protobuf.

## 参考
xk
[ProtoBuf初次见面](https://yi-shiuan.github.io/2016/10/05/2016-10-06-protobuf%E5%88%9D%E6%AC%A1%E8%A6%8B%E9%9D%A2/)
