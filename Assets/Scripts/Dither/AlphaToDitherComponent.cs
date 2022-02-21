#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AlphaToDitherComponent : MonoBehaviour
{
    public Texture2D srcTex;
    [Range(0, 9)]
    public int mipmapLevel;
    public ComputeShader alphaToDitherCS;
    private Texture2D destTex;
    private float[] ditherConvolution;
    private float[] ditherConvolution_1;
    private float[] ditherConvolution_2;

    private void OnValidate()
    {
        if (destTex == null) UpdateDither();
        destTex.mipMapBias = mipmapLevel;
    }

    public void UpdateDither()
    {
        InitDitherConvolution();
        destTex = Alpha2Dither(srcTex);
        destTex.mipMapBias = 4;
        transform.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", destTex);
    }

    public void SaveTexture()
    {
        UpdateDither();
        byte[] bytes = destTex.EncodeToPNG();
        FileStream texFile = File.Open("Assets/Scripts/Dither/" + srcTex.name + System.DateTime.Today.Day + ".png", FileMode.OpenOrCreate);
        BinaryWriter texWrite = new BinaryWriter(texFile);
        texWrite.Write(bytes);
        texFile.Close();
    }

    #region 卷积计算
    private void InitDitherConvolution()
    {
        ditherConvolution = new float[16] 
        {15, 07, 13, 05, 
         03, 11, 01, 09, 
         12, 04, 14, 06, 
         00, 08, 02, 10};
        ditherConvolution_1 = new float[25]
        {0, 14, 22, 5, 8,
         18, 9, 1, 19, 13,
         6, 24, 16, 7, 23,
         21, 2, 12, 20, 3,
         10, 15, 4, 11, 17};
        ditherConvolution = new float[64]
        {
            0, 48, 12, 60, 3, 51, 15, 63,
            32, 16, 44, 28, 35, 19, 47, 31,
            8, 56, 4, 52, 11, 59, 7, 55,
            40, 24, 36, 20, 43, 27, 39, 23,
            2, 50, 14, 62, 1, 49, 13, 61,
            34, 18, 46, 30, 33, 17, 45, 29,
            10, 58, 6, 54, 9, 57, 5, 53,
            42, 26, 38, 22, 41, 25, 37, 21
        };
    }

    private Texture2D Alpha2Dither(Texture texture)
    {
        alphaToDitherCS.SetTexture(0, "_InputTex", texture);
        alphaToDitherCS.SetInt("_SqrtLength", (int)Mathf.Sqrt(ditherConvolution.Length));

        ComputeBuffer ditherBuffer = new ComputeBuffer(ditherConvolution.Length, sizeof(float));
        ditherBuffer.SetData(ditherConvolution);
        alphaToDitherCS.SetBuffer(0, "_DitherConvolution", ditherBuffer);

        Color[] destColors = new Color[texture.width * texture.height];
        ComputeBuffer colorsBuffer = new ComputeBuffer(destColors.Length, sizeof(float) * 4);
        colorsBuffer.SetData(destColors);
        alphaToDitherCS.SetBuffer(0, "Result", colorsBuffer);

        alphaToDitherCS.Dispatch(0, Mathf.CeilToInt((float)texture.width / 8), Mathf.CeilToInt((float)texture.height / 8), 1);

        colorsBuffer.GetData(destColors);
        Texture2D destTex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        destTex.SetPixels(destColors);
        destTex.Apply();
        colorsBuffer.Dispose();
        colorsBuffer.Release();
        ditherBuffer.Release();

        return destTex;
    }
    #endregion
}
#endif