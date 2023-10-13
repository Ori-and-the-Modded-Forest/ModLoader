using OriMod;
using OriMod.Core;
using OriMod.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Todo: Create a mod options menu in game.
/// Todo: Make sure the mod is working again.
/// Todo: Organize code to be neater.
/// Todo: Enable modded modules to exist.
/// </summary>

public class patch_LoadingBootstrap : LoadingBootstrap {

	extern void orig_Awake();

	public void Awake() {
		orig_Awake();


		try {
			TempLogger.Init();

			ModCore.OnGameStartup();
		}
		catch (Exception e) {
			TempLogger.Log($"Loading failed, exiting program. {e}");

			Application.Quit();
		}


	}


}

