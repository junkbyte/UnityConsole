using UnityEngine;

namespace com.spaceape.jbconsole
{
    public class JBConsoleFontReference : ScriptableObject
    {
        public Font font;

        public static JBConsoleFontReference Load()
        {
            return Resources.Load<JBConsoleFontReference>("JBConsoleFontReference");
        }

        public static Font GetDefaultFont()
        {
            var reference = Load();
            Font font = null;
            if (reference)
            {
                font = reference.font;
            }
            return font;
        }
    }
}