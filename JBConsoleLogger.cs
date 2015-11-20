using System;
using System.Text;
using UnityEngine;

public class JBConsoleLogger : Loggable
{
	public Signal<bool> VisibilityChanged = new Signal<bool>();

	public void Init()
	{
		if(!JBConsole.instance)
		{
			JBConsole.Start();
			JBConsole.instance.Visible = false;

			#if UNITY_EDITOR
			JBCToggleOnKey.RegisterToConsole();
			#endif
			JBCVisibleOnPress.RegisterToConsole();
		}
	}

	public void Destroy()
	{
		if(JBConsole.instance)
		{
			JBLogger.instance.Clear();
			GameObject.Destroy(JBConsole.instance.gameObject);
		}
	}
	
	public void Log(params object[] objects)
	{
		Add(ConsoleLevel.Debug, null, objects);
	}
	
	public void Debug(params object[] objects)
	{
		Add(ConsoleLevel.Debug, null, objects);
	}
	
	public void Warn(params object[] objects)
	{
		Add(ConsoleLevel.Warn, null, objects);
	}
	
	public void Info(params object[] objects)
	{
		Add(ConsoleLevel.Info, null, objects);
	}
	
	public void Error(int errorCode, params object[] objects)
	{
		Add(ConsoleLevel.Error, null, objects, errorCode);
	}
	
	public void LogCh(string channel, params object[] objects)
	{
		Add(ConsoleLevel.Debug, channel, objects);
	}
	
	public void DebugCh(string channel, params object[] objects)
	{
		Add(ConsoleLevel.Debug, channel, objects);
	}
	
	public void InfoCh(string channel, params object[] objects)
	{
		Add(ConsoleLevel.Info, channel, objects);
	}
	
	public void WarnCh(string channel, params object[] objects)
	{
		Add(ConsoleLevel.Warn, channel, objects);
	}
	
	public void ErrorCh(string channel, int errorCode, params object[] objects)
	{
		Add(ConsoleLevel.Error, channel, objects, errorCode);
	}
	
	public static bool ShowConsoleOnError = 
#if DEBUG
		true;
#else
		false;
#endif

	void Add(ConsoleLevel level, string channel, object[] objects, int errorCode = 0)
	{
		JBLogger.instance.AddCh(level, channel, objects);
		
		if ((level == ConsoleLevel.Error || level == ConsoleLevel.Fatal))
		{
			PrintStackTrace(channel);

			if (ShowConsoleOnError && JBConsole.instance)
			{
				ShowConsoleOnError = false;
				JBConsole.instance.Visible = true;
			}
		}
	}
	
	public static void PrintStackTrace(string channel)
	{
		JBLogger.instance.AddCh(ConsoleLevel.Debug, channel, "Stack Trace:");
		JBLogger.instance.AddCh(ConsoleLevel.Debug, channel, StackTraceUtility.ExtractStackTrace());
	}
}