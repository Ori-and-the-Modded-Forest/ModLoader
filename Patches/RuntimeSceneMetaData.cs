using System;
using System.Collections.Generic;
using System.IO;
using MonoMod;
using UnityEngine;

public class patch_RuntimeSceneMetaData : RuntimeSceneMetaData {
	public string WorldOrigin { get; private set; }
	public bool ModdedContent { get; private set; }

	public string ModPath { get; private set; }

	public patch_RuntimeSceneMetaData(SceneMetaData data) : base(data) { }

	extern void orig_ctor(patch_SceneMetaData sceneMetaData);
	[MonoModConstructor]
	void ctor(patch_SceneMetaData sceneMetaData) {
		orig_ctor(sceneMetaData);

		WorldOrigin = sceneMetaData.WorldOrigin;
		ModdedContent = sceneMetaData.ModdedContent;
		ModPath = sceneMetaData.ModPath;
		
	}
	

}

