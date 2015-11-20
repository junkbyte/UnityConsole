using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void JBConsoleMenuHandler();
public delegate void JBCDrawBodyHandler(float width, float height, float scale = 1);
public delegate void JBCLogSelectedHandler(ConsoleLog log);

public class JBConsole : MonoBehaviour
{
    private const int baseFontSize = 14;
    delegate void SubMenuHandler(int index);

    public static JBConsole Start(bool visible = true)
	{
		if(instance == null)
		{
			var go = new GameObject("JBConsole");
			instance = go.AddComponent<JBConsole>();
			instance.Visible = visible;
		}
		return instance;
	}

    public static JBConsole instance { get; private set; }

    public static bool Exists
	{
		get { return instance != null; }
    }

	public static void AddMenu(string name, Action callback)
    {
        if (!Exists) return;
		instance.Menu.Add(name, callback);
    }

    public static void RemoveMenu(string name)
    {
        if (!Exists) return;
		instance.Menu.Remove(name);
    }

	
	private JBLogger logger;
    private JBCStyle _style;
    public JBCStyle style { get { if (_style == null) _style = new JBCStyle(); return _style; }
    }

    public int menuItemWidth = 135;
    public int BaseDPI = 100;
    private bool _visible = true;
    public bool Visible
    {
        get { return _visible; }
        set
        {
            _visible = value;
            if (OnVisiblityChanged != null) OnVisiblityChanged();
        }
    }

	public JBCustomMenu Menu {get; private set;}

	public event Action OnVisiblityChanged;

    JBCDrawBodyHandler DrawGUIBodyHandler;
    public JBCLogSelectedHandler OnLogSelectedHandler;
	
    string[] levels;
    string[] topMenu;

    ConsoleLevel viewingLevel = ConsoleLevel.Debug;
    string[] viewingChannels;

    int currentTopMenuIndex = -1;
    string[] currentTopMenu;
    string[] currentSubMenu;
    SubMenuHandler subMenuHandler;
	
	List<ConsoleLog> cachedLogs;

    string searchTerm = "";

	bool autoScrolling = true;
	
	Rect autoscrolltogglerect = new Rect(0, 0, 110, 22);

    Vector2 scrollPosition;
	Vector3 touchPosition;
	float scrollVelocity;
	bool scrolling;
	bool scrolled;

	int stateHash;
	
	void Awake ()
	{
		if(instance == null) instance = this;

	    gameObject.AddComponent<JBCInspector>();
		
		DontDestroyOnLoad(gameObject);

        logger = JBLogger.instance;
		
		Menu = new JBCustomMenu();
        levels = Enum.GetNames(typeof(ConsoleLevel));
        topMenu = currentTopMenu = Enum.GetNames(typeof(ConsoleMenu));
		Menu.Add("Clear", Clear);
	}

	void Clear()
	{
		if(logger != null)
		{
			scrolling = false;
			autoScrolling = true;
			logger.Clear();
		}
	}
	
	void Update ()
    {
		if (logger == null) return;
		
		if(autoScrolling && logger.stateHash != stateHash)
		{
			stateHash = logger.stateHash;
			clearCache();
		}

		if(Visible && Input.GetMouseButton(0))
		{
			if(!scrolling && Input.mousePosition.y < Screen.height - GetMenuHeight(GetGuiScale()))
			{
				clearCache();
				if(autoScrolling) scrollPosition.y = int.MaxValue;
				autoScrolling = false;
				scrolling = true;
				scrolled = false;
				touchPosition = Input.mousePosition;
			}
		}
		else 
		{
			scrolling = false;
		}
		if(scrolling)
		{
			var touch = Input.mousePosition;
			scrollVelocity = (touch - touchPosition).y;
			scrollPosition.y += scrollVelocity;
			touchPosition = touch;
			scrolled |= Mathf.Abs(scrollVelocity) > 3f;
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
				Menu.PopToRoot();
				subMenuHandler = Menu.OnCurrentLinkClicked;
                break;
            case (int)ConsoleMenu.Hide:
                Visible = !Visible;
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
		if (index >= result.Length) return result;
        result[index] = "["+ result[index]+"]";
        return result;
    }
	
	void OnGUI ()
	{
        if (!Visible) return;

        var depth = GUI.depth;
		GUI.depth = int.MaxValue - 10;
		
		var scale = GetGuiScale();
		style.SetScale(scale);
		if(!scrolling && Event.current.type == EventType.Layout)
		{
			scrollVelocity *= 0.95f;
			scrollPosition.y += scrollVelocity;
		}

        DrawGUI(Screen.width, Screen.height, scale);


        GUI.depth = depth;
	}

	float GetGuiScale()
	{
		var scale = 1f;
		var dpi = Screen.dpi;
#if !UNITY_EDITOR
		if(dpi <= 0) dpi = 150;
#endif
		if (dpi > 0 && BaseDPI > 0) scale = dpi / BaseDPI;
		return scale;
	}

	float GetMenuHeight(float scale)
	{
		return style.MenuStyle.fontSize + (10 * scale);
	}
	
	public void DrawGUI(float width, float height, float scale = 1, bool showScrollBar = false)
    {
        GUILayout.BeginVertical(style.BoxStyle);

		var menuheight = GetMenuHeight(scale);

        int selection = GUILayout.Toolbar(-1, currentTopMenu, style.MenuStyle, GUILayout.MinWidth(320 * scale), GUILayout.MaxWidth(Screen.width), GUILayout.Height(menuheight));
        if (selection >= 0)
        {
            Defocus();
            OnMenuSelection(selection);
        }

		if(currentTopMenuIndex == (int)ConsoleMenu.Menu)
		{
			currentSubMenu = Menu.GetCurrentMenuLink();
		}
        if (currentSubMenu != null)
        {
			if(currentSubMenu.Length == 0)
			{
				GUILayout.Label("No Custom Menus...");
			}
			else
			{
			    var count = (int) (width/(menuItemWidth*scale));
                var rows = Mathf.Ceil((float) currentSubMenu.Length / (float)count);
                selection = GUILayout.SelectionGrid(-1, currentSubMenu, count, style.MenuStyle, GUILayout.Height(menuheight * rows));
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

        if (DrawGUIBodyHandler == null)
        {
			DrawLogScroll(width, height, showScrollBar);
        }
        else
        {
            DrawGUIBodyHandler(width, height, scale);
        }

	    GUILayout.EndVertical();
	}

	void BeginScrollView(bool showScrollBar, GUILayoutOption options)
	{
		if(showScrollBar) scrollPosition = GUILayout.BeginScrollView(scrollPosition, options);
		else scrollPosition = GUILayout.BeginScrollView(scrollPosition, style.HiddenScrollBar, style.HiddenScrollBar, options);
	}

	private void DrawLogScroll(float width, float height, bool showScrollBar)
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
			BeginScrollView(showScrollBar, maxwidthscreen);
            if (cachedLogs == null)
            {
                CacheBottomOfLogs(width, height);
            }
            clickedLog = PrintCachedLogs(maxwidthscreen);
        }
        else
        {
			BeginScrollView(showScrollBar, maxwidthscreen);
            if (cachedLogs == null)
            {
                CacheAllOfLogs();
            }
            clickedLog = PrintCachedLogs(maxwidthscreen);
        }
		bool hasLogs = cachedLogs.Count > 0;

		Rect lastContentRect = hasLogs ? GUILayoutUtility.GetLastRect() : new Rect();

        GUILayout.EndScrollView();

		if (!scrolling && hasLogs && Event.current.type == EventType.Repaint)
        {
			Rect scrollViewRect = GUILayoutUtility.GetLastRect();
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
				scrollVelocity = 0f;
                autoScrolling = true;
				clickedLog = null;
            }
		}

		if (clickedLog != null && OnLogSelectedHandler != null)
		{
			OnLogSelectedHandler(clickedLog);
		}
    }

    public void Focus(JBCDrawBodyHandler drawBodyHandler)
    {
        DrawGUIBodyHandler = drawBodyHandler;
    }

    public void Defocus()
    {
        if(DrawGUIBodyHandler != null)
        {
            autoScrolling = true;
            scrollPosition.y = float.MaxValue;
            DrawGUIBodyHandler = null;
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
                GUILayout.Label((log.repeats + 1) + "x " + log.message, style.GetStyleForLogLevel(log.level), maxwidthscreen);
			}
			else
			{

				GUILayout.Label(log.Time.ToLongTimeString() + "-" + log.message, style.GetStyleForLogLevel(log.level), maxwidthscreen);
            }
			if (!scrolled && Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
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
	
	void clearCache()
    {
		cachedLogs = null;
	}


    public T RegisterPlugin<T>() where T : Component
	{
	    var comp = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
	    return comp;
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