// by vassago1 from https://forum.unity.com/threads/having-multitap-interaction-issues.859300/

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class DashInteraction : IInputInteraction
{
    public float firstTapTime = 0.2f;
    public float tapDelay = 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() { }

    static DashInteraction()
    {
        InputSystem.RegisterInteraction<DashInteraction>();
    }

    void IInputInteraction.Process(ref InputInteractionContext context)
    {
        if (context.timerHasExpired)
        {
            context.Canceled();
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Waiting:
                if (context.ControlIsActuated(firstTapTime))
                {
                    context.Started();
                    context.SetTimeout(tapDelay);
                }
                break;

            case InputActionPhase.Started:
                if (context.ControlIsActuated())
                    context.Performed();
                break;
        }
    }

    void IInputInteraction.Reset() { }
}

