using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

internal class patch_SaveGameData : SaveGameData {


	public string World { get; private set; }

	public extern void orig_SaveToWriter(BinaryWriter writer);
	public extern bool orig_LoadFromReader(BinaryReader reader);

	public void SetModDefaults() {
		World = "Nibel";
	}

	public new void SaveToWriter(BinaryWriter writer) {
		orig_SaveToWriter(writer);
	}

	public new bool LoadFromReader(BinaryReader reader) {

		if (!orig_LoadFromReader(reader))
			return false;

		if (reader.ReadString() != "ModdedSaveData") {
			SetModDefaults();
			return true;
		}


		return true;
	}
}

