using OriMod.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace OriMod.Core {
	public static class ModCore {

		static Dictionary<string, ModdingModule> Mods = new Dictionary<string, ModdingModule>();

		private class ModMetaData {
			public string Name { get; set; }
			public System.Version Version { get; set; }
			public string Code { get; set; }
		}
		class VersionConvert : IYamlTypeConverter {
			public bool Accepts(Type type) {
				return type == typeof(System.Version);
			}

			public object ReadYaml(YamlDotNet.Core.IParser parser, Type type) {

				var parsed = parser.Consume<Scalar>().Value;
				var str = parsed.Split('.');

				int major = 0, minor = 0, build = 0, revision = 0;

				for (int i = 0; i < str.Length; i++) {
					switch (i) {
						case 0:
							major = int.Parse(str[i]);
							break;
						case 1:
							minor = int.Parse(str[i]);
							break;
						case 2:
							build = int.Parse(str[i]);
							break;
						case 3:
							revision = int.Parse(str[i]);
							break;
					}
				}

				return new System.Version(major, minor, build, revision);
			}

			public void WriteYaml(YamlDotNet.Core.IEmitter emitter, object value, Type type) {
				System.Version v = (System.Version)value;

				

				emitter.Emit(new Scalar($"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}"));
			}
		}

		public static bool CoreLoaded { get; private set; }

		internal static void OnGameStartup() {
			CoreLoaded = false;
			TempLogger.Log("Loading Start");

			var th = new Thread(LoadCoreThread);
			th.Priority = ThreadPriority.AboveNormal;
			th.Start();

		}
		private static void LoadCoreThread() {
			try {
				TempLogger.Log("Loading");

				AssetManager.Initialize();

				var ser = new SerializerBuilder().WithTypeConverter(new VersionConvert()).Build();
				var des = new DeserializerBuilder().WithTypeConverter(new VersionConvert()).Build();

				List<ModdingModule> modules = new List<ModdingModule>();

				foreach (var dir in Directory.GetDirectories(AssetManager.ModdedContent)) {

					string metaPath = Path.Combine(dir, "mod.yaml");

					var meta = des.Deserialize<ModMetaData>(File.ReadAllText(metaPath));

					if (File.Exists(Path.Combine(dir, meta.Code))) {
						var assembly = Assembly.LoadFrom(Path.Combine(dir, meta.Code));

						foreach (var type in assembly.GetTypes()) {
							if (type.IsSubclassOf(typeof(ModdingModule))) {
								ModdingModule module = assembly.CreateInstance(type.FullName) as ModdingModule;
								modules.Add(module);

								Mods.Add(meta.Name, module);
							}
						}

					}

				}

				foreach (var module in modules) {
					module.OnLoad();
				}

				TempLogger.Log("Done Loading");
				CoreLoaded = true;
			}
			catch (Exception ex) {
				TempLogger.Log(ex);

				UnityEngine.Application.Quit();
			}
		}
	}
}
