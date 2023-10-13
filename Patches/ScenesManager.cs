using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoMod;
using System.IO;
using System.Reflection;
using UnityEngine.Assertions;
using OriMod;
using OriMod.Util;
using System.Threading;

public class patch_ScenesManager : ScenesManager {

	private List<SceneMetaData> customLevels;

	private List<patch_SceneManagerScene> scenesToLoad;
	object scenesToLoad_Lock = new object();


	private Thread LoadingThread;

	public static ScenesManager Instance { get; private set; }
	
	public void ReloadCustomLevels() {

		customLevels = new List<SceneMetaData>();

		foreach (var c in customLevels) {
			TempLogger.Log(c.name);

			RuntimeSceneMetaData remove = null;
			foreach (var vanilla in AllScenes) {
				if (vanilla.Scene == c.SceneName) {
					remove = vanilla;
					break;
				}
			}
			if (remove != null)
				AllScenes.Remove(remove);
		}
	}

	extern void orig_ctor();
	[MonoModConstructor]
	void ctor() {
		orig_ctor();
	}

	extern void orig_Awake();

	void Awake() {

		orig_Awake();

		ReloadCustomLevels();
		failedAttempts = new Dictionary<string, int>();

		Instance = this;

		scenesToLoad = new List<patch_SceneManagerScene>();

		LoadingThread = new Thread(LoadLevels);
		LoadingThread.Start();
		LoadingThread.Priority = System.Threading.ThreadPriority.Lowest;
	}

	private void LoadLevels() {
		while (true) {

			patch_SceneManagerScene level = GetCustomLevel();

			if (level != null) {
				LoadingThread.Priority = System.Threading.ThreadPriority.AboveNormal;

				LoadCustomLevel(level);
			}

			LoadingThread.Priority = System.Threading.ThreadPriority.Lowest;

			Thread.Sleep(50);
		}
	}

	patch_SceneManagerScene GetCustomLevel() {
		lock (scenesToLoad_Lock) {
			if (scenesToLoad.Count == 0)
				return null;

			patch_SceneManagerScene retval = scenesToLoad[0];

			return retval;
		}
	}
	private void AddLevelToLoad(patch_SceneManagerScene level) {
		lock (scenesToLoad_Lock) {
			if (!scenesToLoad.Contains(level)) {
				scenesToLoad.Add(level);
			}
		}
	}
	private void SetLevelAsLoaded(patch_SceneManagerScene level) {
		lock (scenesToLoad_Lock) {
			if (scenesToLoad.Contains(level)) {
				scenesToLoad.Remove(level);
			}
		}
	}


	extern void orig_AdditivelyLoadScene(RuntimeSceneMetaData sceneMetaData, bool async, bool keepPreloaded = false);

	private void AdditivelyLoadScene(RuntimeSceneMetaData sceneMetaData, bool async = true, bool keepPreloaded = false) {

		orig_AdditivelyLoadScene(sceneMetaData, async, keepPreloaded);

	}

	extern bool orig_IsInsideASceneBoundary(Vector3 position);
	private bool IsInsideASceneBoundary(Vector3 position) {

		if (orig_IsInsideASceneBoundary(position)) 
			return true;
		foreach (var meta in customLevels) {
			if (!meta.DependantScene) {
				if (meta.IsInsideSceneBounds(position))
					return true;
			}
		}
		return false;
	}


	[MonoModReplace]
	public void AdditivelyLoadScenesAtPosition(Vector3 position, bool async, bool loadingZones = true, bool keepPreloaded = false) {

		AdditivelyLoadScenesInsideRect(new Rect(position.x, position.y, 0, 0), async, loadingZones, keepPreloaded);
	}
	[MonoModReplace]
	public void AdditivelyLoadScenesInsideRect(Rect rect, bool async, bool loadingZones = true, bool keepPreloaded = false) {

		if (Time.timeScale > 2f) {
			async = false;
		}

		Rect currentSceneBounds;
		GetSceneBoundaryAtPosition(rect.center, out currentSceneBounds);

		string currentWorld = (Game.Checkpoint.SaveGameData as patch_SaveGameData).World;

		foreach (patch_RuntimeSceneMetaData scene in AllScenes) {

			//if (scene.WorldOrigin != currentWorld)
			//	continue;

			if (scene.DependantScene || !scene.IsInTotal(rect)) {
				continue;
			}

			if (scene.IsInsideSceneBounds(rect)) {
				if (scene.CanBeLoaded) {
					AdditivelyLoadScene(scene, async, keepPreloaded);
				}
			}
			else if (scene.IsInsideScenePaddingBounds(rect, currentSceneBounds)) {
				if (scene.CanBeLoaded) {
					AdditivelyLoadScene(scene, async, keepPreloaded);
				}
			}
			else if (scene.IsInsideSceneLoadingZone(rect) && scene.CanBeLoaded && loadingZones) {
				AdditivelyLoadScene(scene, true, keepPreloaded);
			}
			
		}

		foreach (patch_SceneMetaData scene in customLevels) {

			if (scene.WorldOrigin != currentWorld)
				continue;

			if (scene.DependantScene || !(scene.IsInsideSceneBounds(rect) || scene.IsInsideScenePaddingBounds(rect) || scene.IsInsideSceneLoadingZone(rect))) {
				continue;
			}

			if (scene.IsInsideSceneBounds(rect)) {
				if (scene.CanBeLoaded) {
					AddivelyLoadCustomLevel(scene, async, keepPreloaded);
				}
			}
			else if (scene.IsInsideScenePaddingBounds(rect, currentSceneBounds)) {
				if (scene.CanBeLoaded) {
					AddivelyLoadCustomLevel(scene, async, keepPreloaded);
				}
			}
			else if (scene.IsInsideSceneLoadingZone(rect) && scene.CanBeLoaded && loadingZones) {
				AddivelyLoadCustomLevel(scene, true, keepPreloaded);
			}
		}
	}
	//*/
	Dictionary<string, int> failedAttempts;

	private void AddivelyLoadCustomLevel(patch_SceneMetaData sceneMetaData, bool async, bool keepPreloaded) {


		foreach (var scene in ActiveScenes) {
			if (scene.SceneRoot != null && scene.SceneRoot.MetaData == sceneMetaData) {
				failedAttempts[sceneMetaData.SceneName] = 0;
				return;
			}
		}

		if (failedAttempts.ContainsKey(sceneMetaData.SceneName) && failedAttempts[sceneMetaData.SceneName] >= 50) {


			if (failedAttempts[sceneMetaData.SceneName] == 50) {
				TempLogger.Log($"Unable to load {sceneMetaData.SceneName}");
			}
			else {
				failedAttempts[sceneMetaData.SceneName]++;
			}
			return;
		}
		GameObject root = null;

		root = new GameObject();
		var sceneRoot = root.AddComponent<SceneRoot>();
		sceneRoot.MetaData = sceneMetaData;
		var cont = AssetManager.GetContent(sceneMetaData.ModPath);

		SceneManagerScene sceneManagerScene = new patch_SceneManagerScene(sceneRoot, new RuntimeSceneMetaData(sceneMetaData), cont);

		Transform rootTransform = root.transform;
		rootTransform.name = sceneMetaData.SceneName;

		rootTransform.position = sceneMetaData.RootPosition;

		var saveSceneManager = root.AddComponent<SaveSceneManager>();
		var sceneSettingsComponent = root.AddComponent<SceneSettingsComponent>();

		sceneManagerScene.PreventUnloading = keepPreloaded;

		if (async) {
			AddLevelToLoad(sceneManagerScene as patch_SceneManagerScene);
		}
		else {
			LoadCustomLevel(sceneManagerScene as patch_SceneManagerScene);
		}

	}

	private void LoadCustomLevel(patch_SceneManagerScene scene) {

		try {
			var sceneRoot = scene.SceneRoot;
			var art = new GameObject();
			art.name = "art";
			art.transform.SetParent(sceneRoot.transform, false);
			art.transform.localPosition = new Vector3(-0.55f, 0.15f, 0);

			var temp = GameObject.CreatePrimitive(PrimitiveType.Plane);

			var mesh = temp.GetComponent<MeshFilter>().mesh;
			var mat = new Material(patch_GameStateMachine.TempShader);
			//mat.mainTexture = patch_LoadingBootstrap.TestRock;

			mat.renderQueue = 9288;
			mat.color = new Color(0.6f, 0.6f, 0.6f, 1);// = 9288;

			var asset = scene.asset;

			using (var sr = new StreamReader(asset.ContentStream)) {
				var line = sr.ReadLine();

				string[] split;

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
						case "art":
							var artChild = new GameObject();
							artChild.name = "newArt";
							artChild.layer = 28;


							var artMesh = artChild.AddComponent<MeshFilter>();
							artMesh.mesh = mesh;

							var renderer = artChild.AddComponent<MeshRenderer>();
							renderer.material = mat;
							renderer.sortingLayerName = "farBackground";
							renderer.useLightProbes = false;
							renderer.sortingOrder = 38;// = false;
							renderer.receiveShadows = false;

							artChild.transform.SetParent(art.transform, false);
							artChild.transform.localPosition = new Vector3(float.Parse(split[0]), float.Parse(split[1]), 0);
							artChild.transform.rotation = Quaternion.AngleAxis(float.Parse(split[2]) + 180, Vector3.forward) * Quaternion.AngleAxis(90, Vector3.left);
							artChild.transform.localScale = Vector3.Scale(new Vector3(float.Parse(split[3]), 1, float.Parse(split[4])), new Vector3(1f / 9.9f, 1, 1f / 9.5f));


							break;
						case "rock": {

							var collideChild = new GameObject();
							collideChild.name = "rock";
							collideChild.layer = 10;
							collideChild.transform.SetParent(sceneRoot.transform, false);

							List<Vector3> points = new List<Vector3>();
							List<int> tris = new List<int>();

							while ((line = sr.ReadLine()) != "end") {
								TempLogger.Log(line);

								split = line.Split(',');
								if (split.Length < 2)
									break;
								Vector2 p = new Vector2(float.Parse(split[0]), float.Parse(split[1]));

								points.Add(new Vector3(p.x, p.y, +0.001f));
								points.Add(new Vector3(p.x, p.y, -0.001f));
							}

							for (int i = 0; i < points.Count; i += 2) {
								tris.Add(i + 0);
								tris.Add(i + 1);
								tris.Add((i + 2) % points.Count);
								tris.Add((i + 3) % points.Count);
								tris.Add((i + 2) % points.Count);
								tris.Add(i + 1);
							}

							var collMesh = new Mesh();
							Vector3[] verts = new Vector3[points.Count];
							for (int i = 0; i < points.Count; i++) {
								verts[i] = points[i];
							}
							collMesh.vertices = verts;
							collMesh.triangles = tris.ToArray();
							collMesh.RecalculateNormals();

							collMesh.RecalculateBounds();

							var collide = collideChild.AddComponent<MeshCollider>();
							collide.sharedMesh = collMesh;
							collide.convex = false;




							break;
						}

					}
				}

			}

			ActiveScenes.Add(scene);
			failedAttempts[scene.MetaData.Scene] = 0;

			SetLevelAsLoaded(scene);

			scene.ChangeState(SceneManagerScene.State.Loaded);
			TempLogger.Log($"{scene.MetaData.Scene} finished loading");

		}
		catch (Exception e) {

			Destroy(scene.SceneRoot.gameObject);
			
			if (!failedAttempts.ContainsKey(scene.MetaData.Scene)) {
				failedAttempts[scene.MetaData.Scene] = 1;
			}
			else {
				failedAttempts[scene.MetaData.Scene]++;
			}

			if (failedAttempts[scene.MetaData.Scene] == 1) {
				TempLogger.Log(e);
			}

		}

	}


	float cross(Vector2 veca, Vector2 vecb) {

		return vecb.x * veca.y - vecb.y * veca.x;
	}
	int get(List<int> list, int idx) {
		if (idx < 0)
			return list[idx + list.Count];
		if (idx >= list.Count)
			return list[idx - list.Count];
		return list[idx];
	}
	bool inside(Vector2 p, Vector2 veca, Vector2 vecb, Vector2 vecc) {

		if (cross(vecb - veca, p - veca) < 0 ||
			cross(vecc - vecb, p - vecb) < 0 ||
			cross(veca - vecc, p - vecc) < 0)
			return false;

		return true;
	}

	bool Triangulate(List<Vector2> points) {


		List<int> indices = new List<int>();
		List<int> tris = new List<int>();

		for (int i = 0; i < points.Count; i++)
			indices.Add(i);

		int wait = 0;
		int index = 0;
		while (indices.Count > 3) {
			if (wait >= 100)
				return false;

			int a = indices[index];
			int b = get(indices, index - 1);
			int c = get(indices, index + 1);


			Vector2 va = points[a];
			Vector2 vb = points[b];
			Vector2 vc = points[c];

			float cr = cross(vb - va, vc - va);

			if (cr < 0) {
				index = (index + 1) % indices.Count;
				wait++;

				continue;
			}

			bool isEar = true;

			for (int i = 0; i < points.Count; i++) {
				if (i == a || i == b || i == c)
					continue;

				if (inside(points[i], va, vb, vc)) {
					isEar = false;
					break;
				}
			}

			if (isEar) {
				tris.Add(b);
				tris.Add(a);
				tris.Add(c);

				indices.Remove(a);
				index %= indices.Count;
				wait = 0;
			}
			else {
				index = (index + 1) % indices.Count;
				wait++;

			}
		}
		tris.Add(indices[0]);
		tris.Add(indices[1]);
		tris.Add(indices[2]);

		var collideChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
		collideChild.name = "rock";

		var mf = collideChild.GetComponent<MeshFilter>();
		var mesh = new Mesh();
		Vector3[] verts = new Vector3[points.Count];
		for (int i = 0; i < points.Count; i++) {
			verts[i] = points[i];
		}
		mesh.vertices = verts;
		mesh.triangles = tris.ToArray();

		mf.mesh = mesh;



		return true;
	}
}
