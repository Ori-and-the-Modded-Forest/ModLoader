using MonoMod;
using OriMod;
using OriMod.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

internal class patch_SceneMetaData : SceneMetaData {
	public string WorldOrigin { get; private set; }
	public bool ModdedContent { get; private set; }

	public string ModPath { get; private set; }

	string sceneName;

	extern string orig_get_SceneName();

	public string SceneName {
		get {
			if (sceneName != null) {
				return sceneName;
			}
			return orig_get_SceneName();
		}
		private set {
			sceneName = value;
		}
	}

	extern void orig_ctor();

	[MonoModConstructor]
	void ctor() {
		orig_ctor();
		WorldOrigin = "Nibel";
	}

	[MonoModConstructor]
	public void ctor2(LoadedAsset asset) {

		ctor();

		WorldOrigin = "Nibel";

		ModPath = asset.Path;
		sceneName = Path.GetFileNameWithoutExtension(ModPath);

		var guidRNG = new System.Random(asset.Path.GetHashCode());

		byte[] guidBytes = new byte[16];
		for (int i = 0; i < 16; i++) {
			guidBytes[i] = (byte)guidRNG.Next(256);
		}
		SceneMoonGuid = new MoonGuid(new Guid(guidBytes));

		DependantScene = false;

		ModdedContent = true;

		using (var sr = new StreamReader(asset.ContentStream)) {
			string line = sr.ReadLine();
			string[] split = line.Split(',');

			RootPosition = new Vector3(float.Parse(split[0]), float.Parse(split[1]), 0);

			while (!sr.EndOfStream) {
				line = sr.ReadLine();
				TempLogger.Log(line);

				if (string.IsNullOrEmpty(line) || !line.Contains(":"))
					continue;
				split = line.Split(':');
				line = split[0];
				if (split.Length > 1 && split[1].Contains(","))
					split = split[1].Split(',');
				else {
					split = new string[1] { split[1] };
				}

				switch (line) {
					case "bounds":
						this.SceneBoundaries.Add(new Rect(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3])));
						break;
				}
			}
		}
	}

	public bool IsInsideScenePaddingBounds(Vector3 position, Rect rect) {

		return IsInsideScenePaddingBounds(new Rect(position.x, position.y, 0f, 0f), rect);
	}
	public bool IsInsideScenePaddingBounds(Rect position, Rect sceneBounds) {

		for (int i = 0; i < ScenePaddingBoundaries.Count; i++) { 
			Rect rect = this.ScenePaddingBoundaries[i];
			float expand = this.ScenePaddingWideScreenExpansion[i];

			if (rect.Overlaps(sceneBounds)) {
				rect = Rect.MinMaxRect(position.xMin - RuntimeSceneMetaData.PaddingWidthExtension * expand, position.yMin, position.xMax + RuntimeSceneMetaData.PaddingWidthExtension * expand, position.yMax);
			}
			if (rect.Overlaps(position)) {
				return true;
			}
		}
		return true;
	}
}
