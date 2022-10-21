using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private const string itemsFolder = "Assets/SO/Items/";

    // 当前选中的道具
    private ItemConfigSO currentItem;

    // 默认Icon
    private Sprite defaultIcon;

    // 道具列表
    private List<ItemConfigSO> itemList = new();

    // 视图
    private ListView itemListView;
    private VisualElement iconPreview;
    private VisualElement itemDetailsView;

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
        root.Q<Button>("RenameSOAsset").clicked += OnRenameSOAssetButtonClick;
        root.Q<Button>("ExportToCSV").clicked += OnExportCSVButtonClick;

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
        // 获取本地配置好的文件（t:类型）
        var configs = AssetDatabase.FindAssets("t:ItemConfigSO", new[] {itemsFolder});

        if (configs.Length <= 0) return;

        // 把本地配置好的加载并添加到列表中
        itemList = new List<ItemConfigSO>();
        foreach (var c in configs)
        {
            var path = AssetDatabase.GUIDToAssetPath(c);
            var config = AssetDatabase.LoadAssetAtPath(path, typeof(ItemConfigSO)) as ItemConfigSO;

            itemList.Add(config);

            // 标记资源，否则无法保存数据
            EditorUtility.SetDirty(config);
        }

        // 根据ID排序
        itemList.Sort((a, b) => a.itemID > b.itemID ? 1 : -1);
    }

    /// <summary>
    ///     创建编辑器左侧ListView列表
    /// </summary>
    private void GenerateListView()
    {
        itemListView.fixedItemHeight = 50;
        itemListView.itemsSource = itemList;
        itemListView.makeItem = MakeItem;
        itemListView.bindItem = BindItem;

        itemListView.onSelectionChange += OnListSelectionChange;

        // 未选择的时候，右侧详细面板不可见
        itemDetailsView.visible = false;

        // 克隆模板
        VisualElement MakeItem()
        {
            return itemTemplate.CloneTree();
        }

        // 绑定数据
        void BindItem(VisualElement e, int i)
        {
            if (i >= itemList.Count) return;

            // Icon
            if (null != itemList[i].itemIcon)
                e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;

            // Name
            e.Q<Label>("Name").text = itemList[i] == null ? "No Item" : itemList[i].itemName;
        }
    }

    /// <summary>
    ///     当列表中当前选择的项改变时
    /// </summary>
    /// <param name="selectedItem"></param>
    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        var enumerable = selectedItem as object[] ?? selectedItem.ToArray();
        if (enumerable.ToList().Count <= 0) return;

        currentItem = enumerable.First() as ItemConfigSO;
        GetItemDetails();

        // 选择 item时，右侧详细面板显示
        itemDetailsView.visible = true;
    }

    /// <summary>
    ///     添加一项新的道具
    /// </summary>
    private void OnAddButtonClicked()
    {
        var newItemSO = CreateInstance<ItemConfigSO>();
        var path = itemsFolder + itemList.Count + " NewAsset.asset";
        AssetDatabase.CreateAsset(newItemSO, path);
        newItemSO.GUID = AssetDatabase.AssetPathToGUID(path);

        currentItem = newItemSO;

        // 添加到列表中
        itemList.Add(newItemSO);

        // 刷新道具列表
        itemListView.Rebuild();
    }

    /// <summary>
    ///     删除当前选中的道具
    /// </summary>
    private void OnDeleteButtonClicked()
    {
        // 从列表中移除
        itemList.Remove(currentItem);

        // 删除 SO
        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(currentItem.GUID));

        // 刷新道具列表
        itemListView.Rebuild();

        // 道具详细面板不可见，因为当前的Item被删除了，没有选中的Item
        itemDetailsView.visible = false;
    }

    /// <summary>
    ///     将当前道具的名称更新到其SO.Asset上
    /// </summary>
    private void OnRenameSOAssetButtonClick()
    {
        // 更改SO资源名称
        var path = AssetDatabase.GUIDToAssetPath(currentItem.GUID);
        AssetDatabase.RenameAsset(path, currentItem.itemName);
    }

    /// <summary>
    /// 所有配置导出至一个CSV文件（Excel）
    /// </summary>
    private void OnExportCSVButtonClick()
    {
        var csvWriter = new CSVWriter(Application.dataPath + "/ItemConfigs.csv");
        csvWriter.Write(itemList);
        AssetDatabase.Refresh();
    }

    /// <summary>
    ///     数据更改和更新
    /// </summary>
    private void GetItemDetails()
    {
        // 允许保存数据，更改数据，撤销等
        itemDetailsView.MarkDirtyRepaint();

        #region 通用

        // ID
        itemDetailsView.Q<IntegerField>("ItemID").value = currentItem.itemID;
        itemDetailsView.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt => currentItem.itemID = evt.newValue);

        // 名称
        itemDetailsView.Q<TextField>("ItemName").value = currentItem.itemName;
        itemDetailsView.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            currentItem.itemName = evt.newValue;
            itemListView.Rebuild();
        });

        // 图标
        iconPreview.style.backgroundImage = currentItem.itemIcon == null ? defaultIcon.texture : currentItem.itemIcon.texture;
        itemDetailsView.Q<ObjectField>("ItemIcon").value = currentItem.itemIcon;
        itemDetailsView.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            // 如果选择的item没有配置图标，则使用item默认图标
            var newIcon = evt.newValue as Sprite;
            currentItem.itemIcon = newIcon;
            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;

            itemListView.Rebuild();
        });

        // 类型
        itemDetailsView.Q<EnumField>("ItemType").Init(currentItem.itemType);
        itemDetailsView.Q<EnumField>("ItemType").value = currentItem.itemType;
        itemDetailsView.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt => currentItem.itemType = (ItemType) evt.newValue);

        #endregion

        #region 描述

        // 描述
        itemDetailsView.Q<TextField>("Description").value = currentItem.itemDescription;
        itemDetailsView.Q<TextField>("Description").RegisterValueChangedCallback(evt => currentItem.itemDescription = evt.newValue);

        #endregion

        #region 使用规则

        // 可否被拾取
        itemDetailsView.Q<Toggle>("CanPickedUp").value = currentItem.canPickedUp;
        itemDetailsView.Q<Toggle>("CanPickedUp").RegisterValueChangedCallback(evt => currentItem.canPickedUp = evt.newValue);

        // 可否被丢弃
        itemDetailsView.Q<Toggle>("CanDropped").value = currentItem.canDropped;
        itemDetailsView.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt => currentItem.canDropped = evt.newValue);

        // 可否食用
        itemDetailsView.Q<Toggle>("CanAte").value = currentItem.canAte;
        itemDetailsView.Q<Toggle>("CanAte").RegisterValueChangedCallback(evt => currentItem.canAte = evt.newValue);

        #endregion

        #region 交易

        // 价值
        itemDetailsView.Q<IntegerField>("Price").value = currentItem.itemPrice;
        itemDetailsView.Q<IntegerField>("Price").RegisterValueChangedCallback(evt => currentItem.itemPrice = evt.newValue);

        // 出售折损比例
        itemDetailsView.Q<Slider>("SellPercentage").value = currentItem.sellPercentage;
        itemDetailsView.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt => currentItem.sellPercentage = evt.newValue);

        #endregion
    }
}