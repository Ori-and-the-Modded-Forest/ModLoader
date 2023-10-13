using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriMod.Util {
	public static class TextureLoader {

		static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
		static Dictionary<string, List<string>> tada = new Dictionary<string, List<string>>();

		private static void AddToList(this Dictionary<string, List<string>> dic, string key, string value) {
			if (!dic.ContainsKey(key)) {
				dic.Add(key, new List<string>());
			}
			dic[key].Add(value);
		}	

		public static Texture2D LoadTexture(string path, string group) {

			if (loadedTextures.ContainsKey(path)) {
				tada.AddToList(group, path);

				return loadedTextures[path];
			}

			return null;
		}

		public static void UnloadGroup(string group) {
			List<string> toUnload = tada[group];

			tada.Remove(group);

			foreach (var list in tada.Values) {
				foreach (var item in list) {
					if (toUnload.Contains(item)) {
						toUnload.Remove(item);
					}
					if (toUnload.Count == 0)
						break;
				}
				if (toUnload.Count == 0)
					break;
			}

			foreach (var tex in toUnload) {
				loadedTextures.Remove(tex);
			}
		}
	}
}
