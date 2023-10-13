using Game;
using OriMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class patch_GameStateMachine : GameStateMachine {

	public static Shader TempShader;

	extern void orig_SetToTitleScreen();
	public new void SetToTitleScreen() {
		orig_SetToTitleScreen();

		var found = GameObject.Find("introEstablishingTreesA");

		var temp = new GameObject();

		TempLogger.Log("Loading");

		if (found != null) {

			TempShader = found.GetComponent<MeshRenderer>().material.shader;

		}

	}

	void OnGUI() {

		GUI.Label(new Rect(250, 5, 500, 100), $"Sections Left: {unfoundRoots.Count}\nFound Sprites: {finalSprites.Count}");

		for (int i = 0; i < 75; i++) {
			GUI.Label(new Rect(5, 5 + (i * 16), 500, 100), unfoundRoots[i]);
		}
	}

	List<string> unfoundRoots;
	List<string> foundSprites;
	List<SpriteLayout> finalSprites;

	bool exported = false;

	struct SpriteLayout {
		public string Atlas;
		public string SpriteName;

		public Vector3[] verts;
		public int[] tris;
		public Vector2[] uv;
	}

	public void Update() {


		if (unfoundRoots == null) {
			unfoundRoots = new List<string>();

		}

		if (!exported && (unfoundRoots.Count == 0 || Input.GetKey(KeyCode.Alpha5))) {
			exported = true;


		}

		if (unfoundRoots.Count > 0) {

			foreach (var scene in patch_ScenesManager.Instance.ActiveScenes) {
				if (unfoundRoots.Contains(scene.MetaData.Scene)) {

					var root = scene.SceneRoot;
					if (root != null && scene.IsLoadingComplete) {
						unfoundRoots.Remove(scene.MetaData.Scene);
						var art = root.transform.Find("art");

						foreach (var t in art.GetComponentsInChildren<Transform>()) {
							if (!t.GetComponent<MeshRenderer>())
								continue;

							var rend = t.GetComponent<MeshRenderer>().material;
							string sprite = $"{rend.mainTexture.name}/{t.gameObject.name}";

							if (foundSprites.Contains(sprite)) {
								continue;
							}

							var mesh = t.GetComponent<MeshFilter>().mesh;

							var spL = new SpriteLayout(){
								verts = mesh.vertices,
								tris = mesh.triangles,
								uv = mesh.uv,
								Atlas = rend.mainTexture.name,
								SpriteName = t.gameObject.name,
							};

							finalSprites.Add(spL);
							foundSprites.Add(sprite);
						}
					}
				}
			}
		}
		

		if (CheatsHandler.Instance.DebugEnabled) {
			if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.E)) {

				if (Characters.Sein != null) {

					Vector3 pos =  Characters.Sein.Transform.position;

					TempLogger.Log($"{pos.x}, {pos.y}, {pos.z}");

					var found = GameObject.Find("rockAutoCollider");
					TempLogger.Log(found.layer);
					TempLogger.Log(found.tag);

					float min = float.MaxValue, max = float.MinValue;
					var mesh = found.GetComponent<MeshCollider>().sharedMesh;

					foreach (var p in mesh.normals) {
						min = Math.Min(min, p.z);
						max = Math.Max(max, p.z);
					}

					TempLogger.Log("\nLogging Rock Collider");

					TempLogger.Log($"{min} - {max}");

					TempLogger.Log($"{pos.x}, {pos.y}, {pos.z}");
				}


				////*
				//List<Texture2D> list = new List<Texture2D>();
				//List<int> layers = new List<int>();

				//try {

				//	var curr = OriMod.Core.Scenes.Manager.CurrentScene;
				//	GameObject root = null;
				//	foreach (var m in OriMod.Core.Scenes.Manager.ActiveScenes) {
				//		if (m.MetaData.Scene == curr.Scene) {
				//			root = m.SceneRoot.gameObject;
				//			break;
				//		}
				//	}


				//	IEnumerable<GameObject> getChildren(GameObject parent) {
				//		int count = parent.transform.childCount;

				//		for (int i = 0; i < count; i++) {
				//			var go = parent.transform.GetChild(i).gameObject;

				//			yield return go;

				//			foreach (var c in getAllChildren(go)) {
				//				yield return c;
				//			}
				//		}
				//	}
				//	IEnumerable<GameObject> getAllChildren(GameObject parent) {
				//		int count = parent.transform.childCount;

				//		for (int i = 0; i < count; i++) {
				//			var go = parent.transform.GetChild(i).gameObject;

				//			yield return go;

				//			foreach (var c in getAllChildren(go)) {
				//				yield return c;
				//			}
				//		}
				//	}

				//	foreach (var child in getChildren(root)) {
				//		if (child.name == "art") {
				//			foreach (var art in getAllChildren(child)) {
				//				var m = art.GetComponent<MeshRenderer>();
				//				if (m == null)
				//					continue;

				//				layers.Add(art.layer);

				//			}
				//		}
				//	}

				//	foreach (var a in layers) {
				//		TempLogger.Log(a.ToString());
				//	}
				//}
				//catch (Exception e) {
				//	TempLogger.Log("Error exporting level:\n" + e.ToString());
				//}

				
				//*/
			}
		}
	}
}

