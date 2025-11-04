#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Script.ScriptableObjects.Scripts;
using UnityEditor.IMGUI.Controls;

public class ShopItemsWindow : EditorWindow
{
    private Vector2 _scroll;
    private string _search = string.Empty;
    private readonly List<ShopItemScriptableObject> _items = new();
    private SearchField _searchField;

    // Auto-refresh controls
    private bool _autoRefreshEnabled = true;
    private float _autoRefreshInterval = 0.5f;
    private double _nextAutoAt;

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
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        Undo.undoRedoPerformed += OnUndoRedo;
        _nextAutoAt = EditorApplication.timeSinceStartup + _autoRefreshInterval;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnHierarchyChanged()
    {
        // One-shot refresh shortly after hierarchy changes
        EditorApplication.delayCall += () => RefreshAllShopItemsInScenes(true);
    }

    private void OnUndoRedo()
    {
        RefreshAllShopItemsInScenes(true);
    }

    private void OnEditorUpdate()
    {
        if (!_autoRefreshEnabled) return;
        var now = EditorApplication.timeSinceStartup;
        if (now < _nextAutoAt) return;
        _nextAutoAt = now + Mathf.Max(0.1f, _autoRefreshInterval);
        RefreshAllShopItemsInScenes(true);
        // No Repaint spam; window will repaint on user interaction
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
                RefreshAllShopItemsInScenes(false);
            }

            GUILayout.FlexibleSpace();

            // Auto-refresh toolbar controls
            _autoRefreshEnabled = GUILayout.Toggle(_autoRefreshEnabled, new GUIContent("Auto Refresh"), EditorStyles.toolbarButton, GUILayout.Width(100));
            EditorGUIUtility.labelWidth = 60f;
            using (new EditorGUILayout.HorizontalScope())
            {
                _autoRefreshInterval = Mathf.Clamp(EditorGUILayout.FloatField("Every", _autoRefreshInterval, GUILayout.Width(140)), 0.1f, 10f);
                GUILayout.Label("s", GUILayout.Width(12));
            }

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
                    // Icon preview & drag target
                    var icon = so.GetItemIcon();
                    var newIcon = (Sprite)EditorGUILayout.ObjectField(icon, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(so.name, EditorStyles.boldLabel);

                        EditorGUI.BeginChangeCheck();

                        // Editable fields via setters so change events fire
                        var iconField = (Sprite)EditorGUILayout.ObjectField("Icon", so.GetItemIcon(), typeof(Sprite), false);
                        var priceField = EditorGUILayout.IntField("Price", so.GetItemPrice());
                        var productField = EditorGUILayout.TextField("Product ID", so.GetProductId());

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(so, "Edit Shop Item");
                            if (iconField != so.GetItemIcon()) so.SetItemIcon(iconField);
                            if (priceField != so.GetItemPrice()) so.SetItemPrice(priceField);
                            if (productField != so.GetProductId()) so.SetProductId(productField);
                            EditorUtility.SetDirty(so);
                            RefreshShopItemsReferencing(so);
                        }

                        // Also handle thumbnail change
                        if (newIcon != icon)
                        {
                            Undo.RecordObject(so, "Change Item Icon");
                            so.SetItemIcon(newIcon);
                            EditorUtility.SetDirty(so);
                            RefreshShopItemsReferencing(so);
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

    private static void RefreshAllShopItemsInScenes(bool silent)
    {
        int count = 0;
        foreach (var si in FindAllShopItemsInScenes())
        {
            try
            {
                si.ChangeValues();
#if UNITY_EDITOR
                EditorUtility.SetDirty(si);
#endif
                count++;
            }
            catch { /* ignore */ }
        }
        if (!silent && count > 0)
        {
#if UNITY_EDITOR
            EditorSceneManager.MarkAllScenesDirty();
#endif
            Debug.Log($"ShopItemsWindow: Refreshed {count} ShopItem component(s) in open scene(s).");
        }
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
#if UNITY_EDITOR
                    EditorUtility.SetDirty(si);
#endif
                    count++;
                }
                catch { /* ignore */ }
            }
        }
#if UNITY_EDITOR
        if (count > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
#endif
        if (count > 0)
            Debug.Log($"ShopItemsWindow: Refreshed {count} ShopItem user(s) of '{so.name}'.");
    }
}
#endif
