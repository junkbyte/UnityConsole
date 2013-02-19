using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void MenuHandler();

public class JBConsole : MonoBehaviour
{
	
	private static JBConsole _instance;
	
	public static JBConsole Start()
	{
		if(_instance == null)
		{
			GameObject go = new GameObject("JBConsole");
			_instance = go.AddComponent<JBConsole>();
		}
		return _instance;
	}
	
	public static JBConsole instance
	{
		get
		{
			return _instance;
		}
	}
	
	public static bool exists
	{
		get { return _instance != null; }
	}
	
	private JBLogger logger;
	private JBCStyle style;

    public bool visible = false;
    public int menuItemWidth = 100;
	
    string[] levels;
    string[] topMenu;
    Dictionary<int, MenuHandler> customMenuHandlers;

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
		
	public JBConsole()
	{
		style = new JBCStyle();
	}
	
	void Awake ()
	{
		if(_instance == null) _instance = this;
		
		logger = JBLogger.instance;
		
        levels = Enum.GetNames(typeof(ConsoleLevel));
        topMenu = currentTopMenu = Enum.GetNames(typeof(ConsoleMenu));

        customMenus = new string[0];
        customMenuHandlers = new Dictionary<int, MenuHandler>();
		
		
		//
		AddMenu("Email", delegate()
		{
			string body = "";
			foreach(ConsoleLog log in logger.Logs)
			{
				body += log.content.text + "\n";
			}
			body = WWW.EscapeURL(body);
			Application.OpenURL("mailto:?subject=ConsoleLog&body="+body);
		});
	}
	
	void Update ()
    {
		if(autoScrolling && logger.stateHash != stateHash)
		{
			stateHash = logger.stateHash;
			clearCache();
		}
	}
	
    public static void AddMenu(string name, MenuHandler callback)
	{
		if(!exists) return;
		instance.AddCustomMenu(name, callback);
	}
	
    public static void RemoveMenu(string name)
	{
		if(!exists) return;
		instance.RemoveCustomMenu(name);
	}

	
    public void AddCustomMenu(string name, MenuHandler callback)
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
		/*
        float screenScale = Screen.width / 320f;
	    Matrix4x4 scaledMatrix = Matrix4x4.Scale(Vector3.one * screenScale);
	    GUI.matrix = scaledMatrix;
         */
		DrawGUI(Screen.width, Screen.height);
	}
	
	public void DrawGUI(float width, float height)
	{
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        GUILayout.BeginVertical(style.Skin.box);

        //GUILayout.BeginHorizontal();
		//GUILayout.FlexibleSpace();
        int selection = GUILayout.Toolbar(-1, currentTopMenu, GUILayout.MinWidth(280), GUILayout.MaxWidth(380));
        if (selection >= 0)
        {
            OnMenuSelection(selection);
        }
		//GUILayout.EndHorizontal();

        if (currentSubMenu != null)
        {
			if(currentSubMenu.Length == 0)
			{
				GUILayout.Label("No Custom Menus...");
			}
			else
			{
				selection = GUILayout.SelectionGrid(-1, currentSubMenu, (int)width / menuItemWidth);
	            if (selection >= 0 && subMenuHandler != null)
	            {
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
            	searchTerm = newTerm.ToLower();
				clearCache();
			}
            
            GUI.FocusControl("SearchTF");
        }
		
		bool wasForcedBottom = scrollPosition.y == float.MaxValue;
		
		if(autoScrolling)
		{
			if(cachedLogs == null && Event.current.type == EventType.Layout)
			{
				scrollPosition.y = float.MaxValue;
			}
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
			if (cachedLogs == null)
	        {
	            CacheBottomOfLogs(width, height);
	        }
			PrintCachedLogs(maxwidthscreen);
		}
        else
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, maxwidthscreen);
			if (cachedLogs == null)
	        {
	            CacheAllOfLogs();
	        }
			PrintCachedLogs(maxwidthscreen);
		}
		Rect lastContentRect = GUILayoutUtility.GetLastRect();
		GUILayout.EndScrollView();
		
		Rect scrollViewRect = GUILayoutUtility.GetLastRect();
		if(Event.current.type == EventType.Repaint)
		{
			float maxscroll = lastContentRect.y + lastContentRect.height - scrollViewRect.height;
			
			bool atbottom = maxscroll <= 0 || scrollPosition.y > maxscroll - 4; // where 4 = scroll view's skin bound size?
			if(!autoScrolling && wasForcedBottom)
			{
				scrollPosition.y = maxscroll - 1;
			}
			else if(autoScrolling != atbottom)
			{
				autoScrolling = atbottom;
				scrollPosition.y = float.MaxValue;
				clearCache();
			}
		}
		
        GUILayout.EndVertical();
		
		if(!autoScrolling)
		{
			autoscrolltogglerect.x = width - autoscrolltogglerect.width;
			autoscrolltogglerect.y = height - autoscrolltogglerect.height;

			if(GUI.Button(autoscrolltogglerect, "Scroll to bottom"))
			{
				autoScrolling = true;
			}
		}
		
		if(GUI.tooltip.Length > 0)
		{
			//GUI.Label(new Rect(0, Screen.height - Input.mousePosition.y + 10, Screen.width, 100), GUI.tooltip);
		}
	}
	
	void EnsureScrollPosition()
	{
		if(autoScrolling)
		{
			scrollPosition.y = float.MaxValue;
		}
	}
	
	void PrintCachedLogs(GUILayoutOption maxwidthscreen)
	{
		ConsoleLog log;
		for (int i = cachedLogs.Count - 1; i >= 0; i--)
		{
			log = cachedLogs[i];
			if(log.repeats > 0)
			{
				GUILayout.Label(log.repeats + "x " +log.content.text, style.GetStyleForLogLevel(log.level), maxwidthscreen);
			}
			else
			{
				GUILayout.Label(log.content, style.GetStyleForLogLevel(log.level), maxwidthscreen);
			}
		}
	}
		
	bool ShouldShow(ConsoleLog log)
	{
		return (log.level >= viewingLevel 
			&& (viewingChannels == null || Array.IndexOf(viewingChannels, log.channel) >= 0)
	        && (searchTerm == "" || log.content.text.ToLower().Contains(searchTerm)));
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

delegate void SubMenuHandler(int index);

public enum ConsoleMenu
{
    Channels,
    Levels,
    Search,
    Menu,
    Hide
}