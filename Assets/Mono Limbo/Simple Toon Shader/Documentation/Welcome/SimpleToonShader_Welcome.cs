using UnityEditor;
using UnityEngine;
using System.IO;


namespace MonoLimbo
{

    [InitializeOnLoad]
    public static class SimpleToonShader_Welcome
    {
        private const string DontShowKey = "SimpleToonShader_Welcome_DontShow";

        static SimpleToonShader_Welcome()
        {
            if (!EditorPrefs.HasKey(DontShowKey))
            {
                EditorApplication.update += OpenWindowOnce;
            }
        }

        private static void OpenWindowOnce()
        {
            EditorApplication.update -= OpenWindowOnce;
            WelcomeWindow.ShowWindow();
        }
    }

    public class WelcomeWindow : EditorWindow
    {
        private Texture2D banner;
        private bool dontShowAgain;

        private const string publisherUrl = "https://assetstore.unity.com/packages/vfx/shaders/one-click-toon-shader-fast-stylized-rendering-318465";

        // Relative path to documentation folder inside the imported package
        private const string documentationFolderName = "Mono Limbo/Simple Toon/Documentation";

        public static void ShowWindow()
        {
            var window = GetWindow<WelcomeWindow>("Simple Toon Shader");
            window.minSize = new Vector2(430, 520);
        }

        private void OnEnable()
        {
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
                GUILayout.Label("Simple Toon Shader", EditorStyles.boldLabel);
            }

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Thank you for downloading Simple Toon Shader.", EditorStyles.wordWrappedLabel);

            GUILayout.Space(12);
            EditorGUILayout.LabelField("Quick Start", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Create a new Material.\n" +
                "2. Assign Shader Graphs/SimpleToonShader.\n" +
                "3. Drag the material onto your model.\n\n" +
                "Ready to use immediately.",
                MessageType.Info
            );

            GUILayout.Space(18);
            EditorGUILayout.LabelField("Documentation", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Documentation Folder"))
            {
                OpenDocumentation();
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Support", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Support information is included in the Documentation folder.");

            GUILayout.Space(18);
            EditorGUILayout.LabelField("Enjoying the asset?", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("If you find this useful, leaving a review helps a lot.");

            GUI.backgroundColor = new Color(1f, 0.75f, 0.2f);
            if (GUILayout.Button("Leave a Review"))
            {
                Application.OpenURL(publisherUrl);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);
            dontShowAgain = EditorGUILayout.Toggle("Do not show again", dontShowAgain);

            if (dontShowAgain)
            {
                EditorPrefs.SetInt("SimpleToonShader_Welcome_DontShow", 1);
            }
        }

        private void OpenDocumentation()
        {
            // Search for the documentation folder anywhere inside the project
            string[] allFolders = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

            foreach (string folder in allFolders)
            {
                if (folder.Replace("\\", "/").EndsWith(documentationFolderName))
                {
                    EditorUtility.RevealInFinder(folder);
                    return;
                }
            }

            EditorUtility.DisplayDialog(
                "Documentation Not Found",
                "The documentation folder could not be found.\n\n" +
                "Expected location: Assets/" + documentationFolderName,
                "OK"
            );
        }
    }
}