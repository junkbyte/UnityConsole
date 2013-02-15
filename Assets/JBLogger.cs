using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class JBLogger
{
	public const string allChannelsName = " * ";
    public const string defaultChannelName = " - ";
    public const ConsoleLevel defaultConsoleLevel = ConsoleLevel.Debug;
	
	public int maxLogs = 500;
	
    List<string> channels;  
	
    List<ConsoleLog> logs = new List<ConsoleLog>();
	int _stateHash = int.MinValue;
	
	public List<string> Channels { get{return channels;} }
    public List<ConsoleLog> Logs { get{return logs;} }
	public int stateHash { get { return _stateHash;} } // or just use delegate?
		
	private static JBLogger _instance;

	public static JBLogger instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new JBLogger();
			}
			return _instance;
		}
	}
	
	private JBLogger ()
	{
        channels = new List<string>() { allChannelsName, defaultChannelName };
	}
	
	public static void Add(params string[] messages)
    {
        instance.AddCh(defaultConsoleLevel, defaultChannelName, string.Join(" ", messages));
    }

    public static void Add(ConsoleLevel level, params string[] messages)
    {
        instance.AddCh(level, defaultChannelName, string.Join(" " ,messages));
    }

    public static void Add(ConsoleLevel level, string message)
    {
        instance.AddCh(level, defaultChannelName, message);
    }

    public static void AddCh(string channel, params string[] messages)
    {
        instance.AddCh(defaultConsoleLevel, channel, string.Join(" ", messages));
    }

    public static void AddCh(string channel, string message)
    {
        instance.AddCh(defaultConsoleLevel, channel, message);
    }

    public static void AddCh(ConsoleLevel level, string channel, params string[] messages)
    {
        instance.AddCh(level, channel, string.Join(" ", messages));
    }
	
	public static void Debug(string message)
    {
        instance.AddCh(ConsoleLevel.Debug, defaultChannelName, message);
    }

	public static void Warn(string message)
    {
        instance.AddCh(ConsoleLevel.Warn, defaultChannelName, message);
    }
	
	public static void Info(string message)
    {
        instance.AddCh(ConsoleLevel.Info, defaultChannelName, message);
    }

	public static void Error(string message)
    {
        instance.AddCh(ConsoleLevel.Error, defaultChannelName, message);
    }

	
	public static void DebugCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Debug, channel, message);
    }
	
	public static void InfoCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Info, channel, message);
    }
	
	public static void WarnCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Warn, channel, message);
    }
	
	public static void ErrorCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Error, channel, message);
    }
	
	public static void FatalCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Fatal, channel, message);
    }
	
	public static void DebugCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Debug, channel, string.Join(" ", messages));
    }
	
	public static void InfoCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Info, channel, string.Join(" ", messages));
    }
		
	public static void WarnCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Warn, channel, string.Join(" ", messages));
    }
	
	public static void ErrorCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Error, channel, string.Join(" ", messages));
    }
	
	public static void FatalCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Fatal, channel, string.Join(" ", messages));
    }
	
    public void AddCh(ConsoleLevel level, string channel, string message)
    {
		int count = logs.Count;
		if(count > 0 && logs[count-1].content != null && logs[count-1].content.text == message)
		{
			logs[count - 1].repeats++;
			Changed();
			return;
		}
		StackTrace stackTrace = new StackTrace(); 
		StackFrame[] stackFrames = stackTrace.GetFrames();
        logs.Add(new ConsoleLog(level, channel, message, stackFrames));
        if (!channels.Contains(channel))
        {
			channels.Add(channel);
        }
		if(count >= maxLogs) 
		{
			logs.RemoveAt(0);
		}
		Changed();
    }
	
	void Changed()
	{
		if(_stateHash < int.MaxValue) _stateHash++;
		else _stateHash = int.MinValue;
	}
}

public enum ConsoleLevel
{
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}

public class ConsoleLog
{
    public ConsoleLevel level;
    public string channel;
    public GUIContent content;
	public int repeats;
    public float height;

    public ConsoleLog(ConsoleLevel level_, string channel_, string message, StackFrame[] stackFrames)
    {
        level = level_;
        channel = channel_;
		
		string stack = "";
		foreach (StackFrame stackFrame in stackFrames)
		{
			stack += stackFrame.GetFileName() + ": " + stackFrame.GetMethod().ToString() + " @ " +stackFrame.GetFileLineNumber() + "\n";
		}
				
        content = new GUIContent(message, stack);
    }

    public float GetHeightForWidth(float width)
    {
        if (height <= 0)
        {
            height = GUI.skin.label.CalcHeight(content, width);
        }
        return height;
    }
}