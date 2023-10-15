using MonoMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

internal class patch_SaveGameData : SaveGameData {

	[MonoModReplace]
	public new readonly Dictionary<(string, MoonGuid), SaveScene> Scenes = new Dictionary<(string, MoonGuid), SaveScene>();
	[MonoModReplace]
	public new readonly Dictionary<(string, MoonGuid), SaveScene> PendingScenes = new Dictionary<(string, MoonGuid), SaveScene>();

	public new void SaveToWriter(BinaryWriter writer) {
		CurrentSaveFileVersion = 1;
		writer.Write("SaveGameData");
		writer.Write(1);

		writer.Write(this.Scenes.Count);
		foreach (var pair in this.Scenes) {
			var saveScene = pair.Value;

			writer.Write(saveScene.SceneGUID.ToByteArray());
			writer.Write(saveScene.SaveObjects.Count);
			foreach (SaveObject saveObject in saveScene.SaveObjects) {
				writer.Write(saveObject.Id.ToByteArray());
				saveObject.Data.WriteMemoryStreamToBinaryWriter(writer);
			}
		}
		((IDisposable)writer).Dispose();
	}
}

