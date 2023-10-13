//#define ZIP

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Reflection;

namespace OriMod.Util {

	public class LoadedAsset {
		public enum ContentLocationType {
			Folder,
			ZipFile,
		}

		public Stream ContentStream {
			get {
#if ZIP
			if (zipEntry != null)
				return zipEntry.Open();
			else
#endif
				return new FileStream(LiteralPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
		}
#if ZIP
	private ZipArchiveEntry zipEntry;
#endif
		public readonly string Extention;
		public readonly string LiteralPath;
		public readonly string Path;
		public readonly long LastEdit;
		public readonly ContentLocationType AssetType;

#if ZIP
	internal LoadedAsset(ZipArchiveEntry zip, string exactPath, DateTime lastEdit) {
		zipEntry = zip;

		AssetType = ContentLocationType.ZipFile;
		IsModContent = true;

		LastEdit = lastEdit.Ticks;

		Extention = System.IO.Path.GetExtension(zip.FullName);
		LiteralPath = exactPath;
		Path = zip.FullName;
	}
#endif
		internal LoadedAsset(string exactPath, string contentPath, DateTime lastEdit) {

			LastEdit = lastEdit.Ticks;

			Extention = System.IO.Path.GetExtension(contentPath);
			LiteralPath = exactPath;
			Path = contentPath;

			AssetType = ContentLocationType.Folder;

		}

		public string GetText() {

			string retval;
			using (var reader = new StreamReader(ContentStream)) {
				retval = reader.ReadToEnd();
			}
			return retval;
		}

		public byte[] GetBinary() {

			byte[] retval;
			using (var reader = new BinaryReader(ContentStream)) {
				retval = reader.ReadBytes((int)reader.BaseStream.Length);
			}
			return retval;
		}
	}

	public static class AssetManager {


		//private readonly static IDeserializer yamlParse;
		//private readonly static ISerializer yamlSaver;


		public static string ModdedContent { get; private set; }

		private static Dictionary<string, LoadedAsset> content = new Dictionary<string, LoadedAsset>();
#if ZIP
	private static List<ZipArchive> zipFiles = new List<ZipArchive>();
#endif

		public static bool HasContent(string path) {
			path = path.Replace('/', '\\');
			return content.ContainsKey(path);
		}
		public static LoadedAsset GetContent(string path) {
			path = path.Replace('/', '\\');

			if (content.ContainsKey(path)) {
				return content[path];
			}

			return null;
		}
		public static IEnumerable<LoadedAsset> FindAssetsByRegex(string pattern) {
			foreach (var key in content.Keys) {
				if (Regex.IsMatch(key, pattern)) {
					yield return content[key];
				}
			}
		}
		public static string GetText(string path) {
			return GetContent(path).GetText();
		}
		public static string GetLiteralPath(string assetPath) {

			var asset = GetContent(assetPath);

			if (asset.AssetType != LoadedAsset.ContentLocationType.ZipFile)
				return asset.LiteralPath;
			else
				return null;
		}

		public static IEnumerable<LoadedAsset> GetContentInFolder(string path, string extention = null) {
			path = path.Replace('/', '\\');
			if (!path.EndsWith("\\"))
				path += '\\';

			foreach (var pair in content) {
				if (pair.Key.StartsWith(path) && (extention == null || pair.Value.Extention == extention))
					yield return pair.Value;
			}
		}

		internal static void Initialize() {

			if (ModdedContent != null)
				return;

			string p = Assembly.GetExecutingAssembly().Location;

			while (p.Contains("oriDE_Data")) {
				p = Path.GetDirectoryName(p);
			}
			ModdedContent = Path.Combine(p, "Mods");


			content.Clear();

			void AddOpenContent(string path, string name) {

				if (content.ContainsKey(name))
					return;

				content[name] = new LoadedAsset(path, name, File.GetLastWriteTime(path));
			}

			foreach (var dir in Directory.GetDirectories(ModdedContent)) {

				foreach (var c in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) {


					string ext = Path.GetExtension(c);

					string subpath = c.Substring(dir.Length + 1);

					AddOpenContent(c, subpath);
				}
			}
#if ZIP
		foreach (var zip in Directory.EnumerateFiles(ModdedContent, "*.zip")) {
			var file = ZipFile.Open(zip, ZipArchiveMode.Read);

			zipFiles.Add(file);

			DateTime lastEdit = File.GetLastWriteTime(zip);


			foreach (var entry in file.Entries) {

				if (entry.FullName.StartsWith("Levels") && content.ContainsKey(entry.FullName))
					continue;
				if (entry.FullName.StartsWith("Levels") && entry.FullName.EndsWith(".ldtkl"))
					continue;

				AddZipContent(entry, zip, lastEdit);
			}
		}
#endif


		}


		internal static void Unload() {
#if ZIP

			foreach (var zip in zipFiles) {
				zip.Dispose();
			}
			zipFiles.Clear();
#endif
			content.Clear();
		}

	}

}