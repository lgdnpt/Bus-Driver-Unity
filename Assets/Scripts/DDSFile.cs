using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace fs {
    class DDSFile {
        public string ddsPath;

        public uint dwMagic;
        public DDS_HEADER header;

        public Texture Texture {get => texture; set => texture=Texture;}
        private Texture texture;

        public DDSFile(string path) {
            try {
                BinaryReader br = new BinaryReader(new FileStream(path,FileMode.Open));
                Read(br);
                br.Close();
            } catch(Exception e) {
                Debug.LogError("异常发生在dds:"+path+"\n"+e);
            }
        }

        private void Read(BinaryReader br) {
            dwMagic=br.ReadUInt32();
            if(dwMagic!=0x20534444) throw new Exception("Invalid DDS header.");
            header=new DDS_HEADER(br);
            if(header.dwSize!=0x7C) throw new Exception("Invalid DDS header. Structure length is incrrrect.");

            bool mipmaps = header.dwMipMapCount > 0;
            TextureFormat textureFormat;
            switch(header.ddspf.dwFourCC) {
                case 0x0: {
                    //无压缩
                    if(header.ddspf.dwABitMask==0) {
                        textureFormat=TextureFormat.RGB24;
                    } else if(header.ddspf.dwABitMask==0xFF) {
                        textureFormat=TextureFormat.ARGB32;
                    } else {
                        if(header.ddspf.dwRBitMask==0xFF) {
                            textureFormat=TextureFormat.RGBA32;
                        } else {
                            textureFormat=TextureFormat.BGRA32;
                        }
                    }
                } break;
                case 0x31545844:
                    //DXT1
                    textureFormat=TextureFormat.DXT1;
                    break;
                case 0x35545844:
                    //DXT5
                    textureFormat=TextureFormat.DXT5;
                    break;
                default: throw new Exception("Cannot load DDS due to an unsupported pixel format.");
            }

            try {
                byte[] dxtBytes = new byte[br.BaseStream.Length-br.BaseStream.Position];
                int i = 0;
                while(br.BaseStream.Position<br.BaseStream.Length) {
                    dxtBytes[i]=br.ReadByte();
                    i++;
                }
                //Array.Reverse(dxtBytes);



                Texture2D texture2 = new Texture2D((int)header.dwWidth,(int)header.dwHeight,textureFormat,mipmaps);
                texture2.LoadRawTextureData(dxtBytes);
                texture2.Apply();
                


                texture=texture2;
            } catch(Exception ex) {
                throw new Exception("An error occured while loading DirectDraw Surface: " + ex.Message);
            }
        }


        public class DDS_HEADER {
            public uint dwSize;  //Size of structure. This member must be set to 124.
            public uint dwFlags;
            public uint dwHeight;
            public uint dwWidth;
            public uint dwPitchOrLinearSize;//The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture.
            public uint dwDepth;  //Depth of a volume texture (in pixels), otherwise unused.
            public uint dwMipMapCount;//Number of mipmap levels, otherwise unused.
            public uint[] dwReserved1;  //dwReserved1[11] Unused.
            public DDS_PIXELFORMAT ddspf; //The pixel format
            public uint dwCaps; //Specifies the complexity of the surfaces stored.
            public uint dwCaps2;//Additional detail about the surfaces stored.
            public uint dwCaps3;    //Unused.
            public uint dwCaps4;    //Unused.
            public uint dwReserved2;//Unused.

            public DDS_HEADER(BinaryReader br) {
                dwSize = br.ReadUInt32();
                dwFlags = br.ReadUInt32();
                dwHeight = br.ReadUInt32();
                dwWidth = br.ReadUInt32();
                dwPitchOrLinearSize = br.ReadUInt32();
                dwDepth = br.ReadUInt32();
                dwMipMapCount = br.ReadUInt32();
                dwReserved1=new uint[11];
                for(int i = 0;i<11;i++)  dwReserved1[i]=br.ReadUInt32();
                ddspf = new DDS_PIXELFORMAT(br);
                dwCaps = br.ReadUInt32();
                dwCaps2 = br.ReadUInt32();
                dwCaps3 = br.ReadUInt32();
                dwCaps4 = br.ReadUInt32();
                dwReserved2 = br.ReadUInt32();
            }
        }

        public class DDS_PIXELFORMAT {
            public uint dwSize;  //Structure size; set to 32 (bytes).
            public uint dwFlags; //Values which indicate what type of data is in the surface.
            public uint dwFourCC;//Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5.
            public uint dwRGBBitCount;//Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
            public uint dwRBitMask;//Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
            public uint dwGBitMask;//Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
            public uint dwBBitMask;//Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
            public uint dwABitMask;//Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
            
            /*dwFourCC
            R8G8B8=0x0
            DXT1=0x31545844
            DXT2=0x32545844
            DXT3=0x33545844
            DXT4=0x34545844
            DXT5=0x35545844
            */
            public DDS_PIXELFORMAT(BinaryReader br) {
                dwSize = br.ReadUInt32();
                dwFlags = br.ReadUInt32();
                dwFourCC = br.ReadUInt32();
                dwRGBBitCount = br.ReadUInt32();
                dwRBitMask = br.ReadUInt32();
                dwGBitMask = br.ReadUInt32();
                dwBBitMask = br.ReadUInt32();
                dwABitMask = br.ReadUInt32();
            }
        }

        public enum DXGI_FORMAT {
            DXGI_FORMAT_UNKNOWN,
            DXGI_FORMAT_R32G32B32A32_TYPELESS,
            DXGI_FORMAT_R32G32B32A32_FLOAT,
            DXGI_FORMAT_R32G32B32A32_UINT,
            DXGI_FORMAT_R32G32B32A32_SINT,
            DXGI_FORMAT_R32G32B32_TYPELESS,
            DXGI_FORMAT_R32G32B32_FLOAT,
            DXGI_FORMAT_R32G32B32_UINT,
            DXGI_FORMAT_R32G32B32_SINT,
            DXGI_FORMAT_R16G16B16A16_TYPELESS,
            DXGI_FORMAT_R16G16B16A16_FLOAT,
            DXGI_FORMAT_R16G16B16A16_UNORM,
            DXGI_FORMAT_R16G16B16A16_UINT,
            DXGI_FORMAT_R16G16B16A16_SNORM,
            DXGI_FORMAT_R16G16B16A16_SINT,
            DXGI_FORMAT_R32G32_TYPELESS,
            DXGI_FORMAT_R32G32_FLOAT,
            DXGI_FORMAT_R32G32_UINT,
            DXGI_FORMAT_R32G32_SINT,
            DXGI_FORMAT_R32G8X24_TYPELESS,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT,
            DXGI_FORMAT_R10G10B10A2_TYPELESS,
            DXGI_FORMAT_R10G10B10A2_UNORM,
            DXGI_FORMAT_R10G10B10A2_UINT,
            DXGI_FORMAT_R11G11B10_FLOAT,
            DXGI_FORMAT_R8G8B8A8_TYPELESS,
            DXGI_FORMAT_R8G8B8A8_UNORM,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB,
            DXGI_FORMAT_R8G8B8A8_UINT,
            DXGI_FORMAT_R8G8B8A8_SNORM,
            DXGI_FORMAT_R8G8B8A8_SINT,
            DXGI_FORMAT_R16G16_TYPELESS,
            DXGI_FORMAT_R16G16_FLOAT,
            DXGI_FORMAT_R16G16_UNORM,
            DXGI_FORMAT_R16G16_UINT,
            DXGI_FORMAT_R16G16_SNORM,
            DXGI_FORMAT_R16G16_SINT,
            DXGI_FORMAT_R32_TYPELESS,
            DXGI_FORMAT_D32_FLOAT,
            DXGI_FORMAT_R32_FLOAT,
            DXGI_FORMAT_R32_UINT,
            DXGI_FORMAT_R32_SINT,
            DXGI_FORMAT_R24G8_TYPELESS,
            DXGI_FORMAT_D24_UNORM_S8_UINT,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT,
            DXGI_FORMAT_R8G8_TYPELESS,
            DXGI_FORMAT_R8G8_UNORM,
            DXGI_FORMAT_R8G8_UINT,
            DXGI_FORMAT_R8G8_SNORM,
            DXGI_FORMAT_R8G8_SINT,
            DXGI_FORMAT_R16_TYPELESS,
            DXGI_FORMAT_R16_FLOAT,
            DXGI_FORMAT_D16_UNORM,
            DXGI_FORMAT_R16_UNORM,
            DXGI_FORMAT_R16_UINT,
            DXGI_FORMAT_R16_SNORM,
            DXGI_FORMAT_R16_SINT,
            DXGI_FORMAT_R8_TYPELESS,
            DXGI_FORMAT_R8_UNORM,
            DXGI_FORMAT_R8_UINT,
            DXGI_FORMAT_R8_SNORM,
            DXGI_FORMAT_R8_SINT,
            DXGI_FORMAT_A8_UNORM,
            DXGI_FORMAT_R1_UNORM,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP,
            DXGI_FORMAT_R8G8_B8G8_UNORM,
            DXGI_FORMAT_G8R8_G8B8_UNORM,
            DXGI_FORMAT_BC1_TYPELESS,
            DXGI_FORMAT_BC1_UNORM,
            DXGI_FORMAT_BC1_UNORM_SRGB,
            DXGI_FORMAT_BC2_TYPELESS,
            DXGI_FORMAT_BC2_UNORM,
            DXGI_FORMAT_BC2_UNORM_SRGB,
            DXGI_FORMAT_BC3_TYPELESS,
            DXGI_FORMAT_BC3_UNORM,
            DXGI_FORMAT_BC3_UNORM_SRGB,
            DXGI_FORMAT_BC4_TYPELESS,
            DXGI_FORMAT_BC4_UNORM,
            DXGI_FORMAT_BC4_SNORM,
            DXGI_FORMAT_BC5_TYPELESS,
            DXGI_FORMAT_BC5_UNORM,
            DXGI_FORMAT_BC5_SNORM,
            DXGI_FORMAT_B5G6R5_UNORM,
            DXGI_FORMAT_B5G5R5A1_UNORM,
            DXGI_FORMAT_B8G8R8A8_UNORM,
            DXGI_FORMAT_B8G8R8X8_UNORM,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM,
            DXGI_FORMAT_B8G8R8A8_TYPELESS,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB,
            DXGI_FORMAT_B8G8R8X8_TYPELESS,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB,
            DXGI_FORMAT_BC6H_TYPELESS,
            DXGI_FORMAT_BC6H_UF16,
            DXGI_FORMAT_BC6H_SF16,
            DXGI_FORMAT_BC7_TYPELESS,
            DXGI_FORMAT_BC7_UNORM,
            DXGI_FORMAT_BC7_UNORM_SRGB,
            DXGI_FORMAT_AYUV,
            DXGI_FORMAT_Y410,
            DXGI_FORMAT_Y416,
            DXGI_FORMAT_NV12,
            DXGI_FORMAT_P010,
            DXGI_FORMAT_P016,
            DXGI_FORMAT_420_OPAQUE,
            DXGI_FORMAT_YUY2,
            DXGI_FORMAT_Y210,
            DXGI_FORMAT_Y216,
            DXGI_FORMAT_NV11,
            DXGI_FORMAT_AI44,
            DXGI_FORMAT_IA44,
            DXGI_FORMAT_P8,
            DXGI_FORMAT_A8P8,
            DXGI_FORMAT_B4G4R4A4_UNORM,
            DXGI_FORMAT_P208,
            DXGI_FORMAT_V208,
            DXGI_FORMAT_V408,
            DXGI_FORMAT_SAMPLER_FEEDBACK_MIN_MIP_OPAQUE,
            DXGI_FORMAT_SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE,
            DXGI_FORMAT_FORCE_UINT
        };
    }
}
