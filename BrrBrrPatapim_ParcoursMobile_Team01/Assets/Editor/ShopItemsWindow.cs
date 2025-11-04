#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Script;
using Script.ScriptableObjects.Scripts;
using UnityEditor.IMGUI.Controls;

public class ShopItemsWindow : EditorWindow
{
    private Vector2 _scroll;
    private string _search = string.Empty;
    private readonly List<ShopItemScriptableObject> _items = new();
    private SearchField _searchField;

    [MenuItem("Tools/Shop/Shop Items Window")] 
    public static void Open()
    {
        var wnd = GetWindow<ShopItemsWindow>(false, "Shop Items", true);
        wnd.minSize = new Vector2(500, 300);
        wnd.RefreshList();
        wnd.Show();
    }

    private void OnEnable()
    {
        if (_searchField == null) _searchField = new SearchField();
        RefreshList();
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Refresh List", EditorStyles.toolbarButton))
            {
                RefreshList();
            }

            if (GUILayout.Button("Refresh All Shop Items", EditorStyles.toolbarButton))
            {
                RefreshAllShopItemsInScenes();
            }

            GUILayout.FlexibleSpace();
            if (_searchField == null) _searchField = new SearchField();
            _search = _searchField.OnToolbarGUI(_search, GUILayout.MaxWidth(300));
        }

        EditorGUILayout.Space();

        if (_items.Count == 0)
        {
            EditorGUILayout.HelpBox("No ShopItemScriptableObject assets found.", MessageType.Info);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var so in Filtered(_items))
        {
            if (so == null) continue;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Icon preview
                    EditorGUILayout.ObjectField(so.GetItemIcon(), typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(so.name, EditorStyles.boldLabel);

                        EditorGUI.BeginChangeCheck();
                        Undo.RecordObject(so, "Edit Shop Item");

                        // Editable fields
                        var newIcon = (Sprite)EditorGUILayout.ObjectField("Icon", so.GetItemIcon(), typeof(Sprite), false);
                        var newPrice = EditorGUILayout.IntField("Price", so.GetItemPrice());
                        var newProductId = EditorGUILayout.TextField("Product ID", so.GetProductId());

                        if (EditorGUI.EndChangeCheck())
                        {
                            // There are no setters, so access via serialized object
                            var soSerialized = new SerializedObject(so);
                            soSerialized.FindProperty("itemIcon").objectReferenceValue = newIcon;
                            soSerialized.FindProperty("itemPrice").intValue = newPrice;
                            soSerialized.FindProperty("productId").stringValue = newProductId;
                            soSerialized.ApplyModifiedProperties();
                            EditorUtility.SetDirty(so);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // Read-only ID with copy button
                            EditorGUILayout.SelectableLabel($"ID: {so.GetItemId()}", GUILayout.Height(18));
                            if (GUILayout.Button("Copy ID", GUILayout.Width(80)))
                            {
                                EditorGUIUtility.systemCopyBuffer = so.GetItemId();
                            }
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Ping Asset", GUILayout.Width(100)))
                            {
                                EditorGUIUtility.PingObject(so);
                                Selection.activeObject = so;
                            }

                            if (GUILayout.Button("Refresh Users in Scene", GUILayout.Width(180)))
                            {
                                RefreshShopItemsReferencing(so);
                            }
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private IEnumerable<ShopItemScriptableObject> Filtered(IEnumerable<ShopItemScriptableObject> source)
    {
        if (string.IsNullOrWhiteSpace(_search)) return source;
        var s = _search.Trim();
        return source.Where(i => i != null && (
            i.name.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            (!string.IsNullOrEmpty(i.GetProductId()) && i.GetProductId().IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
        ));
    }

    private void RefreshList()
    {
        _items.Clear();
        var guids = AssetDatabase.FindAssets("t:ShopItemScriptableObject");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ShopItemScriptableObject>(path);
            if (asset != null) _items.Add(asset);
        }
        Repaint();
    }

    private static IEnumerable<ShopItem> FindAllShopItemsInScenes()
    {
        // Include inactive objects
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<ShopItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        return Resources.FindObjectsOfTypeAll<ShopItem>()
            .Where(o => o != null && o.gameObject.scene.IsValid());
#endif
    }

    private static void RefreshAllShopItemsInScenes()
    {
        int count = 0;
        foreach (var si in FindAllShopItemsInScenes())
        {
            try
            {
                si.ChangeValues();
                EditorUtility.SetDirty(si);
                count++;
            }
            catch { /* ignore */ }
        }
        if (count > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
        Debug.Log($"ShopItemsWindow: Refreshed {count} ShopItem component(s) in open scene(s).");
    }

    private static void RefreshShopItemsReferencing(ShopItemScriptableObject so)
    {
        if (so == null) return;
        int count = 0;
        foreach (var si in FindAllShopItemsInScenes())
        {
            // Access serialized field as there is no public getter
            var serialized = new SerializedObject(si);
            var prop = serialized.FindProperty("shopItemData");
            if (prop != null && prop.objectReferenceValue == so)
            {
                try
                {
                    si.ChangeValues();
                    EditorUtility.SetDirty(si);
                    count++;
                }
                catch { /* ignore */ }
            }
        }
        if (count > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
        Debug.Log($"ShopItemsWindow: Refreshed {count} ShopItem user(s) of '{so.name}'.");
    }
}
#endif
