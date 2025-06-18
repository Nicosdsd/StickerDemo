using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class CreatePuzzle : EditorWindow
{
    // Fields for creation
    private GameObject puzzlePrefab;
    private string baseName = "拼图";

    // Fields for arrangement
    private int columns = 5;
    private float horizontalSpacing = 2.0f;
    private float verticalSpacing = 2.0f;

    [MenuItem("Tools/拼图创建")]
    public static void ShowWindow()
    {
        GetWindow<CreatePuzzle>("拼图工具");
    }

    void OnGUI()
    {
        // Creation Part
        GUILayout.Label("根据图片批量创建物体", EditorStyles.boldLabel);

        puzzlePrefab = (GameObject)EditorGUILayout.ObjectField("要使用的预制体 (Prefab)", puzzlePrefab, typeof(GameObject), false);
        baseName = EditorGUILayout.TextField("物体名前缀", baseName);

        if (GUILayout.Button("生成"))
        {
            CreateItems();
        }
        
        EditorGUILayout.HelpBox("请在Project窗口中选中一个或多个图片文件，然后将要使用的预制体和命名前缀填好，最后点击'生成'按钮。", MessageType.Info);

        // Separator
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // Arrangement Part
        GUILayout.Label("调整选中物体位置", EditorStyles.boldLabel);

        columns = EditorGUILayout.IntField("列数", columns);
        if (columns < 1) columns = 1;
        horizontalSpacing = EditorGUILayout.FloatField("横向间隔", horizontalSpacing);
        verticalSpacing = EditorGUILayout.FloatField("纵向间隔", verticalSpacing);

        int selectedCount = Selection.gameObjects.Length;
        int calculatedRows = 0;
        if (selectedCount > 0)
        {
            calculatedRows = Mathf.CeilToInt((float)selectedCount / columns);
        }
        EditorGUILayout.LabelField("排列预览 (行 x 列)", $"{calculatedRows} x {columns}");


        if (GUILayout.Button("调整位置"))
        {
            ArrangeItems();
        }
        
        EditorGUILayout.HelpBox("请在Scene窗口中选中一个或多个物体，然后设置好列数和间隔。最后点击'调整位置'按钮，选中的物体将会被自动排列。", MessageType.Info);
    }

    private void CreateItems()
    {
        if (puzzlePrefab == null)
        {
            EditorUtility.DisplayDialog("错误", "请先指定一个预制体 (Prefab)。", "好的");
            return;
        }

        var selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets)
            .OfType<Texture2D>()
            .ToList();

        if (selectedTextures.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在Project窗口中至少选择一张图片。", "好的");
            return;
        }
        
        var sortedTextures = selectedTextures.OrderBy(t => {
                Match match = Regex.Match(t.name, @"(\d+)$");
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            })
            .ThenBy(t => t.name);

        int creationCount = 0;
        foreach (var texture in sortedTextures)
        {
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(puzzlePrefab);
            if (newObject == null)
            {
                Debug.LogError("实例化预制体失败！请检查预制体是否有效。");
                continue;
            }

            Match match = Regex.Match(texture.name, @"(\d+)$");
            if (match.Success)
            {
                newObject.name = $"{baseName}（{match.Groups[1].Value}）";
            }
            else
            {
                newObject.name = $"{baseName} - {texture.name}";
            }

            PropertyController propController = newObject.GetComponent<PropertyController>();
            if (propController != null)
            {
                propController.BaseMap = texture;
                Debug.Log($"成功为 {newObject.name} 设置了贴图 '{texture.name}'。");
                creationCount++;
            }
            else
            {
                Debug.LogWarning($"预制体 '{puzzlePrefab.name}' 上没有找到 PropertyController 脚本。无法为 {newObject.name} 设置贴图。");
            }
        }
        
        if(creationCount > 0)
        {
            EditorUtility.DisplayDialog("完成", $"成功生成了 {creationCount} 个物体。", "好的");
        }
    }

    private void ArrangeItems()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在Scene窗口中至少选择一个物体。", "好的");
            return;
        }

        if (columns <= 0)
        {
            // This is already handled in OnGUI, but as a safeguard.
            columns = 1;
        }

        var sortedObjects = selectedObjects.OrderBy(go => {
                Match match = Regex.Match(go.name, @"（(\d+)）");
                if (!match.Success)
                {
                    match = Regex.Match(go.name, @"(\d+)");
                }
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            })
            .ThenBy(go => go.name)
            .ToList();

        var transformsToArrange = sortedObjects.Select(go => go.transform).ToArray();
        Undo.RecordObjects(transformsToArrange, "Arrange Puzzle Pieces");

        Vector3 center = Vector3.zero;
        if (sortedObjects.Count > 0)
        {
            foreach (var go in sortedObjects)
            {
                center += go.transform.position;
            }
            center /= sortedObjects.Count;
        }

        int numRows = Mathf.CeilToInt((float)sortedObjects.Count / columns);
        
        float totalWidth = (columns - 1) * horizontalSpacing;
        float totalHeight = (numRows - 1) * verticalSpacing;

        Vector3 startGridPosition = new Vector3(-totalWidth / 2f, totalHeight / 2f, 0);

        for (int i = 0; i < sortedObjects.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 newRelativePos = new Vector3(col * horizontalSpacing, -row * verticalSpacing, 0);
            
            sortedObjects[i].transform.position = center + startGridPosition + newRelativePos;
        }
        
        if(sortedObjects.Count > 0)
        {
            EditorUtility.DisplayDialog("完成", $"成功排列了 {sortedObjects.Count} 个物体。", "好的");
        }
    }
}
