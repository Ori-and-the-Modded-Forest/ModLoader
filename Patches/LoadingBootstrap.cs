using OriMod;
using OriMod.Core;
using OriMod.Util;
using System;
using System.Collections;
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
/// Todo: Finish the custom level code.
/// Todo: Figure out how to get textures into the game
/// </summary>

public class patch_LoadingBootstrap : LoadingBootstrap {

	public extern IEnumerator orig_Start();

	public new IEnumerator Start() {

		try {
			TempLogger.Init();

			ModCore.OnGameStartup();
		}
		catch (Exception e) {
			TempLogger.Log($"Loading failed, exiting program. {e}");

			Application.Quit();
		}

		while (!ModCore.CoreLoaded)
			yield return new WaitForEndOfFrame();

		var ien = orig_Start();

		while (ien.MoveNext()) {
			yield return ien.Current;
		}
	}


}

