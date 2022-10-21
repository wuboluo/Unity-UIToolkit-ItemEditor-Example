using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVWriter
{
    private readonly string fileName;

    public CSVWriter(string fileName)
    {
        this.fileName = fileName;
    }

    public void Write(List<ItemConfigSO> configs)
    {
        using TextWriter twHeader = new StreamWriter(fileName, false, Encoding.UTF8);
        twHeader.WriteLine("ID,名称,类型,图标,描述,能否拾取,能否丢弃,能否食用,价值,出售折损");
        twHeader.Close();

        using TextWriter twContent = new StreamWriter(fileName, true, Encoding.UTF8);
        foreach (var c in configs)
        {
            var generalConfig = $"{c.itemID},{c.itemName},{c.itemType},{c.itemIcon.name}";
            var descriptionConfig = c.itemDescription;
            var useConfig = $"{Bool2String(c.canPickedUp)},{Bool2String(c.canDropped)},{Bool2String(c.canAte)}";
            var dealConfig = $"{c.itemPrice},{c.sellPercentage}";

            twContent.WriteLine($"{generalConfig},{descriptionConfig},{useConfig},{dealConfig}");
        }

        twContent.Close();
    }

    private static string Bool2String(bool b)
    {
        return b ? "能" : "不能";
    }
}