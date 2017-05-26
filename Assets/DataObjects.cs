using System.Collections.Generic;
using ProtoBuf;
using Newtonsoft.Json;
namespace Data {

	[ProtoContract]
	public class test1 {
 		[ProtoMember(1)]
		public int id;
		[ProtoMember(2)]
		public string name;
		[ProtoMember(3)]
		public Dictionary<string, List<string>> dictstr = new Dictionary<string, List<string>>();
		[ProtoMember(4)]
		public Dictionary<int, List<int>> dictint = new Dictionary<int, List<int>>();
 

		private static Dictionary<string, test1> items;

        public static test1 GetData(string id){
			var path = "test1.bin";
			if(items == null){
				if(System.IO.File.Exists(path)){
					using (var file = System.IO.File.OpenRead(path)){
						items = Serializer.Deserialize<Dictionary<string, test1>>(file);
					}
				}
				else{
					//can not find this file
				}
			}
			
			return items.ContainsKey(id) ? items[id] : null;
		}

		public static Dictionary<string, test1> getItems(string json){
			items = JsonConvert.DeserializeObject<Dictionary<string, test1>>(json);
			return items;
		}

 
	}

	[ProtoContract]
	public class test2 {
 		[ProtoMember(1)]
		public int id;
		[ProtoMember(2)]
		public string name;
		[ProtoMember(3)]
		public Dictionary<string, List<string>> dictstr = new Dictionary<string, List<string>>();
		[ProtoMember(4)]
		public Dictionary<int, List<int>> dictint = new Dictionary<int, List<int>>();
		[ProtoMember(5)]
		public List<int> listint = new List<int>();
 

		private static Dictionary<string, test2> items;

        public static test2 GetData(string id){
			var path = "test2.bin";
			if(items == null){
				if(System.IO.File.Exists(path)){
					using (var file = System.IO.File.OpenRead(path)){
						items = Serializer.Deserialize<Dictionary<string, test2>>(file);
					}
				}
				else{
					//can not find this file
				}
			}
			
			return items.ContainsKey(id) ? items[id] : null;
		}

		public static Dictionary<string, test2> getItems(string json){
			items = JsonConvert.DeserializeObject<Dictionary<string, test2>>(json);
			return items;
		}

 
	}

}