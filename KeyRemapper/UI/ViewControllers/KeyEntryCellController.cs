using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using KeyRemapper.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static KeyRemapper.Logic.InputMapManager;

namespace KeyRemapper.UI.ViewControllers;

internal class KeyEntryCellController : BSMLAutomaticViewController
{
    [UIComponent("ActionLabel")]  private TextMeshProUGUI _actionLbl;
    [UIComponent("BindingLabel")] private TextMeshProUGUI _bindLbl;
    [UIComponent("RemoveBtn")]    private Button _removeBtn;

    private ModSettingsController    _parent;
    private InputMapManager.RemapAction              _action;
    private ButtonBinding            _binding;     // 可能为 null (占位行)

    internal void Init(object rowObj, ModSettingsController parent)
    {
        var row = (BindingRow)rowObj;     // 拿到 record
        _action    = row.Action;
        _binding   = row.Bind;
        _parent    = parent;

        _actionLbl.text = _action.ToString();

        if (_binding == null)
        {
            _bindLbl.text = "<i>– none –</i>";
            _removeBtn.interactable = false;
        }
        else
        {
            _bindLbl.text = _binding.ToString();
            _removeBtn.interactable = true;
        }
    }

    [UIAction("RemoveBtn")]
    private void OnRemove()
    {
        if (_binding != null)
            _parent.RemoveBinding(_action, _binding);
    }
}