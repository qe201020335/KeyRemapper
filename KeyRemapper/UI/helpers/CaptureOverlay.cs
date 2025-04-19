using System;
using System.Collections.Generic;
using KeyRemapper.Logic;
using UnityEngine;
using UnityEngine.XR;

namespace KeyRemapper.UI.Helpers;

internal static class CaptureOverlay
{
    public static void Show(Action<ButtonBinding> cb)
    {
        var go = new GameObject("KeyCaptureOverlay");
        go.AddComponent<KeyCaptureRunner>().Init(cb);
    }

    private class KeyCaptureRunner : MonoBehaviour
    {
        private Action<ButtonBinding> _callback;
        private readonly List<InputDevice> _devs = new();

        internal void Init(Action<ButtonBinding> cb) => _callback = cb;

        private void Update()
        {
            InputDevices.GetDevices(_devs);
            foreach (var dev in _devs)
            {
                if (dev.TryGetFeatureValue(CommonUsages.primaryButton, out var p) && p)
                {
                    _callback(new ButtonBinding(dev.characteristics, CommonUsages.primaryButton));
                    Destroy(gameObject);
                    return;
                }
                if (dev.TryGetFeatureValue(CommonUsages.secondaryButton, out var s) && s)
                {
                    _callback(new ButtonBinding(dev.characteristics, CommonUsages.secondaryButton));
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}