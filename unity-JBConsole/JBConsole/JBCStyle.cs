using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.spaceape.jbconsole;

public class JBCStyle
{
	private const int MENU_FONT_SIZE = 14;
	private const int LOG_FONT_SIZE = 12;
	
	readonly Dictionary<ConsoleLevel, GUIStyle> levelToStyle = new Dictionary<ConsoleLevel, GUIStyle>();

    GUIStyle boxStyle;
	GUIStyle menuStyle;
	public GUIStyle SearchStyle { get; private set;}
	Texture2D bgTexture;
	public GUIStyle HiddenScrollBar { get; private set; }
	
	public JBCStyle()
	{
		Init(JBConsoleFontReference.GetDefaultFont());
	}
	
	public JBCStyle(Font font)
	{
		Init(font);
	}
	
	private void Init(Font font)
	{
		bgTexture = new Texture2D(1, 1);
		bgTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
		bgTexture.Apply();

		HiddenScrollBar = new GUIStyle(GUI.skin.verticalScrollbar);
		if (font != null)
		{
			HiddenScrollBar.font = font;
		}
		HiddenScrollBar.fixedHeight = HiddenScrollBar.fixedWidth = 0;

		boxStyle = new GUIStyle();
		if (font != null)
		{
			boxStyle.font = font;
		}
		boxStyle.normal.background = bgTexture;
		boxStyle.normal.textColor = Color.white;

		SearchStyle = new GUIStyle(GUI.skin.textField);
		if (font != null)
		{
			SearchStyle.font = font;
		}
		SearchStyle.fontSize = MENU_FONT_SIZE;

		menuStyle = new GUIStyle(GUI.skin.button);
		if (font != null)
		{
			menuStyle.font = font;
		}
		menuStyle.normal.textColor = Color.white;
		menuStyle.fontSize = MENU_FONT_SIZE;
		menuStyle.fontStyle = FontStyle.Bold;
		menuStyle.normal.textColor = new Color(0.8f, 0.6f, 0f);

		levelToStyle.Clear();
		levelToStyle[ConsoleLevel.Debug] = MakeLogStyle(font, new Color(0.62f, 0.82f, 0.62f));
		levelToStyle[ConsoleLevel.Info] = MakeLogStyle(font, new Color(1f, 0.87f, 0.87f));
		levelToStyle[ConsoleLevel.Warn] = MakeLogStyle(font, new Color(1f, 0.47f, 0.47f));
		levelToStyle[ConsoleLevel.Error] = MakeLogStyle(font, new Color(1f, 0.133f, 0.133f));
		levelToStyle[ConsoleLevel.Fatal] = MakeLogStyle(font, new Color(1f, 0.133f, 0.133f));
	}

	internal void SetFont(Font font)
	{
		HiddenScrollBar.font = font;
		boxStyle.font = font;
		menuStyle.font = font;
		SearchStyle.font = font;
		foreach (var levelStyle in levelToStyle)
		{
			levelStyle.Value.font = font;
		}
	}

	GUIStyle MakeLogStyle(Font font, Color textColor)
	{
		var style = new GUIStyle();
		style.fontSize = LOG_FONT_SIZE;
		style.richText = true;
	    style.wordWrap = true;
		style.normal.textColor = textColor;
		if (font != null)
		{
			style.font = font;
		}
		return style;
	}
	
	public float GetLogLineHeight()
	{
		return levelToStyle[ConsoleLevel.Debug].lineHeight;
	}
	
	public GUIStyle BoxStyle
	{
		get { return boxStyle; }
	}

    public GUIStyle MenuStyle
    {
        get { return menuStyle; }
    }

    public void SetScale(float scale)
    {
		menuStyle.fontSize = (int)(MENU_FONT_SIZE * scale);
		SearchStyle.fontSize = (int)(MENU_FONT_SIZE * scale);
		foreach(var s in levelToStyle)
		{
			s.Value.fontSize = (int)(LOG_FONT_SIZE * scale);
		}
    }
	
	public GUIStyle GetStyleForLogLevel(ConsoleLevel level)
	{
		return levelToStyle[level];
	}
}
