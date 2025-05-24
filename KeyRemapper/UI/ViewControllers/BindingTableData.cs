using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System.Reflection;
using UnityEngine;

namespace KeyRemapper.UI.ViewControllers
{
    internal static class BindingTableData
    {
        private const string ReuseIdentifier = "KeyRemapperBindingCell";
        
        public static BindingTableCell GetCell(TableView tableView)
        {
            var tableCell = tableView.DequeueReusableCellForIdentifier(ReuseIdentifier);
            
            if (tableCell == null)
            {
                Plugin.Log?.Debug("Creating new BindingTableCell");
                
                tableCell = new GameObject("BindingTableCell", typeof(Touchable))
                    .AddComponent<BindingTableCell>();
                tableCell.interactable = true;
                tableCell.reuseIdentifier = ReuseIdentifier;
                
                // 解析BSML
                var parserParams = BSMLParser.Instance.Parse(
                    Utilities.GetResourceContent(
                        Assembly.GetExecutingAssembly(), 
                        "KeyRemapper.UI.Views.bindingRow.bsml"),
                    tableCell.gameObject, 
                    tableCell
                );
                
                Plugin.Log?.Debug($"BSML parsed for BindingTableCell, GameObject: {tableCell.gameObject.name}");
            }
            else
            {
                Plugin.Log?.Debug("Reusing existing BindingTableCell");
            }
            
            return (BindingTableCell)tableCell;
        }
    }
} 