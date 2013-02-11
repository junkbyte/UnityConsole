using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void MenuHandler();

public class JBConsole : MonoBehaviour
{
    public const string allChannelsName = " * ";
    public const string defaultChannelName = " - ";
    public const ConsoleLevel defaultConsoleLevel = ConsoleLevel.Debug;
	
	private static JBConsole _instance;
	
	public static JBConsole instance
	{
		get
		{
			if(_instance == null)
			{
				GameObject go = new GameObject("JBConsole");
				_instance = go.AddComponent<JBConsole>();
			}
			return _instance;
		}
	}
	
	public int maxLogs = 500;
    public bool visible = false;
    public int menuItemWidth = 100;
	public KeyCode toggleKey = KeyCode.BackQuote;

    string[] topMenu;
    string[] levels;
    string[] channels;
    string[] customMenus;
    Dictionary<int, MenuHandler> customMenuHandlers;

    ConsoleLevel viewingLevel = ConsoleLevel.Debug;
    string[] viewingChannels;

    int currentTopMenuIndex = -1;
    string[] currentTopMenu;
    string[] currentSubMenu;
    SubMenuHandler subMenuHandler;

    string searchTerm = "";

    List<ConsoleLog> logs = new List<ConsoleLog>();
    List<ConsoleLog> cachedLogs;
	bool atBottom = true;

    Vector2 scrollPosition;

	void Awake ()
	{
		if(_instance == null) _instance = this;
        topMenu = currentTopMenu = Enum.GetNames(typeof(ConsoleMenu));
        levels = Enum.GetNames(typeof(ConsoleLevel));
        channels = new string[] { allChannelsName, defaultChannelName };

        customMenus = new string[0];
        customMenuHandlers = new Dictionary<int, MenuHandler>();
	}
	
	void Update ()
    {
		if(Input.GetKeyDown(toggleKey))
		{
			visible = !visible;
		}
	}
	
    public static void AddMenu(string name, MenuHandler callback)
	{
		instance.AddCustomMenu(name, callback);
	}
	
    public static void RemoveMenu(string name)
	{
		instance.RemoveCustomMenu(name);
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

    public void AddCh(ConsoleLevel level, string channel, string message)
    {
        //Debug.Log(level + " " + message);
		int count = logs.Count;
		if(count > 0 && logs[count - 1].content.text == message)
		{
			logs[count - 1].repeats++;
			cachedLogs = null;
			return;
		}
        logs.Add(new ConsoleLog(level, channel, message));
        int index = Array.IndexOf(channels, channel);
        if (index < 0)
        {
            AddToStringArray(ref channels, channel);
            if (currentTopMenuIndex == (int)ConsoleMenu.Channels)
            {
                UpdateChannelsSubMenu();
            }
        }
		if(count >= maxLogs)
		{
			logs.RemoveAt(0);
		}
        cachedLogs = null;
    }
	
    public void AddCustomMenu(string name, MenuHandler callback)
    {
		RemoveMenu(name);
        AddToStringArray(ref customMenus, name);
        customMenuHandlers[customMenus.Length - 1] = callback;
    }

    public void RemoveCustomMenu(string name)
    {
		for(int i = customMenus.Length - 1; i >= 0; i--)
		{
			if(customMenus[i] == name)
			{
				customMenus = StringsWithoutIndex(customMenus, i);
				customMenuHandlers.Remove(i);
				return;
			}
		}
    }

    void AddToStringArray(ref string[] strings, string str)
    {
        Array.Resize(ref strings, strings.Length + 1);
        strings[strings.Length - 1] = str;
    }

    string[] StringsWithoutString(string[] strings, string str)
    {
        string[] result;
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

    void OnMenuSelection(int index)
    {
        if (currentTopMenuIndex == index)
        {
            index = -1;
        }
        currentSubMenu = null;
        switch (index)
        {
            case (int)ConsoleMenu.Channels:
                UpdateChannelsSubMenu();
                subMenuHandler = OnChannelClicked;
                break;
            case (int)ConsoleMenu.Levels:
                UpdateLevelsSubMenu();
                subMenuHandler = OnLevelClicked;
                break;
            case (int)ConsoleMenu.Search:
                searchTerm = "";
                break;
            case (int)ConsoleMenu.Menu:
                currentSubMenu = customMenus;
                subMenuHandler = OnCustomMenuClicked;
                break;
            case (int)ConsoleMenu.Hide:
                visible = !visible;
                return;
        }
        currentTopMenuIndex = index;
        currentTopMenu = SelectedStateArrayIndex(topMenu, index, true);
    }

    void OnChannelClicked(int index)
    {
        string channel = channels[index];
        if (channel == allChannelsName)
        {
            viewingChannels = null;
        }
        else if (viewingChannels != null)
        {
            if (Array.IndexOf(viewingChannels, channel) >= 0)
            {
                if (viewingChannels.Length > 1) viewingChannels = StringsWithoutString(viewingChannels, channel);
                else viewingChannels = null;
            }
            else
            {
                AddToStringArray(ref viewingChannels, channel);
            }
        }
        else
        {
            viewingChannels = new string[] { channel };
        }
        UpdateChannelsSubMenu();
        cachedLogs = null;
    }

    void OnLevelClicked(int index)
    {
        viewingLevel = (ConsoleLevel)Enum.GetValues(typeof(ConsoleLevel)).GetValue(index);
        UpdateLevelsSubMenu();
        cachedLogs = null;
    }

    void OnCustomMenuClicked(int index)
    {
        if (customMenuHandlers.ContainsKey(index))
        {
            customMenuHandlers[index]();
        }
    }

    void UpdateChannelsSubMenu()
    {
        currentSubMenu = new string[channels.Length];
        Array.Copy(channels, currentSubMenu, channels.Length);
        if (viewingChannels == null)
        {
            SelectedStateArrayIndex(currentSubMenu, 0, false);
        }
        else
        {
            string channel;
            for (int i = channels.Length - 1; i >= 0; i--)
            {
                channel = channels[i];
                if (Array.IndexOf(viewingChannels, channel) >= 0)
                {
                    SelectedStateArrayIndex(currentSubMenu, i, false);
                }
            }
        }
    }

    void UpdateLevelsSubMenu()
    {
        currentSubMenu = SelectedStateArrayIndex(levels, (int)viewingLevel, true);
    }

    string[] SelectedStateArrayIndex(string[] array, int index, bool copy)
    {
        if (index < 0) return array;
        string[] result;
        if (copy)
        {
            result = new string[array.Length];
            Array.Copy(array, result, array.Length);
        }
        else result = array;
        result[index] = "["+ result[index]+"]";
        return result;
    }
	
	void OnGUI ()
	{
        if (!visible) return;
		/*
        float screenScale = Screen.width / 320f;
	    Matrix4x4 scaledMatrix = Matrix4x4.Scale(Vector3.one * screenScale);
	    GUI.matrix = scaledMatrix;
         */
		DrawGUI();
	}
	
	public void DrawGUI()
	{
        float width = (float) Screen.width;
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        GUILayout.BeginVertical("box", GUILayout.MaxHeight(Screen.height));

        GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
        int selection = GUILayout.Toolbar(-1, currentTopMenu, GUILayout.MinWidth(280), GUILayout.MaxWidth(380));
        if (selection >= 0)
        {
            OnMenuSelection(selection);
        }
		GUILayout.EndHorizontal();

        if (currentSubMenu != null)
        {
            selection = GUILayout.SelectionGrid(-1, currentSubMenu, (int)width / menuItemWidth);
            if (selection >= 0 && subMenuHandler != null)
            {
                subMenuHandler(selection);
            }
        }

        if (currentTopMenuIndex == (int)ConsoleMenu.Search)
        {
            GUI.SetNextControlName("SearchTF");
            string newTerm = GUILayout.TextField(searchTerm);
			if(newTerm != searchTerm)
			{
            	searchTerm = newTerm.ToLower();
				cachedLogs = null;
			}
            
            GUI.FocusControl("SearchTF");
        }
		
		
		
		bool hasCachedLog = cachedLogs != null;
        
		if(atBottom)
		{
			scrollPosition.y = float.MaxValue;
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
			
			if (cachedLogs == null)
	        {
	            CacheBottomOfLogs();
	        }
	        int len = cachedLogs.Count;
	        ConsoleLog log;
	        for (int i = 0; i < len; i++)
	        {
	            log = cachedLogs[i];
	            if(log.repeats > 0)
				{
					GUILayout.Label(log.repeats + "x " +log.content.text, maxwidthscreen);
				}
				else GUILayout.Label(log.content, maxwidthscreen);
	        }
	
	        GUILayout.EndScrollView();
		}
        else
		{
			//todo...
		}

        GUILayout.EndVertical();
	}
	
	void CacheBottomOfLogs()
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		ConsoleLog log;
		float height = (float) Screen.height;
		float width = (float) Screen.width;
		for(int i = logs.Count - 1; i >= 0 && height > 0; i--)
		{
			log = logs[i];
			if (ShouldShow(log))
			{
				cachedLogs.Add(log);
				height -= log.GetHeightForWidth(width);
			}
		}
		cachedLogs.Reverse();
	}
	
	bool ShouldShow(ConsoleLog log)
	{
		return (log.level >= viewingLevel 
			&& (viewingChannels == null || Array.IndexOf(viewingChannels, log.channel) >= 0)
	        && (searchTerm == "" || log.content.text.ToLower().Contains(searchTerm)));
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

delegate void SubMenuHandler(int index);

enum ConsoleMenu
{
    Channels,
    Levels,
    Search,
    Menu,
    Hide
}

class ConsoleLog
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