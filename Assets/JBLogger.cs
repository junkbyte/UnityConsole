using System;
using System.Collections.Generic;
using UnityEngine;

public class JBLogger
{
	public const string allChannelsName = " * ";
    public const string defaultChannelName = " - ";
    public const ConsoleLevel defaultConsoleLevel = ConsoleLevel.Debug;
	public int maxLogs = 500;
	public notifyNewLogChannel newChannelListener;
	public logCacheFilter logCacheFilterDelegate;
	
	List<ConsoleLog> cachedLogs;
	
    string[] levels;
	public string[] Levels {
		get{return levels;}
	}
    string[] channels;  
	public string[] Channels {
		get{return channels;}
	}
	
    List<ConsoleLog> logs = new List<ConsoleLog>();	
    public List<ConsoleLog> Logs {
		get{return logs;}
	}
		
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
        levels = Enum.GetNames(typeof(ConsoleLevel));
        channels = new string[] { allChannelsName, defaultChannelName };
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
	
	public static void WarnCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Warn, channel, message);
    }
	
	public static void InfoCh(string channel, string message)
    {
        instance.AddCh(ConsoleLevel.Info, channel, message);
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
	
	public static void ErrorCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Error, channel, string.Join(" ", messages));
    }
		
	public static void WarnCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Warn, channel, string.Join(" ", messages));
    }
	
	public static void InfoCh(string channel, params string[] messages)
    {
        instance.AddCh(ConsoleLevel.Info, channel, string.Join(" ", messages));
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
			clearCache();
			return;
		}
        logs.Add(new ConsoleLog(level, channel, message));
        int index = Array.IndexOf(channels, channel);
        if (index < 0)
        {
            AddToStringArray(ref channels, channel);
			clearCache();
			if (newChannelListener != null)
				newChannelListener(channel);
        }
		if(count >= maxLogs)
		{
			logs.RemoveAt(0);
			clearCache();
		}
    }
	
	void AddToStringArray(ref string[] strings, string str)
    {
        Array.Resize(ref strings, strings.Length + 1);
        strings[strings.Length - 1] = str;
    }

    string[] StringsWithoutString(string[] strings, string str)
    {
        int index = Array.IndexOf(strings, str);
        return StringsWithoutIndex(strings, index);
    }

    string[] StringsWithoutIndex(string[] strings, int index)
    {
        string[] result;
        if (index >= 0)
        {
            result = new string[strings.Length - 1];
            Array.Copy(strings, 0, result, 0, index);
            Array.Copy(strings, index + 1, result, index, strings.Length - index - 1);
        }
        else
        {
            result = new string[strings.Length];
            Array.Copy(strings, result, strings.Length);
        }
        return result;
    }
	
	public List<ConsoleLog> getCache(float width, float height){
		if (cachedLogs == null)
        {
            CacheBottomOfLogs(width, height);
        }
		return cachedLogs;
	}
	
	public void clearCache(){
		cachedLogs = null;
	}
	
	void CacheBottomOfLogs(float width, float height)
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		ConsoleLog log;
		for(int i = logs.Count - 1; i >= 0 && height > 0; i--)
		{
			log = logs[i];
			if (logCacheFilterDelegate != null && logCacheFilterDelegate(log))
			{
				cachedLogs.Add(log);
				height -= log.GetHeightForWidth(width);
			}
		}
		cachedLogs.Reverse();
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

    public ConsoleLog(ConsoleLevel level_, string channel_, string message)
    {
        level = level_;
        channel = channel_;
        content = new GUIContent(message);
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

public delegate void notifyNewLogChannel(String channel);
public delegate bool logCacheFilter(ConsoleLog log);