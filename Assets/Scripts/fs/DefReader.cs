using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fs {
    public class DefReader {
        public string extdefs;
        public Dictionary<string,string> keys;
        public DefReader(string path) {
            Encoding encoding = GetEncoding(path);
            StreamReader sr = new StreamReader(path,encoding);
            System.Diagnostics.Debug.WriteLine(path+"|"+encoding);
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
                if(temp.Contains("extdefs")) {
                    int index = temp.IndexOf('"');
                    int index2 = temp.LastIndexOf('"');
                    if(index<0 || index==index2) continue;
                    extdefs=temp.Substring(index+1,index2-index-1);
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

        public Encoding GetEncoding(string filePath) {
            try { 
                FileStream fs = new FileStream(filePath,FileMode.Open,FileAccess.Read);
                Encoding r = GetType(fs);
                fs.Close();
                return r;
            } catch(Exception e) {
                UnityEngine.Debug.LogError(e+" | "+filePath);
                return Encoding.GetEncoding("GBK");
            }
        }
        public Encoding GetType(Stream fs) {
            byte[] Unicode = new byte[] { 0xFF,0xFE,0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE,0xFF,0x00 };
            byte[] UTF8 = new byte[] { 0xEF,0xBB,0xBF }; //带BOM
            Encoding reVal = Encoding.GetEncoding("GBK");

            BinaryReader r = new BinaryReader(fs,System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(),out i);
            byte[] ss = r.ReadBytes(i);
            if(IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)) {
                reVal = Encoding.UTF8;
            } else if(ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00) {
                reVal = Encoding.BigEndianUnicode;
            } else if(ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41) {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }
        private bool IsUTF8Bytes(byte[] data) {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for(int i = 0;i < data.Length;i++) {
                curByte = data[i];
                if(charByteCounter == 1) {
                    if(curByte >= 0x80) {
                        //判断当前
                        while(((curByte <<= 1) & 0x80) != 0) {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                        if(charByteCounter == 1 || charByteCounter > 6) {
                            return false;
                        }
                    }
                } else {
                    //若是UTF-8 此时第一位必须为1
                    if((curByte & 0xC0) != 0x80) {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if(charByteCounter > 1) {
                throw new Exception("非预期的byte格式");
            }
            return true;
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
