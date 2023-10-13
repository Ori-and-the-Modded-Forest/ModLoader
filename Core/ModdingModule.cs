using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriMod {
	public abstract class ModdingModule {

		public ModdingModule() { 
			
		}

		public abstract void OnLoad();
		public abstract void OnUnload();
	}
}
