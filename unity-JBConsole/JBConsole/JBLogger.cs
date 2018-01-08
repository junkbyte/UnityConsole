using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Collections;

[System.Serializable]
public class JBLogger
{
	public const string allChannelsName = " * ";
    public const string defaultChannelName = " - ";
    public const ConsoleLevel defaultConsoleLevel = ConsoleLevel.Debug;

    int _customMaxLogs;
	public int maxLogs
    {
        get
        {
            if (_customMaxLogs > 0)
            {
                return _customMaxLogs;
            }
            return JBConsole.isEditor == true ? 5000 : 500;
        }
    }

    public bool RecordStackTrace = true;

    readonly List<string> channels = new List<string>(32);

    public event Action ChannelAdded;

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

    protected JBLogger ()
    {
        ResetChannels();
    }

    private void ResetChannels()
    {
        channels.Clear();
        channels.Add(allChannelsName);
        channels.Add(defaultChannelName);
    }

    /// <summary>
    /// Set custom max logs count. If parameter is 0 or less, it will fall back to default.
    /// </summary>
    public static void SetMaxLogCount(int maxCount)
    {
        instance._customMaxLogs = maxCount;
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

    public static void Fatal(string message)
    {
        instance.AddCh(ConsoleLevel.Fatal, defaultChannelName, message);
    }

	public static void Log(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Debug, defaultChannelName, objects);
    }

	public static void Debug(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Debug, defaultChannelName, objects);
    }

	public static void Warn(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Warn, defaultChannelName, objects);
    }

	public static void Info(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Info, defaultChannelName, objects);
    }

	public static void Error(params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Error, defaultChannelName, objects);
	}

	public static void Fatal(params object[] objects)
	{
		instance.AddCh(ConsoleLevel.Fatal, defaultChannelName, objects);
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
        instance.AddCh(ConsoleLevel.Debug, channel, objects);
    }

	public static void InfoCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Info, channel, objects);
    }

	public static void WarnCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Warn, channel, objects);
    }

    public static void ErrorCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Error, channel, objects);
    }

    public static void FatalCh(string channel, params object[] objects)
    {
        instance.AddCh(ConsoleLevel.Fatal, channel, objects);
    }

    public static string GetStringOf(object[] objects, List<WeakReference> references)
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
				if (type.IsPrimitive)
				{
				    obj = "<i><b>" + obj + "</b></i>";
				}
                else if (obj is string)
                {
                    //obj = obj;
                }
				else
                {
                    references.Add(new WeakReference(obj));
				    if (type.IsArray)
                    {
                        obj = "{<color=#ff8800ff>" + type + " Count: " + ((Array)obj).Length + "</color>}";
                    }
                    else if (obj is IList)
                    {
                        obj = "{<color=#ff8800ff>" + type + " Count: " + ((IList)obj).Count + "</color>}";
                    }
                    else
                    {
                        obj = "{<color=#ff8800ff>" + obj + "</color>}";
                    }
				}

			}
			strb.Append(obj);
			if(i < end) strb.Append(" ");
		}
		return strb.ToString();
	}

    private List<WeakReference> emptyRefList = new List<WeakReference>();

    public ConsoleLog AddCh(ConsoleLevel level, string channel, object[] objects)
    {
        var log = new ConsoleLog();
        log.message = GetStringOf(objects, emptyRefList);
        log.level = level;
        log.channel = channel;
        log.Time = DateTime.UtcNow;
        if (emptyRefList.Count > 0)
        {
            log.references = emptyRefList;
            emptyRefList = new List<WeakReference>();
        }
        Add(log);
        return log;
    }

    public ConsoleLog AddCh(ConsoleLevel level, string channel, string message)
    {
        var log = new ConsoleLog();
        log.message = message;
        log.level = level;
        log.channel = channel;
        log.Time = DateTime.UtcNow;
        Add(log);

		return log;
    }

    public void Add(ConsoleLog log)
    {
        lock (this)
        {
            int count = logs.Count;
            if (count > 0 && logs[count - 1].message == log.message)
            {
                logs[count - 1].repeats++;
                Changed();
                return;
            }
            if (log.channel == null) log.channel = defaultChannelName;

            if(RecordStackTrace) log.stackTrace = new StackTrace(3, true);

            logs.Add(log);
            if (!channels.Contains(log.channel))
            {
                channels.Add(log.channel);
                if (ChannelAdded != null)
                {
                    ChannelAdded();
                }
            }
            if (count >= maxLogs)
            {
                logs.RemoveAt(0);
            }
            Changed();
        }
    }

	public void Clear()
	{
		ResetChannels();
		logs.Clear();
		Changed();
	}

	void Changed()
	{
		if(_stateHash < int.MaxValue) _stateHash++;
		else _stateHash = int.MinValue;
	}

    public IEnumerable<IConsoleLog> GetLogs()
    {
        lock(instance)
        {
            foreach(var log in logs)
            {
                yield return log;
            }
        }
    }
}

public class ConsoleLog: IConsoleLog
{
    public DateTime Time;
    public ConsoleLevel level;
    public string channel;
    public string message;
    public StackTrace stackTrace;
    public int repeats;
    public List<WeakReference> references;

    public string GetUnityLimitedMessage()
    {
        var message = this.message;
        const int MAX_LENGTH = System.UInt16.MaxValue / 4 - 1; //From unity source
        if (message.Length > MAX_LENGTH)
        {
            message = message.Substring(0, MAX_LENGTH);
        }
        return message;
    }

    public DateTime GetTime()
    {
        return Time;
    }

    public ConsoleLevel GetLevel()
    {
        return level;
    }

    public string GetChannel()
    {
        return channel;
    }

    public string GetMessage()
    {
        return message;
    }

    public StackTrace GetStackTrace()
    {
        return stackTrace;
    }
}