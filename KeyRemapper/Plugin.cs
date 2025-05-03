using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using KeyRemapper.Installers;
using BeatSaberMarkupLanguage.MenuButtons;
using BS_Utils.Utilities;
using HMUI;
using KeyRemapper.Configuration;
using KeyRemapper.UI.ViewControllers;
using Zenject;

namespace KeyRemapper
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        private SettingsFlow _flow;             // 缓存 FlowCoordinator

        [Init]
        public Plugin(IPALogger logger, IPA.Config.Config conf, Zenjector zenjector)
        {
            Log = logger;
            // 生成或加载 cfg → 存到静态 Instance
            PluginConfig.Initialize(conf);
            logger.Debug("Config loaded.");
            zenjector.Install<AppInstaller>(Location.App, PluginConfig.Instance);
            zenjector.Install<GameplayInstaller>(Location.Player);
            // zenjector.Install<MenuBindingsInstaller>(Location.Menu);
            zenjector.UseLogger(logger);
            Log.Info("KeyRemapper initialized.");
        }

        [OnStart]
        public void Enable()
        {
            // 等“进入主菜单 & 第一次加载”事件
            // 先不显示界面
            // BSEvents.menuSceneLoadedFresh += OnMenuReady;
        }

        [OnExit]
        public void Disable()
        {
            // 先不显示界面
            // BSEvents.menuSceneLoadedFresh -= OnMenuReady;
        }


        // ───────────── once-per-run callback ─────────────
        private void OnMenuReady()
        {
            MenuButtons.Instance.RegisterButton(
                new MenuButton("Key Remapper", "Configure controller bindings", ShowSettings));

            // 只需注册一次即可，移除事件避免多次调用
            BSEvents.menuSceneLoadedFresh -= OnMenuReady;
            Log.Info("Key Remapper button registered.");
        }

        private void ShowSettings()
        {
            if (_flow == null)
                _flow = BeatSaberUI.CreateFlowCoordinator<SettingsFlow>();



            // 推栈
            FlowCoordinator parent = UnityEngine.Resources
                .FindObjectsOfTypeAll<MainFlowCoordinator>()[0];
            _flow.Init(parent);
            parent.PresentFlowCoordinator(_flow);
        }
    }

    // ---------- 自定义 FlowCoordinator ----------
    internal class SettingsFlow : FlowCoordinator
    {
        private ModSettingsController _view;
        private FlowCoordinator       _parent;   // 用于返回

        public void Init(FlowCoordinator parent) => _parent = parent;

        protected override void DidActivate(bool first, bool added, bool _)
        {
            if (!first) return;
            // 1. 创建 VC（不带注入）
            // _view = BeatSaberUI.CreateViewController<UI.ViewControllers.ModSettingsController>();
            //
            // // 2. 手动注入：使用 ProjectContext 全局容器
            // // ProjectContext.Instance.Container.Inject(_view);
            //
            // // 3. 推为主视图
            // ProvideInitialViewControllers(_view);

            _view = BeatSaberUI.CreateViewController<ModSettingsController>();
            // ProjectContext.Instance.Container.Inject(_view);

            // 顶栏标题
            SetTitle("Key Remapper", ViewController.AnimationType.None);
            showBackButton = true;

            ProvideInitialViewControllers(_view);
        }
        protected override void BackButtonWasPressed(ViewController _)
            => _parent.DismissFlowCoordinator(this);

    }
}
