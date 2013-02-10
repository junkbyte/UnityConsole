using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void MenuHandler();

public class JBConsole : MonoBehaviour
{
    public const string allChannelsName = " * ";
    public const string defaultChannelName = " - ";
    public const ConsoleLevel defaultConsoleLevel = ConsoleLevel.Debug;
	
	public int maxLogs = 500;
    public bool visible = true;
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

    Vector2 scrollPosition;

	void Awake ()
	{
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

    public void Add(params string[] messages)
    {
        AddCh(defaultConsoleLevel, defaultChannelName, string.Join(" ", messages));
    }

    public void Add(ConsoleLevel level, params string[] messages)
    {
        AddCh(level, defaultChannelName, string.Join(" " ,messages));
    }

    public void Add(ConsoleLevel level, string message)
    {
        AddCh(level, defaultChannelName, message);
    }

    public void AddCh(string channel, params string[] messages)
    {
        AddCh(defaultConsoleLevel, channel, string.Join(" ", messages));
    }

    public void AddCh(string channel, string message)
    {
        AddCh(defaultConsoleLevel, channel, message);
    }

    public void AddCh(ConsoleLevel level, string channel, params string[] messages)
    {
        AddCh(level, channel, string.Join(" ", messages));
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

    public void AddMenu(string name, MenuHandler callback)
    {
        AddToStringArray(ref customMenus, name);
        customMenuHandlers[customMenus.Length - 1] = callback;
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
                visible = false;
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
	
	void OnGUI ()
	{
        if (!visible) return;
		/*
        float screenScale = Screen.width / 320f;
	    Matrix4x4 scaledMatrix = Matrix4x4.Scale(Vector3.one * screenScale);
	    GUI.matrix = scaledMatrix;
         */

        float width = (float) Screen.width;
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        GUILayout.BeginVertical("box", GUILayout.Height(Screen.height));

        GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
        int selection = GUILayout.Toolbar(-1, currentTopMenu, GUILayout.MinWidth(280), GUILayout.MaxWidth(380));
        if (selection >= 0)
        {
            OnMenuSelection(selection);
        }
		GUILayout.EndHorizontal();

        // show search text field.
        if (currentTopMenuIndex == (int)ConsoleMenu.Search)
        {
            GUI.SetNextControlName("SearchTF");
            searchTerm = GUILayout.TextField(searchTerm);
            searchTerm = searchTerm.ToLower();
            
            GUI.FocusControl("SearchTF");
        }

        if (currentSubMenu != null)
        {
            selection = GUILayout.SelectionGrid(-1, currentSubMenu, (int)width / menuItemWidth);
            if (selection >= 0 && subMenuHandler != null)
            {
                subMenuHandler(selection);
            }
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);

        if (cachedLogs == null)
        {
            // TODO: if at bottom of page, trim down the log to last lines.
            cachedLogs = logs;
        }
        int len = cachedLogs.Count;
        ConsoleLog log;
        for (int i = 0; i < len; i++)
        {
            log = cachedLogs[i];
            // todo, do the filtering in cachedLogs refresh phase.
            if (log.level >= viewingLevel 
                && (viewingChannels == null || Array.IndexOf(viewingChannels, log.channel) >= 0)
                && (searchTerm == "" || log.content.text.ToLower().Contains(searchTerm)))
            {
                if(log.repeats > 0)
				{
					GUILayout.Label(log.repeats + "x " +log.content.text, maxwidthscreen);
				}
				else GUILayout.Label(log.content, maxwidthscreen);
            }
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
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