using OriMod;
using OriMod.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal class patch_SceneManagerScene : SceneManagerScene {

	public LoadedAsset asset { get; private set; }

	public patch_SceneManagerScene() : base(default(RuntimeSceneMetaData)) {}
	extern void orig_ctor2(SceneRoot root, RuntimeSceneMetaData metaData);

	[MonoMod.MonoModConstructor]
	public void ctor2(SceneRoot root, RuntimeSceneMetaData metaData) {
		orig_ctor2(root, metaData);
		asset = null;

	}


	[MonoMod.MonoModConstructor]
	public patch_SceneManagerScene(SceneRoot root, RuntimeSceneMetaData metaData, LoadedAsset asset) : base(root, metaData) {
		this.asset = asset;
	}



}

