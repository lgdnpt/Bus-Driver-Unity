using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fs {
    class PmgFile {
        //header
        public byte version;  //0x11
        public byte[] signature;// = new byte[3];
        public int meshCount;
        public int groupCount;
        public int boneCount;
        public int locatorCount;

        public BoundInfo boundInfo;

        public int boneOffset;
        public int groupOffset;
        public int locaterOffset;
        public int meshOffset;
        public int locaterNameOffset;
        public int locaterNameSize;
        public int animBindsOffset;
        public int animBindsSize;
        public int vertixOffset;
        public int vertixSize;  //verts and norms
        public int uvOffset;//uv and color
        public int uvSize;//size of UVs and color buffer
        public int triangleOffset;
        public int triangleSize;
        //header|size 116
        public byte[] padding; //header.m_bone_offset - 116
        public Bone[] bones;   //bones[header.m_bone_count]
        public Group[] groups; //parts[header.m_part_count];
        public Locator[] locators;//locators[header.m_locator_count];
        public Mesh[] meshes;  //pieces[header.m_piece_count];


        /*public Vector3[] vertex;
        public byte[] uv;
        public ushort[] triangle;*/

        public PmgFile(string path) {
            Read(new BinaryReader(new FileStream(GlobalClass.GetBasePath() + path,FileMode.Open)));
        }
        public void Read(BinaryReader br) {
            version=br.ReadByte();
            signature = br.ReadBytes(3);
            meshCount=br.ReadInt32();
            groupCount=br.ReadInt32();
            boneCount=br.ReadInt32();
            locatorCount=br.ReadInt32();

            boundInfo=new BoundInfo(br);

            boneOffset=br.ReadInt32();
            groupOffset=br.ReadInt32();
            locaterOffset=br.ReadInt32();
            meshOffset=br.ReadInt32();
            locaterNameOffset=br.ReadInt32();
            locaterNameSize=br.ReadInt32();
            animBindsOffset=br.ReadInt32();
            animBindsSize=br.ReadInt32();
            vertixOffset=br.ReadInt32();
            vertixSize=br.ReadInt32();
            uvOffset=br.ReadInt32();
            uvSize=br.ReadInt32();
            triangleOffset=br.ReadInt32();
            triangleSize=br.ReadInt32();

            /*padding=new byte[boneOffset-116];
            bones=new Bone[boneCount];
            for(int i = 0;i<boneCount;i++) {
                bones[i]=new Bone(br);
            }*/

            br.BaseStream.Seek(groupOffset,SeekOrigin.Begin);
            groups=new Group[groupCount];
            for(int i = 0;i<groupCount;i++) {
                groups[i]=new Group(br);
            }

            br.BaseStream.Seek(locaterOffset,SeekOrigin.Begin);
            locators=new Locator[locatorCount];
            for(int i = 0;i<locatorCount;i++) {
                locators[i]=new Locator(br);
            }

            br.BaseStream.Seek(meshOffset,SeekOrigin.Begin);
            meshes=new Mesh[meshCount];
            for(int i = 0;i<meshCount;i++) {
                meshes[i]=new Mesh(br);
            }

            for(int i = 0;i<meshCount;i++) {
                meshes[i].LoadVertix(br);
            }

            br.Close();
        }

        public class Bone {
            public Token name;
            public float[,] transform = new float[4,4];
            public float[,] transformReversed = new float[4,4];
            public Float4 stretch;
            public Float4 rotation;
            public Float3 translation;
            public Float3 scale;
            public float sign_of_determinant_of_matrix;
            public byte parent;//? bone_id m_parent;
            public byte[] pad=new byte[3];//pad[3]
            public Bone(BinaryReader br) {
                name=new Token(br);
                transform[0,0]=br.ReadSingle();
                transform[0,1]=br.ReadSingle();
                transform[0,2]=br.ReadSingle();
                transform[0,3]=br.ReadSingle();
                transform[1,0]=br.ReadSingle();
                transform[1,1]=br.ReadSingle();
                transform[1,2]=br.ReadSingle();
                transform[1,3]=br.ReadSingle();
                transform[2,0]=br.ReadSingle();
                transform[2,1]=br.ReadSingle();
                transform[2,2]=br.ReadSingle();
                transform[2,3]=br.ReadSingle();
                transform[3,0]=br.ReadSingle();
                transform[3,1]=br.ReadSingle();
                transform[3,2]=br.ReadSingle();
                transform[3,3]=br.ReadSingle();

                transformReversed[0,0]=br.ReadSingle();
                transformReversed[0,1]=br.ReadSingle();
                transformReversed[0,2]=br.ReadSingle();
                transformReversed[0,3]=br.ReadSingle();
                transformReversed[1,0]=br.ReadSingle();
                transformReversed[1,1]=br.ReadSingle();
                transformReversed[1,2]=br.ReadSingle();
                transformReversed[1,3]=br.ReadSingle();
                transformReversed[2,0]=br.ReadSingle();
                transformReversed[2,1]=br.ReadSingle();
                transformReversed[2,2]=br.ReadSingle();
                transformReversed[2,3]=br.ReadSingle();
                transformReversed[3,0]=br.ReadSingle();
                transformReversed[3,1]=br.ReadSingle();
                transformReversed[3,2]=br.ReadSingle();
                transformReversed[3,3]=br.ReadSingle();

                stretch=new Float4(br);
                rotation=new Float4(br);
                translation=new Float3(br);
                scale=new Float3(br);
                sign_of_determinant_of_matrix=br.ReadSingle();

                parent=br.ReadByte();
                pad[0]=br.ReadByte();
                pad[1]=br.ReadByte();
                pad[2]=br.ReadByte();
            }
        }

        public class Group {
            public Token name;
            public int meshCount;  //amount of mesh
            public int meshID;     //first mesh
            public int locatorCount;//amount of nodes
            public int locatorID;  //first node
            public Group(BinaryReader br) {
                name=new Token(br);
                meshCount=br.ReadInt32();
                meshID=br.ReadInt32();
                locatorCount=br.ReadInt32();
                locatorID=br.ReadInt32();
            }
        }

        public class Locator {
            public Token name;
            public Float3 position;//x,y,z
            public float scale;     //always 1.0f
            public Float4 rotation;//quaternion
            public int nameOffset;  //offset to string buffer, or -1 if no name.
            public Locator(BinaryReader br) {
                name=new Token(br);
                position=new Float3(br);
                scale=br.ReadSingle();
                rotation=new Float4(br);
                nameOffset=br.ReadInt32();
            }
        }

        public class Mesh {
            public uint triangleCount;
            public uint vertsCount;
            //存疑
            //public UVMask uvMask;
            //public int uvChannel;
            //public int boneChannel;
            public int unint0;

            public int material;

            public BoundInfo boundInfo;

            public int vertPositionOffset;
            public int vertNormalOffset;
            public int vertUVOffset;
            public int vertColorOffset;
            public int vertColor2Offset;
            public int vertTangentOffset;
            public int triangleOffset;
            public int bindOffset;
            public int bindBonesOffset;
            public int bindBonesWeightOffset;

            //顶点数据
            public Float3[] vertex;
            public Float3[] normal;
            public Float2[] uv;
            public uint[] color;
            public ushort[] triangle;

            public Mesh(BinaryReader br) {
                triangleCount=br.ReadUInt32();
                vertsCount=br.ReadUInt32();
                //uvMask=new UVMask();
                unint0=br.ReadInt32();
                material=br.ReadInt32();

                boundInfo=new BoundInfo(br);

                vertPositionOffset=br.ReadInt32();
                vertNormalOffset=br.ReadInt32();
                vertUVOffset=br.ReadInt32();
                vertColorOffset=br.ReadInt32();
                vertColor2Offset=br.ReadInt32();
                vertTangentOffset=br.ReadInt32();
                triangleOffset=br.ReadInt32();
                bindOffset=br.ReadInt32();
                bindBonesOffset=br.ReadInt32();
                bindBonesWeightOffset=br.ReadInt32();

            }

            public void LoadVertix(BinaryReader br) {
                //读顶点/法线
                vertex=new Float3[vertsCount];
                br.BaseStream.Seek(vertPositionOffset,SeekOrigin.Begin);
                for(int i = 0;i<vertsCount;i++) {
                    vertex[i]=new Float3(br);
                }
                normal=new Float3[vertsCount];
                br.BaseStream.Seek(vertNormalOffset,SeekOrigin.Begin);
                for(int i = 0;i<vertsCount;i++) {
                    normal[i]=new Float3(br);
                }

                uv=new Float2[vertsCount];
                br.BaseStream.Seek(vertUVOffset,SeekOrigin.Begin);
                for(int i = 0;i<vertsCount;i++) {
                    uv[i]=new Float2(br);
                }
                color=new uint[vertsCount];
                br.BaseStream.Seek(vertColorOffset,SeekOrigin.Begin);
                for(int i = 0;i<vertsCount;i++) {
                    color[i]=br.ReadUInt32();
                }

                br.BaseStream.Seek(triangleOffset,SeekOrigin.Begin);
                triangle=new ushort[triangleCount];
                for(int i = 0;i<triangleCount;i++) {
                    triangle[i]=br.ReadUInt16();
                }
            }
        }

        public class BoundInfo {
            public Float3 center;
            public float diagonalSize; //对角线尺寸
            public Float3 coord1;
            public Float3 coord2;

            public BoundInfo(BinaryReader br) {
                center=new Float3(br);
                diagonalSize=br.ReadSingle();
                coord1=new Float3(br);
                coord2=new Float3(br);
            }
        }

    }

    /*
    struct pmg {
        struct pmg_header_t     header;
        u8                      padding[header.m_bone_offset - 116 /* sizeof(pmg_header_t) /];
        struct pmg_bone_t       bones[header.m_bone_count];
        struct pmg_part_t       parts[header.m_part_count];
        struct pmg_locator_t    locators[header.m_locator_count];
        struct pmg_piece_t      pieces[header.m_piece_count];
    };
    */
}
