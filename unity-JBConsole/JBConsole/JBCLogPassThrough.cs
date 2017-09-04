using UnityEngine;

public class JBCLogPassThrough : MonoBehaviour
{
	const string CHANNEL = "Unity";

	void SetupUnityToJBC()
	{
		Application.logMessageReceived -= HandleUnityLog;
		Application.logMessageReceived += HandleUnityLog;
	}

	void HandleUnityLog(string logString, string stackTrace, LogType type)
	{
		if(type == LogType.Error || type == LogType.Exception)
		{
			var msg = logString + "\n" + type + "\n" + stackTrace;
			JBLogger.ErrorCh(CHANNEL, msg);
		}
		else if (type == LogType.Warning)
		{
			JBLogger.WarnCh(CHANNEL, logString);
		}
		else
		{
			JBLogger.InfoCh(CHANNEL, logString);
		}
	}

	public static JBCLogPassThrough RegisterUnityToJBC()
	{
		if(JBConsole.Exists)	
		{
			var plugin = JBConsole.instance.RegisterPlugin<JBCLogPassThrough>();
			plugin.SetupUnityToJBC();
			return plugin;
		}
		return null;
	}

	/*
	 todo
	public static JBCLogPassThrough RegisterJBCToUnity()
	{
		if(JBConsole.Exists)	
		{
			var plugin = JBConsole.instance.RegisterPlugin<JBCLogPassThrough>();
			plugin.SetupJBCToUnity()
			return plugin;
		}
		return null;
	}*/
}