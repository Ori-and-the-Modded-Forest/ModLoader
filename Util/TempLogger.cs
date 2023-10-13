using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;

public static class TempLogger {

	static StreamWriter writer;
	static bool pauseFlush = false;

	static Hook onLog;

	public static void Init() {
		if (writer != null)
			return;

		//onLog = new Hook(typeof(UnityEngine.Logger).GetMethod("LogException", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public), (Action<Action<Exception>, Exception>)LogExceptionA);

		writer = new StreamWriter(File.Open("C:/temp/moonLogger.txt", FileMode.Create, FileAccess.Write, FileShare.Read));
		pauseFlush = false;

		Log("Logger initialized");
	}

	static void LogExceptionA(Action<Exception> orig, Exception exception) {
		orig(exception);

		Log(exception);
	}

	public static void PauseFlushing() {
		pauseFlush = true;
	}
	public static void Flush() {
		pauseFlush = false;
		writer.Flush();
	}

	static readonly object LogLock = new object();
	public static void Log(string message) {
		lock (LogLock) {
			writer.WriteLine(message);
			if (!pauseFlush)
				writer.Flush();
		}
	}
	public static void Log(object obj) {
		Log(obj.ToString());
	}
}

