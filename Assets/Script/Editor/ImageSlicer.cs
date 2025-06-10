using UnityEngine;
using UnityEditor;
using System.IO;

public class ImageSlicer : EditorWindow
{
    private Texture2D sourceTexture;
    private int rows = 4;
    private int columns = 4;
    private float tabSizeRatio = 0.25f;
    private string outputDir = "Assets/SlicedImages";

    [MenuItem("Tools/Image Slicer")]
    public static void ShowWindow()
    {
        GetWindow<ImageSlicer>("Image Slicer");
    }

    void OnGUI()
    {
        GUILayout.Label("图片切割工具", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("源图片", sourceTexture, typeof(Texture2D), false);
        rows = EditorGUILayout.IntField("行数", rows);
        columns = EditorGUILayout.IntField("列数", columns);
        tabSizeRatio = EditorGUILayout.Slider("拼图块尺寸比例", tabSizeRatio, 0.1f, 0.4f);
        outputDir = EditorGUILayout.TextField("输出目录", outputDir);

        if (GUILayout.Button("开始切割"))
        {
            if (sourceTexture != null && rows > 0 && columns > 0)
            {
                SliceImage();
            }
            else
            {
                Debug.LogError("请选择一张源图片，并指定有效的行列数。");
            }
        }
    }

    private void SliceImage()
    {
        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter == null)
        {
            Debug.LogError("无法获取 TextureImporter。请确保源图片是项目中的有效纹理。");
            return;
        }

        bool wasReadable = textureImporter.isReadable;
        if (!wasReadable)
        {
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        int sliceWidth = sourceTexture.width / columns;
        int sliceHeight = sourceTexture.height / rows;

        float baseRadius = Mathf.Min(sliceWidth, sliceHeight) * tabSizeRatio;
        float tabHalfWidth = baseRadius;
        float tabProtrusion = baseRadius * 1.5f;
        int padding = Mathf.CeilToInt(tabProtrusion);

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
            AssetDatabase.Refresh();
        }
        
        Color[] sourcePixels = sourceTexture.GetPixels();
        int sourceWidth = sourceTexture.width;
        int sourceHeight = sourceTexture.height;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                // 1 for out, -1 for in, 0 for flat
                int topTab = 0, bottomTab = 0, leftTab = 0, rightTab = 0;

                int GetHJointShape(int r_val, int c_val) => (r_val + c_val) % 2 == 0 ? 1 : -1;
                int GetVJointShape(int r_val, int c_val) => (r_val + c_val + 1) % 2 == 0 ? 1 : -1;

                if (c < columns - 1) rightTab = GetHJointShape(r, c);
                if (c > 0) leftTab = -GetHJointShape(r, c - 1);
                if (r < rows - 1) bottomTab = GetVJointShape(r, c);
                if (r > 0) topTab = -GetVJointShape(r - 1, c);

                int pieceTexWidth = sliceWidth + 2 * padding;
                int pieceTexHeight = sliceHeight + 2 * padding;
                
                Texture2D slicedTexture = new Texture2D(pieceTexWidth, pieceTexHeight, TextureFormat.RGBA32, false);
                Color[] piecePixels = new Color[pieceTexWidth * pieceTexHeight];

                for (int y = 0; y < pieceTexHeight; y++)
                {
                    for (int x = 0; x < pieceTexWidth; x++)
                    {
                        float rx = x - padding;
                        float ry = y - padding;

                        if (IsInsidePuzzleShape(rx, ry, sliceWidth, sliceHeight, tabHalfWidth, tabProtrusion, topTab, rightTab, bottomTab, leftTab))
                        {
                            int sx = c * sliceWidth + Mathf.RoundToInt(rx);
                            int sy = (rows - 1 - r) * sliceHeight + Mathf.RoundToInt(ry);

                            sx = Mathf.Clamp(sx, 0, sourceWidth - 1);
                            sy = Mathf.Clamp(sy, 0, sourceHeight - 1);

                            piecePixels[y * pieceTexWidth + x] = sourcePixels[sy * sourceWidth + sx];
                        }
                        else
                        {
                            piecePixels[y * pieceTexWidth + x] = Color.clear;
                        }
                    }
                }

                slicedTexture.SetPixels(piecePixels);
                slicedTexture.Apply();

                byte[] bytes = slicedTexture.EncodeToPNG();
                string outputPath = Path.Combine(outputDir, $"{sourceTexture.name}_{r}_{c}.png");
                File.WriteAllBytes(outputPath, bytes);
                
                if (Application.isEditor)
                {
                    DestroyImmediate(slicedTexture);
                }
                else
                {
                    Destroy(slicedTexture);
                }
            }
        }

        if (!wasReadable)
        {
            textureImporter.isReadable = false;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();
        Debug.Log("图片切割成功！");
    }

    private bool IsInsidePuzzleShape(float rx, float ry, int sliceWidth, int sliceHeight, float tabHalfWidth, float tabProtrusion, int topTab, int rightTab, int bottomTab, int leftTab)
    {
        bool in_main_rect = (rx >= 0 && rx < sliceWidth && ry >= 0 && ry < sliceHeight);

        float tabCenterX = sliceWidth / 2f;
        float tabCenterY = sliceHeight / 2f;

        if (topTab == -1 && Mathf.Abs(rx - tabCenterX) < tabHalfWidth) {
            if (Mathf.Pow((rx - tabCenterX) / tabHalfWidth, 2) + Mathf.Pow((ry - (sliceHeight - 1)) / tabProtrusion, 2) <= 1) return false;
        }
        if (bottomTab == -1 && Mathf.Abs(rx - tabCenterX) < tabHalfWidth) {
            if (Mathf.Pow(ry / tabProtrusion, 2) + Mathf.Pow((rx - tabCenterX) / tabHalfWidth, 2) <= 1) return false;
        }
        if (rightTab == -1 && Mathf.Abs(ry - tabCenterY) < tabHalfWidth) {
            if (Mathf.Pow((rx - (sliceWidth - 1)) / tabProtrusion, 2) + Mathf.Pow((ry - tabCenterY) / tabHalfWidth, 2) <= 1) return false;
        }
        if (leftTab == -1 && Mathf.Abs(ry - tabCenterY) < tabHalfWidth) {
            if (Mathf.Pow(rx / tabProtrusion, 2) + Mathf.Pow((ry - tabCenterY) / tabHalfWidth, 2) <= 1) return false;
        }
        
        if (in_main_rect) return true;

        if (topTab == 1 && Mathf.Abs(rx - tabCenterX) < tabHalfWidth) {
            if (Mathf.Pow((rx - tabCenterX) / tabHalfWidth, 2) + Mathf.Pow((ry - (sliceHeight - 1)) / tabProtrusion, 2) <= 1) return true;
        }
        if (bottomTab == 1 && Mathf.Abs(rx - tabCenterX) < tabHalfWidth) {
            if (Mathf.Pow(ry / tabProtrusion, 2) + Mathf.Pow((rx - tabCenterX) / tabHalfWidth, 2) <= 1) return true;
        }
        if (rightTab == 1 && Mathf.Abs(ry - tabCenterY) < tabHalfWidth) {
            if (Mathf.Pow((rx - (sliceWidth - 1)) / tabProtrusion, 2) + Mathf.Pow((ry - tabCenterY) / tabHalfWidth, 2) <= 1) return true;
        }
        if (leftTab == 1 && Mathf.Abs(ry - tabCenterY) < tabHalfWidth) {
            if (Mathf.Pow(rx / tabProtrusion, 2) + Mathf.Pow((ry - tabCenterY) / tabHalfWidth, 2) <= 1) return true;
        }

        return false;
    }
}
