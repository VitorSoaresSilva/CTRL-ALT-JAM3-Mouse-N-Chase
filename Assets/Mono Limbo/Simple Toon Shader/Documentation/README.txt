using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SimpleToonShader_Welcome
{
    private const string DontShowKey = "SimpleToonShader_Welcome_DontShow";

    static SimpleToonShader_Welcome()
    {
        // Show only once per project
        if (!EditorPrefs.HasKey(DontShowKey))
        {
            EditorApplication.update += OpenWindowOnce;
        }
    }

    static void OpenWindowOnce()
    {
        EditorApplication.update -= OpenWindowOnce;
        WelcomeWindow.ShowWindow();
    }
}

public class WelcomeWindow : EditorWindow
{
    Texture2D banner;

    bool dontShowAgain;

    const string publisherUrl = "https://assetstore.unity.com/publishers/98904?preview=1";
    const string documentationUrl = "https://monogd.mydurable.com/fr";
    
    public static void ShowWindow()
    {
        var window = GetWindow<WelcomeWindow>("Simple Toon Shader");
        window.minSize = new Vector2(430, 520);
    }

    private void OnEnable()
    {
        // Optional: Add a custom banner image (PNG)
        banner = Resources.Load<Texture2D>("SimpleToon_Banner");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        if (banner != null)
        {
            GUILayout.Label(banner, GUILayout.Height(140));
        }
        else
        {
            GUILayout.Label("✨ Simple Toon Shader", EditorStyles.boldLabel);
        }

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Thanks for downloading!", EditorStyles.wordWrappedLabel);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Start", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
           "1. Right-click → Create → Material\n" +
           "2. Assign Shader Graphs/SimpleToonShader\n" +
           "3. Drag material onto your model\n\n" +
           "That's it! You're ready.",
           MessageType.Info
        );

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Useful Links", EditorStyles.boldLabel);

        if (GUILayout.Button("📘 Documentation"))
            Application.OpenURL(documentationUrl);

        if (GUILayout.Button("💬 Support / Contact"))
            Application.OpenURL(documentationUrl);

        GUILayout.Space(18);
        EditorGUILayout.LabelField("Enjoying the asset?", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("A review helps a LOT and keeps free tools coming ❤️");

        GUI.backgroundColor = new Color(1f, 0.75f, 0.2f);
        if (GUILayout.Button("⭐ Leave a Review"))
            Application.OpenURL(publisherUrl);
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();

        GUILayout.Space(10);
        dontShowAgain = EditorGUILayout.Toggle("Don’t show again", dontShowAgain);

        if (dontShowAgain)
        {
            EditorPrefs.SetInt("SimpleToonShader_Welcome_DontShow", 1);
        }
    }
}