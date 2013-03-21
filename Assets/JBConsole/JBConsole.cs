using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void JBConsoleMenuHandler();

public class JBConsole : MonoBehaviour
{
    delegate void SubMenuHandler(int index);

    public static JBConsole Start()
	{
		if(instance == null)
		{
			var go = new GameObject("JBConsole");
			instance = go.AddComponent<JBConsole>();
		}
		return instance;
	}

    public static JBConsole instance { get; private set; }

    public static bool exists
	{
		get { return instance != null; }
    }

    public static void AddMenu(string name, JBConsoleMenuHandler callback)
    {
        if (!exists) return;
        instance.AddCustomMenu(name, callback);
    }

    public static void RemoveMenu(string name)
    {
        if (!exists) return;
        instance.RemoveCustomMenu(name);
    }

	
	private JBLogger logger;
	private JBCStyle style;

    public bool visible = true;
    public int menuItemWidth = 100;
    public int BaseDPI = 160;
	
    string[] levels;
    string[] topMenu;
    Dictionary<int, JBConsoleMenuHandler> customMenuHandlers;

    ConsoleLevel viewingLevel = ConsoleLevel.Debug;
    string[] viewingChannels;

    int currentTopMenuIndex = -1;
	string[] customMenus;
    string[] currentTopMenu;
    string[] currentSubMenu;
    SubMenuHandler subMenuHandler;
	
	List<ConsoleLog> cachedLogs;

    string searchTerm = "";

	bool autoScrolling = true;
	
	Rect autoscrolltogglerect = new Rect(0, 0, 110, 22);

    Vector2 scrollPosition;
	
	int stateHash;

    private ConsoleLog focusedLog;

	
	void Awake ()
	{
		if(instance == null) instance = this;

		
		DontDestroyOnLoad(gameObject);
		
		logger = JBLogger.instance;
		
        levels = Enum.GetNames(typeof(ConsoleLevel));
        topMenu = currentTopMenu = Enum.GetNames(typeof(ConsoleMenu));

        customMenus = new string[0];
        customMenuHandlers = new Dictionary<int, JBConsoleMenuHandler>();
	}
	
	void Update ()
    {
		if(autoScrolling && logger.stateHash != stateHash)
		{
			stateHash = logger.stateHash;
			clearCache();
		}
	}

    public void AddCustomMenu(string name, JBConsoleMenuHandler callback)
    {
		RemoveMenu(name);
        JBConsoleUtils.AddToStringArray(ref customMenus, name);
        customMenuHandlers[customMenus.Length - 1] = callback;
    }

    public void RemoveCustomMenu(string name)
    {
		for(int i = customMenus.Length - 1; i >= 0; i--)
		{
			if(customMenus[i] == name)
			{
				customMenus = JBConsoleUtils.StringsWithoutIndex(customMenus, i);
				customMenuHandlers.Remove(i);
				return;
			}
		}
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
		EnsureScrollPosition();
        currentTopMenuIndex = index;
        currentTopMenu = SelectedStateArrayIndex(topMenu, index, true);
    }

    void OnChannelClicked(int index)
    {
        string channel = logger.Channels[index];
        if (channel == JBLogger.allChannelsName)
        {
            viewingChannels = null;
        }
        else if (viewingChannels != null)
        {
            if (Array.IndexOf(viewingChannels, channel) >= 0)
            {
                if (viewingChannels.Length > 1) viewingChannels = JBConsoleUtils.StringsWithoutString(viewingChannels, channel);
                else viewingChannels = null;
            }
            else
            {
                JBConsoleUtils.AddToStringArray(ref viewingChannels, channel);
            }
        }
        else
        {
            viewingChannels = new string[] { channel };
        }
        UpdateChannelsSubMenu();
        clearCache();
    }

    void OnLevelClicked(int index)
    {
        viewingLevel = (ConsoleLevel)Enum.GetValues(typeof(ConsoleLevel)).GetValue(index);
        UpdateLevelsSubMenu();
        clearCache();
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
		var channels = logger.Channels.ToArray();
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

        var depth = GUI.depth;
		GUI.depth = int.MaxValue - 10;
		
		float scale = 1;
        var dpi = Screen.dpi;
        if (dpi > 0 && BaseDPI > 0) scale = dpi / BaseDPI;

        DrawGUI(Screen.width, Screen.height, scale);

        GUI.depth = depth;
	}
	
	public void DrawGUI(float width, float height, float scale = 1)
    {
        if (style == null) style = new JBCStyle();

        GUILayout.BeginVertical(style.BoxStyle);

        int selection = GUILayout.Toolbar(-1, currentTopMenu, style.MenuStyle, GUILayout.MinWidth(280 * scale), GUILayout.MaxWidth(380 * scale));
        if (selection >= 0)
        {
            Defocus();
            OnMenuSelection(selection);
        }

        if (currentSubMenu != null)
        {
			if(currentSubMenu.Length == 0)
			{
				GUILayout.Label("No Custom Menus...");
			}
			else
			{
                selection = GUILayout.SelectionGrid(-1, currentSubMenu, (int)(width / (menuItemWidth * scale)), style.MenuStyle);
	            if (selection >= 0 && subMenuHandler != null)
	            {
	                Defocus();
	                subMenuHandler(selection);
	            }
			}
        }

        if (currentTopMenuIndex == (int)ConsoleMenu.Search)
        {
            GUI.SetNextControlName("SearchTF");
            string newTerm = GUILayout.TextField(searchTerm);
			if(newTerm != searchTerm)
			{
                Defocus();
            	searchTerm = newTerm.ToLower();
				clearCache();
			}
            
            GUI.FocusControl("SearchTF");
        }

        if (focusedLog != null)
        {
            DrawFocusedLog(width, height);
        }
        else
        {
            DrawLogScroll(width, height);
        }

	    GUILayout.EndVertical();

		
		if(GUI.tooltip.Length > 0)
        {
            if (Event.current.type == EventType.MouseUp)
            {

            }
			/*
            GUIContent content = new GUIContent(GUI.tooltip);
			float tooltiph = style.BoxStyle.CalcHeight(content, width);
			GUI.Label(new Rect(0, Screen.height - Input.mousePosition.y + 16, width, tooltiph), GUI.tooltip, style.BoxStyle);
            */
		}
	}

    private void DrawLogScroll(float width, float height)
    {
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        bool wasForcedBottom = scrollPosition.y == float.MaxValue;

        ConsoleLog clickedLog;
        if (autoScrolling)
        {
            if (cachedLogs == null && Event.current.type == EventType.Layout)
            {
                scrollPosition.y = float.MaxValue;
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
            if (cachedLogs == null)
            {
                CacheBottomOfLogs(width, height);
            }
            clickedLog = PrintCachedLogs(maxwidthscreen);
        }
        else
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
            if (cachedLogs == null)
            {
                CacheAllOfLogs();
            }
            clickedLog = PrintCachedLogs(maxwidthscreen);
        }

        if (clickedLog != null)
        {
            focusedLog = clickedLog;
        }

        Rect lastContentRect = GUILayoutUtility.GetLastRect();
        GUILayout.EndScrollView();

        Rect scrollViewRect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint)
        {
            float maxscroll = lastContentRect.y + lastContentRect.height - scrollViewRect.height;

            bool atbottom = maxscroll <= 0 || scrollPosition.y > maxscroll - 4; // where 4 = scroll view's skin bound size?
            if (!autoScrolling && wasForcedBottom)
            {
                scrollPosition.y = maxscroll - 3;
            }
            else if (autoScrolling != atbottom)
            {
                autoScrolling = atbottom;
                scrollPosition.y = float.MaxValue;
                clearCache();
            }
        }


        if (!autoScrolling)
        {
            autoscrolltogglerect.x = width - autoscrolltogglerect.width;
            autoscrolltogglerect.y = height - autoscrolltogglerect.height;

            if (GUI.Button(autoscrolltogglerect, "Scroll to bottom"))
            {
                autoScrolling = true;
            }
        }
    }

    private void DrawFocusedLog(float width, float height)
    {
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        if (GUILayout.Button("Back", style.MenuStyle))
        {
            Defocus();
        }
        else
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
            GUILayout.Label(focusedLog.message, style.GetStyleForLogLevel(focusedLog.level), maxwidthscreen);
            GUILayout.Label(focusedLog.stack, maxwidthscreen);
            GUILayout.EndScrollView();
        }
    }

    private void Defocus()
    {
        if (focusedLog != null)
        {
            autoScrolling = true;
            scrollPosition.y = float.MaxValue;
            focusedLog = null;
        }
    }

    void EnsureScrollPosition()
	{
		if(autoScrolling)
		{
			scrollPosition.y = float.MaxValue;
		}
	}
	
	ConsoleLog PrintCachedLogs(GUILayoutOption maxwidthscreen)
	{
		ConsoleLog log;
	    ConsoleLog clicked = null;
		for (int i = cachedLogs.Count - 1; i >= 0; i--)
		{
			log = cachedLogs[i];
			if(log.repeats > 0)
			{
                GUILayout.Label(log.repeats + "x " + log.message, style.GetStyleForLogLevel(log.level), maxwidthscreen);
			}
			else
			{
				GUILayout.Label(log.message, style.GetStyleForLogLevel(log.level), maxwidthscreen);
            }
            if (Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                clicked = log;
            }
		}
	    return clicked;
	}
		
	bool ShouldShow(ConsoleLog log)
	{
		return (log.level >= viewingLevel 
			&& (viewingChannels == null || Array.IndexOf(viewingChannels, log.channel) >= 0)
	        && (searchTerm == "" || log.message.ToLower().Contains(searchTerm)));
	}
	
	void CacheBottomOfLogs(float width, float height)
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		List<ConsoleLog> logs = logger.Logs;
		ConsoleLog log;
		float lineHeight = style.GetLogLineHeight();
		for(int i = logs.Count - 1; i >= 0 && height > 0; i--)
		{
			log = logs[i];
			if (ShouldShow(log))
			{
				cachedLogs.Add(log);
				height -= lineHeight;
			}
		}
	}
	
	void CacheAllOfLogs()
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		List<ConsoleLog> logs = logger.Logs;
		ConsoleLog log;
		for(int i = logs.Count - 1; i >= 0; i--)
		{
			log = logs[i];
			if (ShouldShow(log))
			{
				cachedLogs.Add(log);
			}
		}
	}
	
	public void clearCache(){
		cachedLogs = null;
	}
}

public enum ConsoleMenu
{
    Channels,
    Levels,
    Search,
    Menu,
    Hide
}