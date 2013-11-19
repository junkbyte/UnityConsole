using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JBCStyle
{
    private const int MENU_FONT_SIZE = 14;
	
	Dictionary<ConsoleLevel, GUIStyle> levelToStyle;

    GUIStyle boxStyle;
    GUIStyle menuStyle;
	Texture2D bgTexture;
	
	public JBCStyle()
	{
		bgTexture = new Texture2D(1, 1);
		bgTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
		bgTexture.Apply();
		
		boxStyle = new GUIStyle();
		boxStyle.normal.background = bgTexture;
        boxStyle.normal.textColor = Color.white;

        menuStyle = new GUIStyle(GUI.skin.button);
        menuStyle.normal.textColor = Color.white;
        menuStyle.fontSize = MENU_FONT_SIZE;
        menuStyle.fontStyle = FontStyle.Bold;
        menuStyle.normal.textColor = new Color(0.8f, 0.6f, 0f);
		
		levelToStyle = new Dictionary<ConsoleLevel, GUIStyle>();
		
		GUIStyle style = MakeLogStyle();
		style.normal.textColor = new Color(0.62f, 0.82f, 0.62f);
		levelToStyle[ConsoleLevel.Debug] = style;
		style = MakeLogStyle();
		style.normal.textColor = new Color(1f, 0.87f, 0.87f);
		levelToStyle[ConsoleLevel.Info] = style;
		style = MakeLogStyle();
		style.normal.textColor = new Color(1f, 0.47f, 0.47f);
		levelToStyle[ConsoleLevel.Warn] = style;
		style = MakeLogStyle();
		style.normal.textColor = new Color(1f, 0.133f, 0.133f);
		levelToStyle[ConsoleLevel.Error] = style;
		levelToStyle[ConsoleLevel.Fatal] = style;
	}
	
	GUIStyle MakeLogStyle()
	{
		var style = new GUIStyle();
		style.richText = true;
	    style.wordWrap = true;
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
    }
	
	public GUIStyle GetStyleForLogLevel(ConsoleLevel level)
	{
		return levelToStyle[level];
	}
}
