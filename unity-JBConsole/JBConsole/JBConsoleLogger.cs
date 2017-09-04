using System;
using System.Text;
using UnityEngine;

public class JBConsoleLogger : Loggable
{
	public Signal<bool> VisibilityChanged = new Signal<bool>();

	public static bool ShowConsoleOnError = false;

	public static bool ShowToastOnError = false;

	public virtual void Init()
	{
		if(!JBConsole.instance)
		{
			JBConsole.Start();
			SetupConsole(JBConsole.instance);
		}
	}

	protected virtual void SetupConsole(JBConsole console)
	{
		console.Visible = false;

		if (JBConsole.isEditor)
			JBCToggleOnKey.RegisterToConsole ();
		
		JBCVisibleOnPress.RegisterToConsole();
	}

	public virtual void Destroy()
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

    protected virtual ConsoleLog Add(ConsoleLevel level, string channel, object[] objects, int errorCode = 0, System.Diagnostics.StackTrace stackTrace = null)
	{
		var log = JBLogger.instance.AddCh(level, channel, objects);
		
		if ((level == ConsoleLevel.Error || level == ConsoleLevel.Fatal))
		{
			PrintStackTrace(channel);

			if (ShowConsoleOnError && JBConsole.instance)
			{
				ShowConsoleOnError = false;
				JBConsole.instance.Visible = true;
			}else if (ShowToastOnError && JBConsole.instance && JBConsole.ToastLog == null)
			{
			    JBConsole.ToastLog = log;
			    JBConsole.ToastExpiry = Time.time + 9.0f;
			}
		}
        return log;
	}
	
	public static void PrintStackTrace(string channel)
	{
		JBLogger.instance.AddCh(ConsoleLevel.Debug, channel, "Stack Trace:");
		JBLogger.instance.AddCh(ConsoleLevel.Debug, channel, StackTraceUtility.ExtractStackTrace());
	}
}