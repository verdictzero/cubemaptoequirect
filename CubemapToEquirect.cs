using UnityEngine;
using UnityEditor;
using System.IO;

namespace CubemapConverter
{
    public class CubemapToEquirect : EditorWindow
    {
        private ReflectionProbe reflectionProbe;
        private int outputWidth = 4096;
        private int outputHeight = 2048;
        private string savePath = "Assets/Panoramas/";
        private bool useHDR = true;

        [MenuItem("Tools/Cubemap to Equirect Converter")]
        public static void ShowWindow()
        {
            GetWindow<CubemapToEquirect>("Cubemap to Equirect");
        }

        private void OnGUI()
        {
            GUILayout.Label("Cubemap to Equirectangular Converter", EditorStyles.boldLabel);

            reflectionProbe = (ReflectionProbe)EditorGUILayout.ObjectField(
                "Reflection Probe", reflectionProbe, typeof(ReflectionProbe), true);

            outputWidth = EditorGUILayout.IntField("Output Width", outputWidth);
            outputHeight = EditorGUILayout.IntField("Output Height", outputHeight);
            useHDR = EditorGUILayout.Toggle("Save as HDR", useHDR);

            if (GUILayout.Button("Select Save Path"))
            {
                savePath = EditorUtility.SaveFolderPanel("Save Panorama To", savePath, "");
                if (!string.IsNullOrEmpty(savePath))
                    savePath = "Assets" + savePath.Substring(Application.dataPath.Length);
            }

            GUI.enabled = reflectionProbe != null && !string.IsNullOrEmpty(savePath);
            if (GUILayout.Button("Convert and Save"))
            {
                ConvertToEquirect();
            }
            GUI.enabled = true;
        }

        private void ConvertToEquirect()
        {
            // Get the cubemap from the reflection probe
            Cubemap cubemap = reflectionProbe.bakedTexture as Cubemap;
            if (cubemap == null)
            {
                Debug.LogError("No baked cubemap found in reflection probe!");
                return;
            }

            // Create render texture for equirectangular projection
            RenderTexture equirectRT = new RenderTexture(outputWidth, outputHeight, 0, 
                useHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            equirectRT.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            equirectRT.Create();

            // Convert cubemap to equirectangular
            Material conversionMaterial = new Material(Shader.Find("Hidden/CubemapToEquirectangular"));
            conversionMaterial.SetTexture("_MainTex", cubemap);
            Graphics.Blit(null, equirectRT, conversionMaterial);

            // Read the render texture and save to file
            Texture2D outputTexture = new Texture2D(outputWidth, outputHeight, 
                useHDR ? TextureFormat.RGBAHalf : TextureFormat.RGBA32, false);
            RenderTexture.active = equirectRT;
            outputTexture.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
            outputTexture.Apply();
            RenderTexture.active = null;

            // Save the texture
            string fileName = $"{reflectionProbe.name}_panorama_{System.DateTime.Now:yyyyMMdd_HHmmss}";
            string extension = useHDR ? ".exr" : ".png";
            string fullPath = Path.Combine(savePath, fileName + extension);

            byte[] bytes;
            if (useHDR)
                bytes = outputTexture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
            else
                bytes = outputTexture.EncodeToPNG();

            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.Refresh();

            // Cleanup
            DestroyImmediate(outputTexture);
            equirectRT.Release();
            DestroyImmediate(equirectRT);
            DestroyImmediate(conversionMaterial);

            Debug.Log($"Saved panorama to: {fullPath}");
        }
    }
}