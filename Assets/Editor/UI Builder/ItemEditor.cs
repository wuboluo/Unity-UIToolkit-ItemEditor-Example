using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private const string itemsFolder = "Assets/SO/Items/";

    // 默认Icon
    private Sprite defaultIcon;

    // 视图
    private ListView itemListView;
    private VisualElement itemDetailsView;
    private VisualElement iconPreview;

    // 道具列表
    private List<ItemConfigSO> itemList = new();

    // 道具配置
    private ItemConfigSO itemConfigSO;

    // 列表处道具模板
    private VisualTreeAsset itemTemplate;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // 获取道具模板
        itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemTemplate.uxml");

        // 各个视图变量赋值
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ItemListView");
        itemDetailsView = root.Q<VisualElement>("ItemDetails");

        iconPreview = itemDetailsView.Q<VisualElement>("Icon");

        // 获取默认图标资源
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Icons/Syh.png");

        // 按键点击事件
        root.Q<Button>("ItemAddButton").clicked += OnAddButtonClicked;
        root.Q<Button>("ItemDeleteButton").clicked += OnDeleteButtonClicked;

        // 加载数据
        LoadDataBase();

        // 生成ListView
        GenerateListView();
    }

    [MenuItem("Yang/Item Editor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    private void LoadDataBase()
    {
    }

    private void GenerateListView()
    {
    }

    private void OnAddButtonClicked()
    {
        var newItemSO = CreateInstance<ItemConfigSO>();
        const string path = itemsFolder + "NewAsset.asset";
        AssetDatabase.CreateAsset(newItemSO, path);
        newItemSO.GUID = AssetDatabase.AssetPathToGUID(path);

        itemConfigSO = newItemSO;
        
        Debug.Log(path);
        Debug.Log(newItemSO.GUID);

        // 刷新道具列表
        itemListView.Rebuild();
    }

    private void OnDeleteButtonClicked()
    {
        // 删除 SO
        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(itemConfigSO.GUID));

        // 刷新道具列表
        itemListView.Rebuild();

        // 道具详细面板不可见，因为当前的Item被删除了，没有选中的Item
        itemDetailsView.visible = false;
    }
}