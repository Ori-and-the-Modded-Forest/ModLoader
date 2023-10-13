using OriMod.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

public class patch_UberShaderPrewarmer : UberShaderPrewarmer {

	static bool modLoaded = false;

	public extern static bool orig_get_IsLoaded();
	public static bool IsLoaded {
		get => orig_get_IsLoaded() && ModCore.CoreLoaded;
	}

}
