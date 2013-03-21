using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Collections;

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
	
	public static void Log(string message)
    {
        instance.AddCh(ConsoleLevel.Debug, defaultChannelName, message);
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
	
	public static void Log(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Debug, defaultChannelName, GetStringOf(objects));
    }
	
	public static void Debug(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Debug, defaultChannelName, GetStringOf(objects));
    }

	public static void Warn(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Warn, defaultChannelName, GetStringOf(objects));
    }
	
	public static void Info(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Info, defaultChannelName, GetStringOf(objects));
    }

	public static void Error(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Error, defaultChannelName, GetStringOf(objects));
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
	
	public static void DebugCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Debug, channel, GetStringOf(objects));
    }
	
	public static void InfoCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Info, channel, GetStringOf(objects));
    }
		
	public static void WarnCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Warn, channel, GetStringOf(objects));
    }
	
	public static void ErrorCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Error, channel, GetStringOf(objects));
    }
	
	public static void FatalCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Fatal, channel, GetStringOf(objects));
    }
	
	public static string GetStringOf(object[] objects)
	{
		StringBuilder strb = new StringBuilder();
		int end = objects.Length - 1;
		object obj;
		for(int i = 0; i <= end; i++)
		{
			obj = objects.GetValue(i);
			
			if(obj != null)
			{
				Type type = obj.GetType();
				if(type.IsPrimitive)
				{
					obj = "<i><b>"+ obj + "</b></i>";
				}
			}
			
			/*
			// TODO: read object type and print more info about it...
			
			if(type.IsArray)
			{
				obj = "Array " + ((Array) obj).Length;
			}
			else if(obj is IList)
			{
			    obj = "[" + type + " Count: " + ((IList) obj).Count + "]";
			}
			*/
			strb.Append(obj);
			if(i < end) strb.Append(" ");
		}
		return strb.ToString();
	}
	
    public void AddCh(ConsoleLevel level, string channel, object[] objects)
    {
		AddCh(level, channel, GetStringOf(objects));
	}
	
    public void AddCh(ConsoleLevel level, string channel, string message)
    {
		lock(this)
		{
			int count = logs.Count;
			if(count > 0 &&  logs[count-1].message == message)
			{
				logs[count - 1].repeats++;
				Changed();
				return;
			}
			if(channel == null) channel = defaultChannelName;
			StackTrace stackTrace = new StackTrace();

		    string stack = "";
		    int linenum;
		    foreach (StackFrame stackFrame in stackTrace.GetFrames())
		    {
		        linenum = stackFrame.GetFileLineNumber();
                stack += stackFrame.GetFileName() + ": " + stackFrame.GetMethod().ToString() + (linenum > 0 ? " @ " + linenum : "") + "\n";
		    }

            logs.Add(new ConsoleLog(level, channel, message, stack));
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
    public string message;
    public string stack;
	public int repeats;

    public ConsoleLog(ConsoleLevel level_, string channel_, string message, string stack)
    {
        level = level_;
        channel = channel_;
        this.message = message;
        this.stack = stack;
    }
}