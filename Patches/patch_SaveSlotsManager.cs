using MonoMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class patch_SaveSlotsManager : SaveSlotsManager {

	[MonoModReplace]
	public void Awake() {
		SaveSlotsManager.Instance = this;
		for (int i = 0; i < 10 || GameController.Instance.SaveGameController.SaveExists(i - 1); i++) {
			this.SaveSlots.Add(null);
		}
	}

	[MonoModReplace]
	public static void PrepareSlots() {
		SaveSlotsManager.Instance.SaveSlots.Clear();

		int i = 0;

		do {
			if (GameController.Instance.SaveGameController.SaveExists(i)) {
				string saveFilePath = GameController.Instance.SaveGameController.GetSaveFilePath(i, -1);
				using (BinaryReader binaryReader = new BinaryReader(File.Open(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))) {
					SaveSlotInfo saveSlotInfo = new SaveSlotInfo();
					if (saveSlotInfo.LoadFromReader(binaryReader)) {
						if (GameController.Instance.IsTrial && !saveSlotInfo.IsTrialSave) {
							SaveSlotsManager.Instance.SaveSlots.Add(null);
						}
						else {
							SaveSlotsManager.Instance.SaveSlots.Add(saveSlotInfo);
						}
					}
					else {
						SaveSlotsManager.Instance.SaveSlots.Add(null);
					}
				}
			}
			else {
				SaveSlotsManager.Instance.SaveSlots.Add(null);
			}
			i++;

		} while (i < 10 || GameController.Instance.SaveGameController.SaveExists(i - 1));

	}
}

