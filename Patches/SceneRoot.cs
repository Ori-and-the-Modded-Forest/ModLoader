using Core;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class patch_SceneRoot : SceneRoot {

	public new void LateStart() {

		Events.Scheduler.OnSceneStartLateBeforeSerialize.Call(this);
		if (this.SaveSceneManager) {
			if ((Game.Checkpoint.SaveGameData as patch_SaveGameData).PendingScenes.ContainsKey(("Nibel", this.MetaData.SceneMoonGuid))) {
				this.SaveSceneManager.Load((Game.Checkpoint.SaveGameData as patch_SaveGameData).PendingScenes[("Nibel", this.MetaData.SceneMoonGuid)]);
			}
			else if (Game.Checkpoint.SaveGameData.SceneExists(this.MetaData.SceneMoonGuid)) {
				this.SaveSceneManager.Load((Game.Checkpoint.SaveGameData as patch_SaveGameData).InsertScene(this.MetaData.SceneMoonGuid));
			}
			else {
				this.SaveSceneManager.Save(Game.Checkpoint.SaveGameData.InsertScene(this.MetaData.SceneMoonGuid));
			}
		}
		Events.Scheduler.OnSceneStartLateAfterSerialize.Call(this);
		Scenes.Manager.OnSceneStartCompleted(this);
	}
}

