using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using KeyRemapper.Configuration;
using KeyRemapper.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static KeyRemapper.Logic.InputMapManager;
using KeyRemapper.UI.Helpers;

namespace KeyRemapper.UI.ViewControllers;

[HotReload(RelativePathToLayout = @"../Views/ModSettings.bsml")]
internal class ModSettingsController : BSMLAutomaticViewController
{
    // -------- Injected runtime singletons --------
    [Inject] private readonly InputMapManager _map;
    [Inject] private readonly PluginConfig _cfg;

    // -------- UI components --------
    [UIComponent("KeyMapTable")] private CustomCellListTableData _table;
    // [UIComponent("DisableToggle")] private Toggle _disToggle;

    // Table data source
    private readonly List<object> _tableData = new();

    #region BSML lifecycle

    [UIAction("#post-parse")]
    private void Setup()
    {
        // _disToggle.onValueChanged.AddListener(OnDisableToggle);
        RefreshTable();
    }

    #endregion

    // ------- Table helpers -------
    private void RefreshTable()
    {
        _tableData.Clear();
        foreach (RemapAction act in System.Enum.GetValues(typeof(RemapAction)))
        {
            foreach (var bind in _map.GetBindings(act))
                _tableData.Add(new BindingRow(act, bind));
            if (_map.GetBindings(act).Count == 0) // 没有绑定也显示一行灰显
                _tableData.Add(new BindingRow(act, null));
        }

        _table.TableView.ReloadData();
        _table.TableView.RefreshCellsContent();
    }

    // -------- UI actions --------
    [UIAction("cell-reuse")]
    private void CellReuse(TableView table, int idx, Transform cell)
    {
        var ctrl = cell.GetComponent<KeyEntryCellController>();
        ctrl.Init((BindingRow)_tableData[idx], this);
    }

    [UIAction("cell-size")]
    private float CellSize(TableView _, int __) => 8f;

    [UIAction("table-view-did-select-cell-with-idx")]
    private void OnCellSelect(TableView _, int __)
    {
        /* no-op */
    }

    [UIAction("AddBtn")]
    private void OnAddBinding()
    {
        // 简版：总是向 Pause 动作添加新绑定
        // 实际使用可弹窗让玩家选择动作，然后捕获按键
        CaptureOverlay.Show(binding =>
        {
            _map.AddBinding(RemapAction.Pause, binding);
            RefreshTable();
        });
    }

    [UIAction("ResetBtn")]
    private void OnReset()
    {
        _cfg.Bindings.Clear();
        _cfg.Changed();
        RefreshTable();
    }

    [UIAction("OnDisableToggle")]
    private void OnDisableToggle(bool value)
    {
        _map.SetDisableBuiltIn(value);
    }

    // 由 KeyEntryCellController 回调
    internal void RemoveBinding(InputMapManager.RemapAction act, ButtonBinding bind)
    {
        _map.RemoveBinding(act, bind);
        RefreshTable();
    }
}

// ---------- Local row model ----------
internal class BindingRow
{
    public InputMapManager.RemapAction Action { get; }
    public ButtonBinding Bind { get; }

    public BindingRow(InputMapManager.RemapAction a, ButtonBinding b)
    {
        Action = a;
        Bind = b;
    }

    public override string ToString() => $"{Action}-{Bind}";
}