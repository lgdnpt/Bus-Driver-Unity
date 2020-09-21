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

        public Texture Texture { get; private set; }

        public DDSFile(string path) {
            ddsPath=path;
            try {
                Read(G.BasePath + path);
            } catch(FileNotFoundException e) {
                Debug.LogError("[fs] Unable to open file for reading. ("+path+") |"+e);
            } catch(Exception e) {
                Debug.LogError("[dds] Can not load '"+path+"' |"+e.Message);
            }
        }

        private void Read(string path) {
            BinaryReader br = new BinaryReader(new FileStream(path,FileMode.Open));
            dwMagic=br.ReadUInt32();
            if(dwMagic!=0x20534444) throw new Exception("Invalid DDS header.");
            header=new DDS_HEADER(br);
            if(header.dwSize!=0x7C) throw new Exception("Invalid DDS header. Structure length is incrrrect.");

            bool mipmaps = header.dwMipMapCount > 0;

            
            //bool yMirror = true;
            bool compress = false;

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
                    compress=true;
                    break;
                case 0x35545844:
                    //DXT5
                    textureFormat=TextureFormat.DXT5;
                    compress=true;
                    break;
                default: {
                    br.Close();
                    throw new Exception("Cannot load DDS due to an unsupported pixel format.");
                }
            }

            int length = (int)(br.BaseStream.Length-br.BaseStream.Position);
            byte[] dxtBytes = br.ReadBytes(length);
            br.Close();

            try {
                Texture2D texture2 = new Texture2D((int)header.dwWidth,(int)header.dwHeight,textureFormat,mipmaps);
                texture2.LoadRawTextureData(dxtBytes);
                texture2.Apply();


                texture2 = FlipTexture(texture2);
                if(compress) texture2.Compress(true);
                Texture = texture2;
                /*if(yMirror) {
                } else {
                    Texture = texture2;
                }*/
                
            } catch(Exception ex) {
                br.Close();
                throw new Exception("An error occured while loading DirectDraw Surface: " + ex.Message);
            }
        }

        void FlipTextureSelf(Texture2D textureOrigin) {
            Color[] pixels = textureOrigin.GetPixels();
            Color[] pixelsFlipped = new Color[pixels.Length];

            for(int i = 0;i < header.dwHeight;i++) {
                Array.Copy(pixels,i*header.dwWidth,pixelsFlipped,(header.dwHeight-i-1) * header.dwWidth,header.dwWidth);
            }
            

            textureOrigin.SetPixels(pixelsFlipped);
            textureOrigin.Apply();
        }

        public static Texture2D FlipTexture(Texture2D textureOrigin) {
            Color[] pixels = textureOrigin.GetPixels();
            Color[] pixelsFlipped = new Color[pixels.Length];

            for(int i = 0;i < textureOrigin.height;i++) {
                Array.Copy(pixels,i*textureOrigin.width,pixelsFlipped,(textureOrigin.height-i-1) * textureOrigin.width,textureOrigin.width);
            }
            
            Texture2D t = new Texture2D(textureOrigin.width,textureOrigin.height);
            t.SetPixels(pixelsFlipped);
            t.Apply();
            return t;
        }


        public class DDS_HEADER {
            public uint dwSize;  //Size of structure. This member must be set to 124.
            public uint dwFlags;
            public uint dwHeight;
            public uint dwWidth;
            public uint dwPitchOrLinearSize;//The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture.
            public uint dwDepth;  //Depth of a volume texture (in pixels), otherwise unused.
            public uint dwMipMapCount;//Number of mipmap levels, otherwise unused.
            //public uint[] dwReserved1;  //dwReserved1[11] Unused.
            public DDS_PIXELFORMAT ddspf; //The pixel format
            public uint dwCaps; //Specifies the complexity of the surfaces stored.
            public uint dwCaps2;//Additional detail about the surfaces stored.
            //public uint dwCaps3;    //Unused.
            //public uint dwCaps4;    //Unused.
            //public uint dwReserved2;//Unused.

            public DDS_HEADER(BinaryReader br) {
                dwSize = br.ReadUInt32();
                dwFlags = br.ReadUInt32();
                dwHeight = br.ReadUInt32();
                dwWidth = br.ReadUInt32();
                dwPitchOrLinearSize = br.ReadUInt32();
                dwDepth = br.ReadUInt32();
                dwMipMapCount = br.ReadUInt32();
                br.BaseStream.Seek(44,SeekOrigin.Current);
                //dwReserved1=new uint[11];
                //for(int i = 0;i<11;i++)  dwReserved1[i]=br.ReadUInt32();
                ddspf = new DDS_PIXELFORMAT(br);
                dwCaps = br.ReadUInt32();
                dwCaps2 = br.ReadUInt32();
                br.BaseStream.Seek(12,SeekOrigin.Current);
                //dwCaps3 = br.ReadUInt32();
                //dwCaps4 = br.ReadUInt32();
                //dwReserved2 = br.ReadUInt32();
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
        }

        
    }
}
