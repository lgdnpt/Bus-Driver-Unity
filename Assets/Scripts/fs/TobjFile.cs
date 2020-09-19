using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace fs {
    public class TobjFile {
        public string tobjPath;
        //header
        public uint commonHead; //1890650625
        //public uint unint0;
        //public uint unint1;
        //public uint unint2;
        //public uint unint3;
        //public ushort unshort0;
        public byte bias;
        public byte unbyte0;
        public byte type;      // { map_1d = 1, map_2d = 2, map_3d = 3, map_cube = 5 }
        public byte unbyte1;
        public byte filterMag; // { nearest = 0, linear = 1, unknown = 2, _default_ = 3 }
        public byte filterMin;
        public byte filterMip;
        public byte unbyte2;
        public byte addrU; // { repeat = 0, clamp = 1, clamp_to_edge = 2, clamp_to_border = 3, 
        public byte addrV; //   mirror = 4,  mirror_clamp = 5, mirror_clamp_to_edge = 6 }
        public byte addrW;
        public byte ui;
        //public byte unbyte3;
        //public byte unbyte4;
        //public byte unbyte5;
        //public byte unbyte6;
        public byte tsnormal;
        public byte unbyte7;
        //texture
        public uint length;
        public uint unint4;
        public string path;

        public Texture texture;

        //public static Dictionary<string,TobjFile> lib = new Dictionary<string,TobjFile>();

        public TobjFile(string path) {
            try {
                BinaryReader br = new BinaryReader(new FileStream(G.BasePath + path,FileMode.Open));
                Read(br);
                br.Close();
            } catch(Exception e) {
                Debug.LogError("异常发生在tobj:"+path+"\n"+e);
            }
        }
        private void Read(BinaryReader br) {
            commonHead=br.ReadUInt32();
            br.BaseStream.Seek(18,SeekOrigin.Current);
            //unint0=br.ReadUInt32();
            //unint1=br.ReadUInt32();
            //unint2=br.ReadUInt32();
            //unint3=br.ReadUInt32();
            //unshort0=br.ReadUInt16();
            bias=br.ReadByte();
            unbyte0=br.ReadByte();
            type=br.ReadByte();
            unbyte1=br.ReadByte();
            filterMag=br.ReadByte();
            filterMin=br.ReadByte();
            filterMip=br.ReadByte();
            unbyte2=br.ReadByte();
            addrU=br.ReadByte();
            addrV=br.ReadByte();
            addrW=br.ReadByte();
            ui=br.ReadByte();
            br.BaseStream.Seek(4,SeekOrigin.Current);
            //unbyte3=br.ReadByte();
            //unbyte4=br.ReadByte();
            //unbyte5=br.ReadByte();
            //unbyte6=br.ReadByte();
            tsnormal=br.ReadByte();
            unbyte7=br.ReadByte();

            length=br.ReadUInt32();
            unint4=br.ReadUInt32();

            //byte[] pathb =new byte[length];
            byte[] pathb = br.ReadBytes((int)length);
/*            for(uint i = 0;i<length;i++) {
                pathb[i]=br.ReadByte();
            }*/
            path=Encoding.ASCII.GetString(pathb);

            texture=LoadTexture();
        }

        private Texture LoadTexture() {
            Texture texture;
            if(!string.IsNullOrEmpty(path)) {
                if(path.IndexOf('/')==-1) {
                    Debug.LogError(tobjPath);
                    path=tobjPath.Substring(0,tobjPath.LastIndexOf('/')+1)+path;
                }
                path=path.Replace("\0","");
                Debug.Log("读取纹理:"+path);
                try {
                    texture = (new DDSFile(path)).Texture; //Dummiesman.DDSLoader.Load(GlobalClass.GetBasePath() + path);
                    texture.wrapModeU = GetWrapMode(addrU);
                    texture.wrapModeV = GetWrapMode(addrV);
                    texture.wrapModeW = GetWrapMode(addrW);
                    //texture.filterMode = FilterMode.
                    return texture;
                } catch(Exception e) {
                    Debug.LogError("DDS("+path+")读取错误:\n"+e);
                }
            } else {
                Debug.LogWarning("空纹理路径:tobj("+tobjPath+")");
            }
            return null;
        }

        private TextureWrapMode GetWrapMode(byte b) {
            switch(b) {
                case 0: return TextureWrapMode.Repeat;
                case 1: return TextureWrapMode.Clamp;
                case 2: return TextureWrapMode.Clamp;
                case 3: return TextureWrapMode.Clamp;
                case 4: return TextureWrapMode.Mirror;
                case 5: return TextureWrapMode.MirrorOnce;
                case 6: return TextureWrapMode.MirrorOnce;
                default: return TextureWrapMode.Repeat;
            }
        }
        private FilterMode GetFilterMode(byte b) {
            switch(b) {
                case 0: return FilterMode.Point;
                case 1: return FilterMode.Bilinear;
                case 2: return FilterMode.Bilinear;
                case 3: return FilterMode.Trilinear;
                default: return FilterMode.Bilinear;
            }
        }
    }
}
