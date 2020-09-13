/*
 * Copyright (c) 2019 Dummiesman
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
*/

using Dummiesman;
using SharpConfig;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MTLLoader {
    /// <summary>
    /// 材质配置文件
    /// </summary>
    public class MaterialConfig {
        public bool enable { get; set; }
        public string shader { get; set; }
        public string renderingMode { get; set; }

        public float[] albedoColor { get; set; }
        public string albedoMap { get; set; }

        public string metallicMap { get; set; }
        public float metallicValue { get; set; }

        public float glossiness { get; set; }
        public float glossMapScale { get; set; }
        public string source { get; set; }

        public string normalMap { get; set; }
        public float normalValue { get; set; }

        public string heightMap { get; set; }
        public float heightValue { get; set; }

        public string occlusionMap { get; set; }
        public float occlusionValue { get; set; }

        public string detailMask { get; set; }

        public float[] emissionColor { get; set; }
        public string emissionMap { get; set; }

        public float[] tiling { get; set; }
        public float[] offset { get; set; }


        public string detailAlbedox2 { get; set; }
        public string normalMapx2 { get; set; }
        public float[] tilingx2 { get; set; }
        public float[] offsetx2 { get; set; }
    }


    public List<string> SearchPaths = new List<string>() { "%FileName%_Textures",string.Empty };

    private FileInfo _objFileInfo = null;

    string mtlLibPath;

    /// <summary>
    /// 贴图加载函数 Overridable for stream loading purposes.
    /// </summary>
    /// <param name="path">The path supplied by the OBJ file, converted to OS path seperation</param>
    /// <param name="isNormalMap">Whether the loader is requesting we convert this into a normal map</param>
    /// <returns>Texture2D if found, or NULL if missing</returns>
    public virtual Texture2D TextureLoadFunction(string path,bool isNormalMap) {
        //find it
        foreach(var searchPath in SearchPaths) {
            //replace varaibles and combine path
            string processedPath = (_objFileInfo != null) ? searchPath.Replace("%FileName%",Path.GetFileNameWithoutExtension(_objFileInfo.Name))
                                                          : searchPath;
            string filePath = Path.Combine(processedPath,path);

            //return if eists
            if(File.Exists(filePath)) {
                var tex = ImageLoader.LoadTexture(filePath);

                if(isNormalMap)
                    ImageUtils.ConvertToNormalMap(tex);

                return tex;
            }
        }

        //not found
        return null;
    }

    private Texture2D TryLoadTexture(string texturePath,bool normalMap = false) {
        //swap directory seperator char
        texturePath = texturePath.Replace('\\',Path.DirectorySeparatorChar);
        texturePath = texturePath.Replace('/',Path.DirectorySeparatorChar);

        return TextureLoadFunction(texturePath,normalMap);
    }

    public enum RenderingMode {
        Opaque,
        Cutout,
        Fade,
        Transparent,
    }

    /// <summary>
    /// 设定渲染模式
    /// </summary>
    /// <param name="material">材质</param>
    /// <param name="renderingMode">指定渲染模式</param>
    public static void SetMaterialRenderingMode(Material material,RenderingMode renderingMode) {
        switch(renderingMode) {
            case RenderingMode.Opaque:
                material.SetFloat("_Mode",0f);
                material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite",1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                /*
                 mtl.SetFloat("_Mode", 3f);
        mtl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mtl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mtl.SetInt("_ZWrite", 0);
        mtl.DisableKeyword("_ALPHATEST_ON");
        mtl.EnableKeyword("_ALPHABLEND_ON");
        mtl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mtl.renderQueue = 3000;
                 */

                break;
            case RenderingMode.Cutout:
                material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite",1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case RenderingMode.Fade:
                material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite",0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case RenderingMode.Transparent:
                material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite",0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }

    /// <summary>
    /// 加载一个mtl文件
    /// </summary>
    /// <param name="input">mtl文件流</param>
    /// <returns>包含了所有材质的Dictionary</returns>
    public Dictionary<string,Material> Load(Stream input) {
        Dictionary<string,Material> mtlDict = new Dictionary<string,Material>();

        //加载材质库
        LoadRaw(input,ref mtlDict);

        //string mtcPath = mtlLibPath.Substring(0,mtlLibPath.IndexOf('.')) + ".mtc";
        Configuration myConfig = Configuration.LoadFromFile(mtlLibPath.Substring(0,mtlLibPath.IndexOf('.')) + ".mtc");
        byte matNum = myConfig["Materials"]["count"].ByteValue;

        for(byte i = 0;i < matNum;i++) {
            string matPath = myConfig["Materials"]["mat_" + i].StringValue;
            string matName;

            Configuration matConfig;
            if(matPath.Substring(0,1) == "%") {
                //本文件内
                matName = matPath.Substring(2,matPath.Length - 2);
                matConfig = myConfig;

                if(!mtlDict.ContainsKey(matName) && !matConfig.Contains(matName)) continue;

            } else {
                //全路径
                //model_3=/vehicle/xmq/aa.mtc:11_-_Default
                matName = matPath.Substring(matPath.IndexOf(':'),matPath.Length - matPath.IndexOf(':'));
                matConfig = Configuration.LoadFromFile(matPath.Substring(0,matPath.IndexOf(':') - 1));

                if(!mtlDict.ContainsKey(matName) && !matConfig.Contains(matName)) continue;
            }

            MaterialConfig materialConfig = new MaterialConfig();
            materialConfig = matConfig[matName].ToObject<MaterialConfig>();

            //设置RenderType
            if(!string.IsNullOrEmpty(materialConfig.renderingMode)) {
                switch(materialConfig.renderingMode) {
                    case "Opaque":
                        SetMaterialRenderingMode(mtlDict[matName],RenderingMode.Opaque);
                        //Debug.Log(matName + "的RenderType设为Opaque");
                        break;
                    case "Cutout":
                        SetMaterialRenderingMode(mtlDict[matName],RenderingMode.Cutout);
                        break;
                    case "Fade":
                        SetMaterialRenderingMode(mtlDict[matName],RenderingMode.Fade);
                        break;
                    case "Transparent":
                        SetMaterialRenderingMode(mtlDict[matName],RenderingMode.Transparent);
                        break;
                    default:
                        Debug.LogError("材质错误");
                        break;
                }
            }

            //设置Albedo块
            if(materialConfig.albedoColor != null) {
                if(materialConfig.albedoColor.Length == 3) {
                    mtlDict[matName].SetColor("_Color",new Color(materialConfig.albedoColor[0],materialConfig.albedoColor[1],materialConfig.albedoColor[2]));
                } else if(materialConfig.albedoColor.Length == 4)
                    mtlDict[matName].SetColor("_Color",new Color(materialConfig.albedoColor[0],materialConfig.albedoColor[1],materialConfig.albedoColor[2],materialConfig.albedoColor[3]));
                if(!string.IsNullOrEmpty(materialConfig.albedoMap)) {
                    mtlDict[matName].SetTexture("_MainTex",TryLoadTexture(materialConfig.albedoMap));
                }

            }

            //设置Metallic
            if(matConfig[matName].Contains("metallicValue")) {
                mtlDict[matName].SetFloat("_Metallic",materialConfig.metallicValue);
            }
            if(!string.IsNullOrEmpty(materialConfig.metallicMap)) {
                mtlDict[matName].EnableKeyword("_METALLICGLOSSMAP");
                mtlDict[matName].SetTexture("_MetallicGlossMap",TryLoadTexture(materialConfig.metallicMap));
            }

            //设置Smoothness
            if(matConfig[matName].Contains("glossiness")) {
                mtlDict[matName].SetFloat("_Glossiness",materialConfig.glossiness);
            }
            if(!string.IsNullOrEmpty(materialConfig.metallicMap)) {
                if(materialConfig.metallicMap == "Metallic Alpha")
                    mtlDict[matName].DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                else
                    mtlDict[matName].EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            }

            //设置Normal
            if(!string.IsNullOrEmpty(materialConfig.normalMap)) {
                mtlDict[matName].SetTexture("_BumpMap",TryLoadTexture(materialConfig.normalMap,true));

                if(matConfig[matName].Contains("normalValue")) {
                    mtlDict[matName].SetFloat("_BumpScale",materialConfig.normalValue);
                }
                mtlDict[matName].EnableKeyword("_NORMALMAP");
            }

            //设置Height
            if(!string.IsNullOrEmpty(materialConfig.heightMap)) {
                mtlDict[matName].SetTexture("_ParallaxMap",TryLoadTexture(materialConfig.heightMap));

                if(matConfig[matName].Contains("heightValue")) {
                    mtlDict[matName].SetFloat("_Parallax",materialConfig.heightValue);
                }
                mtlDict[matName].EnableKeyword("_PARALLAXMAP");
            }

            //设置Occlusion
            if(!string.IsNullOrEmpty(materialConfig.occlusionMap)) {
                mtlDict[matName].SetTexture("_OcclusionMap",TryLoadTexture(materialConfig.occlusionMap));

                if(matConfig[matName].Contains("occlusionValue")) {
                    mtlDict[matName].SetFloat("_OcclusionStrength",materialConfig.occlusionValue);
                }
            }

            //设置Detail Mask
            if(!string.IsNullOrEmpty(materialConfig.detailMask)) {
                mtlDict[matName].SetTexture("_DetailMask",TryLoadTexture(materialConfig.detailMask));

            }

            //设置Emission
            if(materialConfig.emissionColor != null && materialConfig.emissionColor.Length == 3) {

                if(!string.IsNullOrEmpty(materialConfig.emissionMap)) {
                    mtlDict[matName].SetTexture("_EmissionMap",TryLoadTexture(materialConfig.emissionMap));
                }

                mtlDict[matName].SetColor("_EmissionColor",new Color(materialConfig.emissionColor[0],materialConfig.emissionColor[1],materialConfig.emissionColor[2]));

                mtlDict[matName].EnableKeyword("_EMISSION");
            }

        }

        return mtlDict;
    }


    void LoadRaw(Stream input,ref Dictionary<string,Material> mtlDict) {
        var inputReader = new StreamReader(input);
        var reader = new StringReader(inputReader.ReadToEnd());

        Material currentMaterial = null;

        for(string line = reader.ReadLine();line != null;line = reader.ReadLine()) {
            if(string.IsNullOrWhiteSpace(line))
                continue;

            string processedLine = line.Clean();
            string[] splitLine = processedLine.Split(' ');

            //空白或注释 跳过
            //blank or comment
            if(splitLine.Length < 2 || processedLine[0] == '#')
                continue;

            //遇到新定义材质的语句 新建材质并跳到下一行
            //newmtl
            if(splitLine[0] == "newmtl") {
                string materialName = processedLine.Substring(7);

                //var newMtl = new Material(Shader.Find("Standard (Specular setup)")) { name = materialName };
                var newMtl = new Material(Shader.Find("Standard")) { name = materialName };
                mtlDict[materialName] = newMtl;
                currentMaterial = newMtl;

                continue;
            }

            //下面每条语句都需要一个材质来运行,当前材质为空则跳过
            //anything past here requires a material instance
            if(currentMaterial == null)
                continue;

            //漫反射颜色
            //diffuse color
            if(splitLine[0] == "Kd" || splitLine[0] == "kd") {
                currentMaterial.SetColor("_Color",OBJLoaderHelper.ColorFromStrArray(splitLine));
                continue;
            }

            //漫反射贴图
            //diffuse map
            if(splitLine[0] == "map_Kd" || splitLine[0] == "map_kd") {
                string texturePath = processedLine.Substring(7);

                var KdTexture = TryLoadTexture(texturePath);
                currentMaterial.SetTexture("_MainTex",KdTexture);

                //如果贴图有透明度设置成透明模式 set transparent mode if the texture has transparency
                //******************************************************************************************************************************
                if(KdTexture != null && (KdTexture.format == TextureFormat.DXT5 || KdTexture.format == TextureFormat.ARGB32)) {
                    OBJLoaderHelper.EnableMaterialTransparency(currentMaterial);
                    //Debug.Log(currentMaterial.ToString());
                }

                //如果是DDS则翻转纹理   flip texture if this is a dds
                if(Path.GetExtension(texturePath).ToLower() == ".dds") {
                    currentMaterial.mainTextureScale = new Vector2(1f,-1f);
                }

                continue;
            }

            //
            //凹凸贴图
            if(splitLine[0] == "map_Bump" || splitLine[0] == "map_bump") {
                string texturePath = processedLine.Substring(9);
                var bumpTexture = TryLoadTexture(texturePath,true);

                if(bumpTexture != null) {
                    currentMaterial.SetTexture("_BumpMap",bumpTexture);
                    currentMaterial.EnableKeyword("_NORMALMAP");
                }

                continue;
            }

            //镜面颜色
            //specular color
            if(splitLine[0] == "Ks" || splitLine[0] == "ks") {
                currentMaterial.SetColor("_SpecColor",OBJLoaderHelper.ColorFromStrArray(splitLine));
                continue;
            }

            //自发光颜色
            //emission color
            if(splitLine[0] == "Ka" || splitLine[0] == "ka") {
                currentMaterial.SetColor("_EmissionColor",OBJLoaderHelper.ColorFromStrArray(splitLine,0.05f));
                currentMaterial.EnableKeyword("_EMISSION");
                continue;
            }

            //自发光贴图
            //emission map
            if(splitLine[0] == "map_Ka" || splitLine[0] == "map_ka") {
                string texturePath = processedLine.Substring(9);
                currentMaterial.SetTexture("_EmissionMap",TryLoadTexture(texturePath));
                continue;
            }

            //透明度
            //alpha
            if(splitLine[0] == "d") {
                float visibility = OBJLoaderHelper.FastFloatParse(splitLine[1]);
                if(visibility < (1f - Mathf.Epsilon)) {
                    Color temp = currentMaterial.color;

                    temp.a = visibility;
                    currentMaterial.SetColor("_Color",temp);

                    OBJLoaderHelper.EnableMaterialTransparency(currentMaterial);
                }
                continue;
            }

            //光泽度
            //glossiness
            if(splitLine[0] == "Ns" || splitLine[0] == "ns") {
                float Ns = OBJLoaderHelper.FastFloatParse(splitLine[1]);
                Ns = (Ns / 1000f);
                currentMaterial.SetFloat("_Glossiness",Ns);
            }
        }

        //return our dict
        //return mtlDict;
    }

    /// <summary>
    /// 加载一个mtl文件
    /// </summary>
    /// <param name="path">mtl文件路径</param>
    /// <returns>Dictionary containing loaded materials</returns>
	public Dictionary<string,Material> Load(string path) {
        mtlLibPath = path;
        _objFileInfo = new FileInfo(path); //get file info
        SearchPaths.Add(_objFileInfo.Directory.FullName); //add root path to search dir

        using(var fs = new FileStream(path,FileMode.Open)) {
            return Load(fs); //actually load
        }
    }
}
