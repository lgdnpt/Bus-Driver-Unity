using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fs {
    public abstract class Origin {
        public OriginType nodeType;
        public NodeBoundData dataHead;
        public uint flag;
        public byte distance;
        public abstract void Read(BinaryReader br);
    }
    public class Node {
        public Float3 position;  //位置
        public Float3 direction; //方向
        public uint backOrigin;  //上一点原点号
        public uint thisOrigin;  //下一点原点号(自身编号)
        public uint flag;        //存储位信息
        public byte divF;        //F分割
        public Node() : this(0x00) { }
        public Node(uint index) {
            position=new Float3();
            direction=new Float3();
            backOrigin=0xFFFFFFFF;
            thisOrigin=index;
            flag=0x01;
            divF=0xFF;
        }
        public Node(BinaryReader br) {
            position=new Float3(br);
            position.z=-position.z;
            direction=new Float3(br);
            direction.z=-direction.z;
            backOrigin=br.ReadUInt32();
            thisOrigin=br.ReadUInt32();
            flag=br.ReadUInt32();
            divF=br.ReadByte();
        }

        public UnityEngine.Vector3 Position => position.GetVector();
        public UnityEngine.Vector3 Direction => direction.GetVector();
    }
    public enum OriginType:uint {Road=0x02,Prefab=0x03,Building=0x01, Model=0x04, CutPlane=0x07,
        Mover=0x08, City=0x0B, QuestPoint=0x0D, NoWeather=0x0A, BusStop=0x0E, MissionModel=0x10
    }
    public class MbdFile {
        public const uint fileHead = 0xD4;
        public uint nodeCount;
        public uint originCount;
        public Origin[] origins;
        public Node[] nodes;

        public Node GetNode(uint i) => nodes[i];
        public Origin GetOrigin(uint i) => origins[i];
        public Origin GetThisOrigin(Node node) => origins[node.thisOrigin];
        public Origin GetBackOrigin(Node node) => origins[node.backOrigin];

        public MbdFile(string mapPath) {
            BinaryReader br = new BinaryReader(new FileStream(mapPath,FileMode.Open));
            if(br.ReadUInt32()!=fileHead) UnityEngine.Debug.LogError("MBD文件格式错误");
            nodeCount=br.ReadUInt32();
            originCount=br.ReadUInt32();
            //System.Diagnostics.Debug.WriteLine("节点数量:"+nodeCount+"|原点数量:"+originCount);

            DefLib.Load();
            ReadFile(br);
            br.Close();
        }

        public void ReadFile(BinaryReader br) {
            origins=new Origin[originCount];
            for(uint i = 0;i<originCount;i++) {
                uint nodeType = br.ReadUInt32();
                switch(nodeType) {
                    case 0x02: origins[i]=new Road(br); break;
                    case 0x03: origins[i]=new Prefab(br); break;
                    case 0x01: origins[i]=new Building(br); break;
                    case 0x04: origins[i]=new Model(br); break;
                    case 0x07: origins[i]=new CutPlane(br); break;
                    case 0x08: origins[i]=new Mover(br); break;
                    case 0x0B: origins[i]=new City(br); break;
                    case 0x0D: origins[i]=new QuestPoint(br); break;
                    case 0x0A: origins[i]=new NoWeather(br); break;
                    case 0x0E: origins[i]=new BusStop(br); break;
                    case 0x0F: UnityEngine.Debug.LogWarning("原点:"+i+"|AnimatedModel:"+nodeType); break;
                    case 0x10: origins[i]=new MissionModel(br); break;
                    default: {
                        UnityEngine.Debug.LogError("原点:"+i+"|错误的原点类型:"+nodeType);
                        //System.Diagnostics.Debug.WriteLine("\t\t|位置:"+br.BaseStream.Position);
                        continue;
                    }
                }
                //System.Diagnostics.Debug.WriteLine("原点:"+i+"|类型:"+origins[i].ToString()+"|位置:"+br.BaseStream.Position);
            }
            nodes=new Node[nodeCount];
            for(uint i = 0;i<nodeCount;i++) {
                nodes[i]=new Node(br);
                //System.Diagnostics.Debug.WriteLine(string.Format("点:{0:g} |位置:({1:g},{2:g},{3:g})",
                //    i,nodes[i].position.x,nodes[i].position.y,nodes[i].position.z));
            }
            br.Close();
        }

        public class Road:Origin {
            public uint roadNum;
            public uint matNum;
            public uint startIndex;
            public uint endIndex;
            public RoadData dataLeft;
            public RoadData dataRight;
            public ushort centerNum;
            public float tangent;
            public uint unknown;
            public uint signCount;
            public SignData[] signs;
            public uint brushCount;
            public BrushData[] brushs;
            public Road() : this(0x00) { }
            public Road(uint index) {
                nodeType = (OriginType)0x02;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                roadNum=0x00;
                matNum=0x00;
                startIndex=index;
                endIndex=index+1;
                dataRight=new RoadData();
                dataLeft=new RoadData();
                centerNum=0x00;
                tangent=10.0f;
                unknown=0x000186A0;
                signCount=0x00;
                brushCount=0x00;
            }
            public Road(BinaryReader br) {
                nodeType = (OriginType)0x02;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                roadNum=br.ReadUInt32();
                matNum=br.ReadUInt32();
                startIndex=br.ReadUInt32();
                endIndex=br.ReadUInt32();
                dataRight=new RoadData(br);
                dataLeft=new RoadData(br);
                centerNum=br.ReadUInt16();
                tangent=br.ReadSingle();
                unknown=br.ReadUInt32();
                signCount=br.ReadUInt32();
                if(signCount>0) {
                    signs=new SignData[signCount];
                    for(int i = 0;i<signCount;i++) {
                        signs[i]=new SignData(br);
                    }
                }
                brushCount=br.ReadUInt32();
                if(brushCount>0) {
                    brushs=new BrushData[brushCount];
                    for(int i = 0;i<brushCount;i++) {
                        brushs[i]=new BrushData(br);
                    }
                }
            }
            public override string ToString() {
                return string.Format("Road(编号:{0:g},材质:{1:g}",roadNum,matNum);
            }

            public enum Flag:uint {
                LeftNoise50 = 0x01,
                LeftNoise00 = 0x02,
                RightNoise50 = 0x04,
                RightNoise00 = 0x08,
                LeftTrans8 = 0x10,
                LeftTrans4 = 0x20,
                RightTrans8 = 0x40,
                RightTrans4 = 0x80,

                LeftSidewalk12 = 0x0100,
                LeftSidewalk02 = 0x0200,
                RightSidewalk12 = 0x0400,
                RightSidewalk02 = 0x0800,
                NoRandomSign = 0x2000,   //无随机物体?
                Cracks = 0x4000,

                Terrain = 0x010000,  //TODO: 道路模型 or 模型 ? railing/model
                RightShiftModels = 0x020000,
                LeftShiftModels = 0x040000,
                CityRoad = 0x080000,
                RightInvertRailing = 0x100000,
                LeftInvertRailing = 0x200000,
                NoTraffic = 0x400000,

                HalfRoadStep = 0x01000000,
                ShowInUiMap = 0x02000000,  //关gps?
                NoSpeedSigns = 0x04000000, //无随机标志?
                NoBoundary = 0x08000000
            }
        }

        public class RoadData {
            public ushort railNum;
            public ushort railOffset;
            public ushort modelNum;
            public ushort modelOffset;
            public ushort modelDistance;
            public ushort terrMatNum;
            public ushort terrQuad;
            public ushort terrType;
            public float terrCoef;
            public ushort tree;
            public ushort sidewalk;
            public uint height;
            public RoadData() {
                railNum=0xFFFF;
                railOffset=0x00;
                modelNum=0xFFFF;
                modelOffset=0x00;
                modelDistance=0x28;
                terrMatNum=0x00;
                terrQuad=0x08;
                terrType=0x00;
                terrCoef=1.0f;
                tree=0xFFFF;
                sidewalk=0xFFFF;
                height=0x00;
            }
            public RoadData(BinaryReader br) {
                railNum=br.ReadUInt16();
                railOffset=br.ReadUInt16();
                modelNum=br.ReadUInt16();
                modelOffset=br.ReadUInt16();
                modelDistance=br.ReadUInt16();
                terrMatNum=br.ReadUInt16();
                terrQuad=br.ReadUInt16();
                terrType=br.ReadUInt16();
                terrCoef=br.ReadSingle();
                tree=br.ReadUInt16();
                sidewalk=br.ReadUInt16();
                height=br.ReadUInt32();
            }
        }
        public class SignData {
            public uint signNum;
            public float offset;
            public uint flag;
            public float posCoef;
            public float height;
            public float rotation;
            public SignSubData data1;
            public SignSubData data2;
            public SignSubData data3;
            public SignData() {
                signNum=0x00;
                offset=0.0f;
                flag=0x00;
                posCoef=0.5f;
                height=0.0f;
                rotation=0.0f;
                data1=new SignSubData();
                data2=new SignSubData();
                data3=new SignSubData();
            }
            public SignData(BinaryReader br) {
                signNum=br.ReadUInt32();
                offset=br.ReadSingle();
                flag=br.ReadUInt32();
                posCoef=br.ReadSingle();
                height=br.ReadSingle();
                rotation=br.ReadSingle();
                data1=new SignSubData(br);
                data2=new SignSubData(br);
                data3=new SignSubData(br);
            }

            public class SignSubData {
                public ulong road;
                public ulong text1;
                public ulong text2;
                public SignSubData() {
                    road=0x00;
                    text1=0x00;
                    text2=0x00;
                }
                public SignSubData(BinaryReader br) {
                    road=br.ReadUInt64();
                    text1=br.ReadUInt64();
                    text2=br.ReadUInt64();
                }
            }

            public enum Flag {
                _1Left = 0x01,
                _1Right = 0x02,
                _2Left = 0x04,
                _2Right = 0x08,
                _3Left = 0x10,
                _3Right = 0x20,

                _1N = 0x0100,
                _1E = 0x0200,
                _1W = 0x0400,
                _2N = 0x1000,
                _2E = 0x2000,
                _2W = 0x4000,

                _3N = 0x010000,
                _3E = 0x020000,
                _3W = 0x040000,

                FollowRoad = 0x01000000
            }
        }
        public class BrushData {
            public uint matNum;
            public uint stampNum;
            public int tIndex;
            public int nIndex;
            public BrushData(uint matNum,uint stampNum,int tIndex,int nIndex) {
                this.matNum=matNum;
                this.stampNum=stampNum;
                this.tIndex=tIndex;
                this.nIndex=nIndex;
            }
            public BrushData(BinaryReader br) {
                matNum=br.ReadUInt32();
                stampNum=br.ReadUInt32();
                tIndex=br.ReadInt32();
                nIndex=br.ReadInt32();
            }
        }

        public class Prefab:Origin {
            public uint prefabNum;     //路口表中编号
            public uint lookNum;       //look编号
            //public uint redIndex;      //原点编号
            public uint[] indexs;     //点编号数组
            public short unknown;      //0分隔符
            public short nodeIndex;   //pdd内原点号
            public short rotY;        //Y旋转
            public short rotZ;        //Z旋转
            public PrefabTerrain[] terrain;//地形数组
            public Prefab() : this(0x00) { }
            public Prefab(uint index) {
                nodeType = (OriginType)0x03;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                prefabNum =0x00;
                lookNum=0x00;

                indexs=new uint[4] {index, index+1,index+2,index+3};

                unknown=0x00;
                nodeIndex=0x00;
                rotY=0x00;
                rotZ=0x00;
                terrain=new PrefabTerrain[4];
                for(uint i = 0;i<terrain.Length;i++) {
                    terrain[i]=new PrefabTerrain();
                }
            }
            public Prefab(BinaryReader br) {
                nodeType = (OriginType)0x03;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                prefabNum =br.ReadUInt32();
                lookNum=br.ReadUInt32();

                int prefabnodeCount = DefLib.pdds[prefabNum].nodeCount;
                indexs=new uint[prefabnodeCount];
                for(uint i = 0;i<prefabnodeCount;i++) {
                    indexs[i]=br.ReadUInt32();
                }

                unknown=br.ReadInt16();
                nodeIndex=br.ReadInt16();
                rotY=(short)-br.ReadInt16();
                rotZ=br.ReadInt16();
                terrain=new PrefabTerrain[prefabnodeCount];
                for(uint i = 0;i<prefabnodeCount;i++) {
                    terrain[i]=new PrefabTerrain(br);
                }
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("Prefab(");
                sb.Append("编号:");
                sb.Append(prefabNum);
                sb.Append(",外观:");
                sb.Append(lookNum);
                sb.Append(")");
                return sb.ToString();
            }
        }
        public class PrefabTerrain {
            public byte terLength; //地形长度,平方
            public byte terNum;    //地形表中编号
            public uint empty;     //0分隔符
            public float terCoef;  //地形变形系数
            public ushort tree;    //植物
            public PrefabTerrain() {
                terLength=0x00;
                terNum=0x00;
                empty=0x00;
                terCoef=1.0f;
                tree=0xFF;
            }
            public PrefabTerrain(BinaryReader br) {
                terLength=br.ReadByte();
                terNum=br.ReadByte();
                empty=br.ReadUInt32();
                terCoef=br.ReadSingle();
                tree=br.ReadUInt16();
            }
        }

        public class Building:Origin {
            public uint buildingNum;
            public uint index0;
            public uint index1;
            public uint index2;
            public uint index3;
            public float length;
            public uint firstBuilding;
            public uint fbLook;
            public uint seed;
            public float float1;
            public uint modelCount;
            public ModelData[] models;
            public uint addCount;
            public AddData[] adds;
            public Building(uint index) {
                nodeType = (OriginType)0x01;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                buildingNum=0x00;
                index0=index;
                index1=index+1;
                index2=index+2;
                index3=index+3;
                length=10.0f;
                firstBuilding=0xFFFFFFFF;
                fbLook=0xFFFFFFFF;
                seed=0x02;
                float1=0.0f;
                modelCount=0x00;
                addCount=0x00;
            }
            public Building(BinaryReader br) {
                nodeType = (OriginType)0x01;
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                buildingNum=br.ReadUInt32();
                index0=br.ReadUInt32();
                index1=br.ReadUInt32();
                index2=br.ReadUInt32();
                index3=br.ReadUInt32();
                length=br.ReadSingle();
                firstBuilding=br.ReadUInt32();
                fbLook=br.ReadUInt32();
                seed=br.ReadUInt32();
                float1=br.ReadSingle();
                modelCount=br.ReadUInt32();
                models=new ModelData[modelCount];
                for(uint i=0;i<modelCount;i++) {
                    models[i].modelNum=br.ReadByte();
                    models[i].baseLook=br.ReadByte();
                    models[i].topLook=br.ReadByte();
                    models[i].accLook=br.ReadByte();
                }

                addCount=br.ReadUInt32();
                adds=new AddData[addCount];
                for(uint i = 0;i<addCount;i++) {
                    adds[i].baseModel=br.ReadByte();
                    adds[i].topModel=br.ReadByte();
                    adds[i].accModel=br.ReadByte();
                    adds[i].unbyte=br.ReadByte();
                }
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("Building(");
                sb.Append("编号:");
                sb.Append(buildingNum);
                sb.Append(",长度:");
                sb.Append(length);
                sb.Append(")");
                return sb.ToString();
            }

            public struct ModelData {
                public byte modelNum;
                public byte baseLook;
                public byte topLook;
                public byte accLook;
            }
            public struct AddData {
                public byte baseModel;
                public byte topModel;
                public byte accModel;
                public byte unbyte;
            }
        }
        
        public class Model:Origin {
            public uint modelNum;     //模型表中编号
            public uint matNum;       //模型变体
            public uint nodeIndex;    //原点号
            public Float3 rotation;  //旋转
            public Float3 scale;     //缩放
            public Model() : this(0x00) { }
            public Model(uint index) {
                nodeType = (OriginType)0x04;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                modelNum =0x00;
                matNum=0x00;
                nodeIndex=index;
                rotation=new Float3();
                scale=new Float3();
            }
            public Model(BinaryReader br) {
                nodeType = (OriginType)0x04;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                modelNum =br.ReadUInt32();
                matNum=br.ReadUInt32();
                nodeIndex=br.ReadUInt32();
                rotation=new Float3(br);
                rotation.y=-rotation.y;
                scale=new Float3(br);
                //scale.z=-scale.z;
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("Model(");
                sb.Append("编号:");
                sb.Append(modelNum);
                sb.Append(",外观:");
                sb.Append(matNum);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class CutPlane:Origin {
            public uint index1;  //起点编号
            public uint index2;  //终点编号
            public CutPlane(uint index) {
                nodeType = (OriginType)0x07;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                index1=index;
                index2=index+1;
            }
            public CutPlane(BinaryReader br) {
                nodeType = (OriginType)0x07;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                index1=br.ReadUInt32();
                index2=br.ReadUInt32();
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("CutPlane(");
                sb.Append("起点:");
                sb.Append(index1);
                sb.Append(",终点:");
                sb.Append(index2);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class Mover:Origin {
            public ushort moverNum;   //mover表中编号
            public float speed;       //速度
            public float delay;       //delay at end,延迟时间
            public uint tangentCount; //控制线数量
            public float[] tangents;  //控制线长度
            public uint nodeCount;    //节点数量
            public uint[] nodeIndexs; //节点编号

            /*bits
            白天移动 06 0000 0010
            白天移动 04 0000 0100
            往返移动 08 0000 1000
            直线移动 10 0001 0000
            曲线移动 01 0000 0001
            */
            public enum Flag {
                UseCurve = 0x01,
                ActiveDay = 0x02,
                ActiveNight = 0x04,
                BounceAtEnd = 0x08,
                FollowDir = 0x10
            }

            public Mover() : this(0x00) { }
            public Mover(uint index) {
                nodeType = (OriginType)0x08;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                moverNum=0x00;
                speed=1.0f;
                delay=0.0f;
                tangentCount=0x01;
                tangents=new float[tangentCount];
                for(uint i = 0;i<tangentCount;i++) {
                    tangents[i]=2.0f;
                }
                nodeCount=0x02;
                nodeIndexs=new uint[nodeCount];
                for(uint i = 0;i<nodeCount;i++) {
                    nodeIndexs[i]=index+i;
                }
            }
            public Mover(BinaryReader br) {
                nodeType = (OriginType)0x08;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                moverNum=br.ReadUInt16();
                speed=br.ReadSingle();
                delay=br.ReadSingle();
                tangentCount=br.ReadUInt32();
                tangents=new float[tangentCount];
                for(uint i = 0;i<tangentCount;i++) {
                    tangents[i]=br.ReadSingle();
                }
                nodeCount=br.ReadUInt32();
                nodeIndexs=new uint[nodeCount];
                for(uint i = 0;i<nodeCount;i++) {
                    nodeIndexs[i]=br.ReadUInt32();
                }
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("Mover(");
                sb.Append("编号:");
                sb.Append(moverNum);
                sb.Append(",节点数量:");
                sb.Append(nodeCount);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class City:Origin {
            public ulong cityName; //城市名,long存储字符串
            public float width;    //宽度
            public float height;   //高度
            public uint nodeIndex; //原点号
            public City() : this(0x00) { }
            public City(uint index) {
                nodeType = (OriginType)0x0B;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                cityName =0x00;
                width=100.0f;
                height=100.0f;
                nodeIndex=index;
            }
            public City(BinaryReader br) {
                nodeType = (OriginType)0x0B;
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                cityName =br.ReadUInt64();
                width=br.ReadSingle();
                height=br.ReadSingle();
                nodeIndex=br.ReadUInt32();
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("City(");
                sb.Append("城市名:");
                sb.Append(Token.getString(cityName));
                sb.Append(",宽度:");
                sb.Append(width);
                sb.Append(",高度:");
                sb.Append(height);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class QuestPoint:Origin {
            public ulong missionName; //线路名,long存储字符串
            public uint nodeCount;    //节点数
            public uint[] nodeIndexs;  //节点编号数组
            public QuestPoint() : this(0x00) { }
            public QuestPoint(uint index) {
                nodeType = (OriginType)0x0D;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                missionName =0x00;
                nodeCount=0x02;
                nodeIndexs=new uint[nodeCount];
                for(uint i = 0;i<nodeCount;i++) {
                    nodeIndexs[i]=index+i;
                }
            }
            public QuestPoint(BinaryReader br) {
                nodeType = (OriginType)0x0D;
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                missionName =br.ReadUInt64();
                nodeCount=br.ReadUInt32();
                nodeIndexs=new uint[nodeCount];
                for(int i = 0;i<nodeCount;i++) {
                    nodeIndexs[i]=br.ReadUInt32();
                }
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("QuestPoint(");
                sb.Append("线路名:");
                sb.Append(Token.getString(missionName));
                sb.Append(",节点数量:");
                sb.Append(nodeCount);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class NoWeather:Origin {
            public float width;    //宽度
            public float height;   //高度
            public uint nodeIndex; //原点号
            public NoWeather() : this(0x0) { }
            public NoWeather(uint index) {
                nodeType = (OriginType)0x0A;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                width =100.0f;
                height=100.0f;
                nodeIndex=index;
            }
            public NoWeather(BinaryReader br) {
                nodeType = (OriginType)0x0A;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                width =br.ReadSingle();
                height=br.ReadSingle();
                nodeIndex=br.ReadUInt32();
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("NoWeather(");
                sb.Append("宽度:");
                sb.Append(width);
                sb.Append(",高度:");
                sb.Append(height);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class BusStop:Origin {
            public ushort stopNum; //车站表中号
            public uint stopID;    //站点号
            public uint nodeIndex; //原点编号
            public BusStop() : this(0x00) { }
            public BusStop(uint index) {
                nodeType = (OriginType)0x0E;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                stopNum =0x00;
                stopID=0x00;
                nodeIndex=index;
            }
            public BusStop(BinaryReader br) {
                nodeType = (OriginType)0x0E;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                stopNum =br.ReadUInt16();
                stopID=br.ReadUInt32();
                nodeIndex=br.ReadUInt32();
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append("BusStop(");
                sb.Append("编号:");
                sb.Append(stopNum);
                sb.Append(",站点编号:");
                sb.Append(stopID);
                sb.Append(")");
                return sb.ToString();
            }
        }

        public class AnimatedModel:Origin {

            public override void Read(BinaryReader br) {
                nodeType = (OriginType)0x0F;
                throw new NotImplementedException();
            }
        }
        public class MissionModel:Origin {
            public uint modelNum;       //模型表中编号
            public uint matNum;         //模型变体号
            public uint nodeIndex;      //原点编号
            public Float3 rotation;    //旋转
            public Float3 scale;       //缩放
            public uint missionCount;   //线路数量
            public ulong[] missionName; //线路名,long存储字符串
            public MissionModel() : this(0x00) { }
            public MissionModel(uint index) {
                nodeType = (OriginType)0x10;
                dataHead=new NodeBoundData();
                flag=0x00;
                distance=0x00;
                modelNum =0x00;
                matNum=0x00;
                nodeIndex=index;
                rotation=new Float3();
                scale=new Float3();
                missionCount=0x00;
            }
            public MissionModel(BinaryReader br) {
                nodeType = (OriginType)0x10;
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                modelNum =br.ReadUInt32();
                matNum=br.ReadUInt32();
                nodeIndex=br.ReadUInt32();
                rotation=new Float3(br);
                rotation.y=-rotation.y;
                scale=new Float3(br);
                missionCount=br.ReadUInt32();
                missionName=new ulong[missionCount];
                for(int i = 0;i<missionCount;i++) {
                    missionName[i]=br.ReadUInt64();
                }
            }
            public override string ToString() {
                return string.Format("MissionModel(编号:{0:g},外观:{1:g},线路数量:{2:g}",modelNum,matNum,missionCount);
            }
        }


        public class MapData {
            public Float3 runmap;    //runmap位置
            public uint str1, str2, str3; //固定串3d1t0rh34d3r
            public Float3 camera;    //视角位置
            public MapData() {
                runmap=new Float3();
                camera=new Float3();
            }
            public MapData(BinaryReader br) {
                runmap=new Float3(br);
                str1=br.ReadUInt32();
                str2=br.ReadUInt32();
                str3=br.ReadUInt32();
                camera=new Float3(br);
            }
        }
    }
}
