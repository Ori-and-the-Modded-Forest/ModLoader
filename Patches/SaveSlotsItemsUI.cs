using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class patch_SaveSlotsItemsUI : SaveSlotsItemsUI {

	[MonoModReplace]
	public void Awake() {
		for (int i = 0; i < 10 || GameController.Instance.SaveGameController.SaveExists(i - 1); i++) {
			Items.Add(null);
		}
	}

	[MonoModReplace]
	public void Refresh() {
		if (Items.Count == 0)
			return;
		
		for (int i = 0; i < Items.Count; i++) {
			RefreshItem(i);
		}
	}
}

