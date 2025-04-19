using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;


namespace KeyRemapper
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            Log.Info("KeyRemapper initialized.");

            zenjector.Install<KeyRemapperInstaller>(Location.GameCore);
        }

        // 下面 OnStart / OnExit 可以留空或保留日志
        [OnStart] public void OnApplicationStart() { }
        [OnExit]  public void OnApplicationQuit()  { }
    }
}
