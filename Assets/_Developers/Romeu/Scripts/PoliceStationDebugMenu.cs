using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Police Station secret debug cheats. Opens only by typing "debug" (no visible entry point).
/// Uses IMGUI so mouse clicks always work regardless of EventSystem / Input System wiring.
/// </summary>
public class PoliceStationDebugMenu : MonoBehaviour
{
    const string TargetScene = "PoliceStation";
    const string CheatCode = "debug";
    const float BufferTimeout = 2.5f;

    static readonly int[] PointPresets = { 0, 1000, 7000, 15000, 30000, 50000 };

    readonly System.Text.StringBuilder _buffer = new System.Text.StringBuilder(16);
    float _lastKeyTime;
    bool _open;

    string _feedback = "Clique um botao para aplicar";
    Color _feedbackColor = Color.white;
    float _feedbackUntil;
    float _flashUntil;
    MainMenu _hubMenu;

    GUIStyle _windowStyle;
    GUIStyle _titleStyle;
    GUIStyle _statusStyle;
    GUIStyle _feedbackStyle;
    GUIStyle _sectionStyle;
    GUIStyle _buttonStyle;
    bool _stylesReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryAttach(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TryAttach(scene);

    static void TryAttach(Scene scene)
    {
        if (scene.name != TargetScene)
            return;
        if (Object.FindObjectOfType<PoliceStationDebugMenu>() != null)
            return;

        var go = new GameObject("PoliceStationDebugMenu");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<PoliceStationDebugMenu>();
    }

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != TargetScene)
            enabled = false;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != TargetScene)
            return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && _open)
        {
            _open = false;
            return;
        }

        if (Time.unscaledTime - _lastKeyTime > BufferTimeout && _buffer.Length > 0)
            _buffer.Clear();

        if (keyboard == null)
            return;

        TryAppendKey(keyboard.dKey, 'd');
        TryAppendKey(keyboard.eKey, 'e');
        TryAppendKey(keyboard.bKey, 'b');
        TryAppendKey(keyboard.uKey, 'u');
        TryAppendKey(keyboard.gKey, 'g');
    }

    void TryAppendKey(UnityEngine.InputSystem.Controls.KeyControl key, char c)
    {
        if (key != null && key.wasPressedThisFrame)
            AppendCheatChar(c);
    }

    void AppendCheatChar(char c)
    {
        int nextIndex = _buffer.Length;
        if (nextIndex >= CheatCode.Length)
            nextIndex = 0;

        if (c != CheatCode[nextIndex])
        {
            _buffer.Clear();
            if (c == CheatCode[0])
            {
                _lastKeyTime = Time.unscaledTime;
                _buffer.Append(c);
            }
            return;
        }

        _lastKeyTime = Time.unscaledTime;
        _buffer.Append(c);

        if (_buffer.ToString() == CheatCode)
        {
            _buffer.Clear();
            ToggleOpen();
        }
    }

    void ToggleOpen()
    {
        _open = !_open;
        if (_open)
            ShowFeedback("Menu aberto — clique um botão", new Color(0.4f, 1f, 0.55f));
    }

    void OnGUI()
    {
        if (!_open || SceneManager.GetActiveScene().name != TargetScene)
            return;

        EnsureStyles();

        float w = Mathf.Min(640f, Screen.width - 40f);
        float h = Mathf.Min(560f, Screen.height - 40f);
        Rect win = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

        Color prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
        GUI.color = prev;

        if (Time.unscaledTime < _flashUntil)
            GUI.backgroundColor = new Color(0.25f, 0.85f, 0.4f, 1f);
        else
            GUI.backgroundColor = new Color(0.18f, 0.2f, 0.24f, 1f);

        GUI.Window(918273, win, DrawWindow, "", _windowStyle);
        GUI.backgroundColor = Color.white;
    }

    void DrawWindow(int id)
    {
        GUILayout.Space(6);
        GUILayout.Label("DEBUG — Career Cheats", _titleStyle);

        bool showFlash = Time.unscaledTime < _feedbackUntil;
        _feedbackStyle.normal.textColor = showFlash ? _feedbackColor : new Color(0.85f, 0.9f, 0.95f);
        GUILayout.Label(showFlash ? _feedback : _feedback, _feedbackStyle);

        var cp = CareerPoints.instance;
        if (cp == null)
        {
            GUILayout.Label("ERRO: CareerPoints nao encontrado", _statusStyle);
            return;
        }

        GUILayout.Label(
            $"Pts {cp.Points}  |  Lost {cp.LostPoints}  |  Missions {cp.MissionsCompleted}\n" +
            $"FR {cp.FastResponseCompleted}/10   Pursuit {cp.PursuitCompleted}/10   Rescue {cp.RescueCompleted}/10   Boss {cp.BossCompleted}/1\n" +
            $"Bumper {(cp.BumperUnlockedPermanent ? "PERM" : (cp.BumperUnlocked ? "ON" : "OFF"))}   " +
            $"Shield {(cp.ShieldUnlockedPermanent ? "PERM" : (cp.ShieldUnlocked ? "ON" : "OFF"))}   " +
            $"Slot {(cp.SlotUnlocked ? "ON" : "OFF")}   " +
            $"Color {(cp.ColorUnlocked ? "ON" : "OFF")}@{cp.ColorUnlockPoints}   " +
            $"Secret {(cp.SecretCarUnlocked ? (cp.usingSecretCar ? "USING" : "UNLOCKED") : "LOCKED")}",
            _statusStyle);

        GUILayout.Space(4);
        GUILayout.Label("POINTS", _sectionStyle);
        GUILayout.BeginHorizontal();
        foreach (int preset in PointPresets)
        {
            int value = preset;
            if (GUILayout.Button(value.ToString(), _buttonStyle, GUILayout.Height(32)))
                Apply($"Points = {value}", () => cp.SetPoints(value));
        }
        if (GUILayout.Button("Lost=0", _buttonStyle, GUILayout.Height(32)))
            Apply("Lost Points = 0", () => cp.SetLostPoints(0));
        GUILayout.EndHorizontal();

        GUILayout.Label("MISSIONS (Wanted)", _sectionStyle);
        MissionRow("FR", MissionType.FastResponse, 10);
        MissionRow("Pursuit", MissionType.Pursuit, 10);
        MissionRow("Rescue", MissionType.Rescue, 10);
        MissionRow("Boss", MissionType.Boss, 1);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock Pursuit", _buttonStyle, GUILayout.Height(30)))
        {
            Apply("Pursuit desbloqueado", () =>
            {
                if (cp.FastResponseCompleted < 1)
                    cp.SetMissionCount(MissionType.FastResponse, 1);
            });
        }
        if (GUILayout.Button("Unlock Rescue", _buttonStyle, GUILayout.Height(30)))
        {
            Apply("Rescue desbloqueado", () =>
            {
                if (cp.FastResponseCompleted < 1)
                    cp.SetMissionCount(MissionType.FastResponse, 1);
                if (cp.PursuitCompleted < 1)
                    cp.SetMissionCount(MissionType.Pursuit, 1);
            });
        }
        if (GUILayout.Button("Unlock Boss", _buttonStyle, GUILayout.Height(30)))
        {
            Apply("Boss desbloqueado", () =>
            {
                cp.SetMissionCount(MissionType.FastResponse, 10);
                cp.SetMissionCount(MissionType.Pursuit, 10);
                cp.SetMissionCount(MissionType.Rescue, 10);
            });
        }
        if (GUILayout.Button("Max All", _buttonStyle, GUILayout.Height(30)))
        {
            Apply("Tudo liberado (Max All)", () =>
            {
                cp.SetMissionCount(MissionType.FastResponse, CareerPoints.MissionQuota);
                cp.SetMissionCount(MissionType.Pursuit, CareerPoints.MissionQuota);
                cp.SetMissionCount(MissionType.Rescue, CareerPoints.MissionQuota);
                cp.SetMissionCount(MissionType.Boss, CareerPoints.BossQuota);
                cp.SetPoints(50000);
                cp.SetBumperPermanent(true);
                cp.SetShieldPermanent(true);
                cp.SetColorUnlocked(true);
                cp.SetSecretCarUnlocked(true);
            });
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("UPGRADES / SECRET", _sectionStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Bumper", _buttonStyle, GUILayout.Height(30)))
            Apply($"Bumper permanente = {!cp.BumperUnlockedPermanent}", () => cp.SetBumperPermanent(!cp.BumperUnlockedPermanent));
        if (GUILayout.Button("Toggle Shield", _buttonStyle, GUILayout.Height(30)))
            Apply($"Shield permanente = {!cp.ShieldUnlockedPermanent}", () => cp.SetShieldPermanent(!cp.ShieldUnlockedPermanent));
        if (GUILayout.Button("Toggle Color", _buttonStyle, GUILayout.Height(30)))
            Apply($"Color unlocked = {!cp.ColorUnlocked}", () => cp.SetColorUnlocked(!cp.ColorUnlocked));
        if (GUILayout.Button("Unlock Secret", _buttonStyle, GUILayout.Height(30)))
            Apply("Secret car desbloqueado", () => cp.SetSecretCarUnlocked(true));
        if (GUILayout.Button("Toggle Use Secret", _buttonStyle, GUILayout.Height(30)))
        {
            Apply("Uso do secret car alternado", () =>
            {
                if (!cp.SecretCarUnlocked)
                    cp.SetSecretCarUnlocked(true);
                cp.usingSecretCar = !cp.usingSecretCar;
            });
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fresh Save (reset)", _buttonStyle, GUILayout.Height(34)))
            Apply("Progresso resetado", () => cp.ResetProgressFull());
        if (GUILayout.Button("Fechar (Esc)", _buttonStyle, GUILayout.Height(34)))
            _open = false;
        GUILayout.EndHorizontal();
    }

    void MissionRow(string label, MissionType type, int max)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"{label} -", _buttonStyle, GUILayout.Height(28)))
            Apply($"{label} -1", () => CareerPoints.instance.SetMissionCount(type, GetCount(type) - 1));
        if (GUILayout.Button($"{label} +", _buttonStyle, GUILayout.Height(28)))
            Apply($"{label} +1", () => CareerPoints.instance.SetMissionCount(type, GetCount(type) + 1));
        if (GUILayout.Button($"{label}={max}", _buttonStyle, GUILayout.Height(28)))
            Apply($"{label} = {max}", () => CareerPoints.instance.SetMissionCount(type, max));
        if (GUILayout.Button($"{label}=0", _buttonStyle, GUILayout.Height(28)))
            Apply($"{label} = 0", () => CareerPoints.instance.SetMissionCount(type, 0));
        GUILayout.EndHorizontal();
    }

    static int GetCount(MissionType type)
    {
        var cp = CareerPoints.instance;
        switch (type)
        {
            case MissionType.FastResponse: return cp.FastResponseCompleted;
            case MissionType.Pursuit: return cp.PursuitCompleted;
            case MissionType.Rescue: return cp.RescueCompleted;
            case MissionType.Boss: return cp.BossCompleted;
            default: return 0;
        }
    }

    void Apply(string label, System.Action mutate)
    {
        if (CareerPoints.instance == null)
        {
            ShowFeedback("ERRO: CareerPoints nao encontrado", new Color(1f, 0.35f, 0.35f));
            return;
        }

        mutate?.Invoke();
        CareerPoints.instance.Save();

        if (_hubMenu == null)
            _hubMenu = Object.FindObjectOfType<MainMenu>();
        if (_hubMenu != null)
            _hubMenu.RefreshHubUI();

        ShowFeedback($"APLICADO: {label}", new Color(0.35f, 1f, 0.45f));
        _flashUntil = Time.unscaledTime + 0.35f;
    }

    void ShowFeedback(string msg, Color color)
    {
        _feedback = msg;
        _feedbackColor = color;
        _feedbackUntil = Time.unscaledTime + 2f;
    }

    void EnsureStyles()
    {
        if (_stylesReady)
            return;

        _windowStyle = new GUIStyle(GUI.skin.window);
        _windowStyle.normal.background = MakeTex(4, 4, new Color(0.12f, 0.14f, 0.18f, 0.97f));
        _windowStyle.onNormal.background = _windowStyle.normal.background;
        _windowStyle.padding = new RectOffset(14, 14, 12, 12);

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = Color.white;

        _feedbackStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };

        _statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            wordWrap = true
        };
        _statusStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);

        _sectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
        _sectionStyle.normal.textColor = new Color(0.55f, 0.85f, 1f);
        _sectionStyle.margin = new RectOffset(0, 0, 8, 2);

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
        _buttonStyle.normal.textColor = Color.white;
        _buttonStyle.hover.textColor = Color.white;
        _buttonStyle.active.textColor = Color.white;
        _buttonStyle.normal.background = MakeTex(4, 4, new Color(0.28f, 0.36f, 0.48f, 1f));
        _buttonStyle.hover.background = MakeTex(4, 4, new Color(0.38f, 0.55f, 0.75f, 1f));
        _buttonStyle.active.background = MakeTex(4, 4, new Color(0.15f, 0.7f, 0.35f, 1f));

        _stylesReady = true;
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = col;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
