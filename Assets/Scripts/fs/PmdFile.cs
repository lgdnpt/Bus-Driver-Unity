using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fs {
    class PmdFile {
        public uint version;

        public uint materialCount;
        public uint lookCount;
        public uint meshCount;  //(matches amount of meshes in PMG)
        public uint variantCount;
        public uint groupCount; //(matches amount of groups in PMG)
        public uint attribCount;//(matches amount of groups in PMG)

        public uint attribValueSize;   //m_dwSizeMatMap
        public uint materialPathLength;//m_dwSizeMatText

        public uint lookOffset;
        public uint variantOffset;
        public uint partAttribOffset;
        public uint attribValueOffset;
        public uint attribOffset;
        public uint matrialOffset;
        public uint matrialPathOffset;
        //↑size=64

        public Token[] looks;
        public Token[] variants;

        public AttribLink[] attribLinks;
        public AttribDef[] attribDefs;

        public byte[] attribValuses;

        public uint[] matrialOffsets;
        public byte[] materialPaths;
        public string[] matPath;

        public PmdFile(string path) {
            Read(new BinaryReader(new FileStream(GlobalClass.GetBasePath() + path,FileMode.Open)));
        }

        public void Read(BinaryReader br) {
            version=br.ReadUInt32();

            materialCount=br.ReadUInt32();
            lookCount=br.ReadUInt32();
            meshCount=br.ReadUInt32();
            variantCount=br.ReadUInt32();
            groupCount=br.ReadUInt32();
            attribCount=br.ReadUInt32();

            attribValueSize=br.ReadUInt32();
            materialPathLength=br.ReadUInt32();

            lookOffset=br.ReadUInt32();
            variantOffset=br.ReadUInt32();
            partAttribOffset=br.ReadUInt32();
            attribValueOffset=br.ReadUInt32();
            attribOffset=br.ReadUInt32();
            matrialOffset=br.ReadUInt32();
            matrialPathOffset=br.ReadUInt32();

            looks=new Token[lookCount];
            for(uint i = 0;i<lookCount;i++) {
                looks[i]=new Token(br);
            }
            variants=new Token[variantCount];
            for(uint i = 0;i<variantCount;i++) {
                variants[i]=new Token(br);
            }

            attribLinks=new AttribLink[groupCount];
            for(uint i = 0;i<groupCount;i++) {
                attribLinks[i]=new AttribLink(br);
            }
            attribDefs=new AttribDef[attribCount];
            for(uint i = 0;i<attribCount;i++) {
                attribDefs[i]=new AttribDef(br);
            }

            attribValuses=new byte[variantCount*attribValueSize];
            for(uint i = 0;i<variantCount*attribValueSize;i++) {
                attribValuses[i]=br.ReadByte();
            }

            matrialOffsets=new uint[lookCount*materialCount];
            for(uint i = 0;i<lookCount*materialCount;i++) {
                matrialOffsets[i]=br.ReadUInt32();
            }

            materialPaths=new byte[materialPathLength];
            for(uint i = 0;i<materialPathLength;i++) {
                materialPaths[i]=br.ReadByte();
            }
            matPath = Encoding.ASCII.GetString(materialPaths,0,materialPaths.Length).Split('\0');
            br.Close();
        }

        public string GetMatPath(int look,int mat) {
            return matPath[look*lookCount+mat];
        }

        public class AttribLink {
            public int from;
            public int to;
            public AttribLink(BinaryReader br) {
                from=br.ReadInt32();
                to=br.ReadInt32();
            }
        }

        public class AttribDef {
            public Token name;
            public int type;   //0 = INT
            public int offset; //offset in value block
            public AttribDef(BinaryReader br) {
                name=new Token(br);
                type=br.ReadInt32();
                offset=br.ReadInt32();
            }
        }
    }
}
