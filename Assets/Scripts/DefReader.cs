using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fs {
    public class DefReader {
        public Dictionary<string,string> keys;
        public DefReader(string path) {
            StreamReader sr = new StreamReader(path);
            string temp;
            keys=new Dictionary<string,string>();
            while(!sr.EndOfStream) {
                temp=sr.ReadLine().Trim();
                if(temp.Contains("#")) {
                    int index = temp.IndexOf('#');
                    if(index==0) continue;
                    temp=temp.Substring(0,index);
                    if(temp.Length<1) continue; //去注释
                }
                
                if(temp.Contains(":")) {
                    //属性行
                    int index = temp.IndexOf(':');
                    string keyName = temp.Substring(0,index).Trim().ToLower();
                    string value = temp.Substring(index+1,temp.Length-index-1).Trim();
                    keys.Add(keyName,value);
                }
            }
            sr.Close();
        }
    }

    static class DefLib {
        //public static DefReader prefab=new DefReader("base/def/world/prefab.def");
        public static PddFile[] pdds;
        public static void Load() {
            string temp;
            DefReader prefabReader = new DefReader(GlobalClass.GetBasePath() + "/def/world/prefab.def");
            prefabReader.keys.TryGetValue("prefab_count",out temp);
            uint prefabCount = Convert.ToUInt32(temp);

            pdds=new PddFile[prefabCount];
            for(uint i = 0;i<prefabCount;i++) {
                prefabReader.keys.TryGetValue("prefab"+i,out temp);
                if(temp.Equals("\"\"")) continue;
                pdds[i]=new PddFile(GlobalClass.GetBasePath() + temp.Substring(1,temp.Length-2).Replace(".pmd",".pdd"));
                //System.Diagnostics.Debug.WriteLine("prefab"+i+"|"+pdds[i].nodeCount);
            }
        }
    }
}
