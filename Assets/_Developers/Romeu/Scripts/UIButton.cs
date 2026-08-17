using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [SerializeField] TextMeshProUGUI StartText;
    [SerializeField] private UIPointer GlobalPointer;
    [field: SerializeField] public Vector3 PointerPosition { get; private set; }
    [field: SerializeField] public Vector3 PointerRotation { get; private set; }
    [SerializeField] private bool randomMission = false;
    [SerializeField] private MissionType _missionType;

    // Referenciar o texto de quantidade/progresso (ex: "0/10")
    [SerializeField] private TextMeshProUGUI ProgressText;

    // Ícone de bloqueio (opcional)
    [SerializeField] private Image LockIcon;

    // Array de GameObjects para aparecer quando a missão estiver desbloqueada
    [SerializeField] private GameObject[] UnlockedVisuals;

    private string btnText;
    private string progressText;
    private static UIButton previouslySelectedButton;

    public MissionType missionType
    {
        // Get random mission based on the missionType enum, mas apenas as desbloqueadas
        get
        {
            if (!randomMission)
                return _missionType;

            // Se randomMission está ativo, selecionar apenas entre missões desbloqueadas
            if (CareerPoints.instance == null)
                return _missionType;

            // Coletar todas as missões desbloqueadas
            List<MissionType> unlockedMissions = new List<MissionType>();
            foreach (MissionType mission in System.Enum.GetValues(typeof(MissionType)))
            {
                if (CareerPoints.instance.IsMissionUnlocked(mission))
                    unlockedMissions.Add(mission);
            }

            // Se há missões desbloqueadas, retornar uma aleatória
            if (unlockedMissions.Count > 0)
                return unlockedMissions[UnityEngine.Random.Range(0, unlockedMissions.Count)];

            // Fallback: retornar a missão padrão se nenhuma estiver desbloqueada
            return _missionType;
        }

        set => _missionType = value;
    }

    public void SetPointerAnchor(UIPointer pointer, Vector3 position, Vector3 rotationEuler)
    {
        if (pointer != null)
            GlobalPointer = pointer;
        else if (GlobalPointer == null)
            GlobalPointer = FindObjectOfType<UIPointer>();

        PointerPosition = position;
        PointerRotation = rotationEuler;
    }

    public Button Button { get; private set; }

    void OnEnable()
    {
        Button = GetComponent<Button>();

        if(GlobalPointer == null) GlobalPointer = FindObjectOfType<UIPointer>();

        // Inicializar estado do LockIcon baseado em desbloqueio
        if (CareerPoints.instance != null)
        {
            bool isMissionUnlocked = CareerPoints.instance.IsMissionUnlocked(missionType);
            if (LockIcon != null)
                LockIcon.gameObject.SetActive(!isMissionUnlocked);

            // Ocultar ProgressText se a missão estiver bloqueada
            if (ProgressText != null)
                ProgressText.gameObject.SetActive(isMissionUnlocked);

            // Ativar/desativar UnlockedVisuals baseado em desbloqueio
            if (UnlockedVisuals != null)
            {
                foreach (GameObject visual in UnlockedVisuals)
                {
                    if (visual != null)
                        visual.SetActive(isMissionUnlocked);
                }
            }
        }

        // Ocultar StartText inicialmente
        if (StartText != null)
            StartText.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Se havia um botão selecionado anteriormente, restaurar seu texto original
        if (previouslySelectedButton != null && previouslySelectedButton != this)
        {
            previouslySelectedButton.RestoreOriginalText();
        }

        // Verificar se a missão está desbloqueada
        bool isMissionUnlocked = CareerPoints.instance != null && CareerPoints.instance.IsMissionUnlocked(missionType);

        // Mover o pointer para esta missão
        if (GlobalPointer == null)
            GlobalPointer = FindObjectOfType<UIPointer>();
        if (GlobalPointer != null)
            GlobalPointer.MoveTo(PointerPosition, Quaternion.Euler(PointerRotation));

        // Se a missão está bloqueada, apenas garantir que o LockIcon está visível e ocultar StartText/ProgressText/UnlockedVisuals
        if (!isMissionUnlocked)
        {
            if (LockIcon != null)
                LockIcon.gameObject.SetActive(true);
            if (StartText != null)
                StartText.gameObject.SetActive(false);
            if (ProgressText != null)
                ProgressText.gameObject.SetActive(false);

            // Desativar UnlockedVisuals se bloqueado
            if (UnlockedVisuals != null)
            {
                foreach (GameObject visual in UnlockedVisuals)
                {
                    if (visual != null)
                        visual.SetActive(false);
                }
            }

            if (MissionTipBalloon.instance != null)
                MissionTipBalloon.instance.Hide();

            previouslySelectedButton = this;
            return;
        }

        // Missão desbloqueada - ocultar o ícone de bloqueio
        if (LockIcon != null)
            LockIcon.gameObject.SetActive(false);

        // Garantir que ProgressText fica visível quando desbloqueado
        if (ProgressText != null)
            ProgressText.gameObject.SetActive(true);

        // Ativar UnlockedVisuals quando desbloqueado
        if (UnlockedVisuals != null)
        {
            foreach (GameObject visual in UnlockedVisuals)
            {
                if (visual != null)
                    visual.SetActive(true);
            }
        }

        // Mostrar StartText apenas se a missão estiver desbloqueada
        if (StartText != null)
        {
            // Armazenar o texto original (progresso) se não foi armazenado antes
            if (string.IsNullOrEmpty(btnText))
                btnText = StartText.text;

            // Se há um ProgressText separado, usar ele como referência
            if (ProgressText != null)
                progressText = ProgressText.text;

            // Exibir "Play" temporariamente
            StartCoroutine(PutPlayText());
            IEnumerator PutPlayText()
            {
                StartText.gameObject.SetActive(true);
                StartText.text = "Play";
                yield return new WaitForSeconds(5);
                StartText.text = btnText;
                StartText.gameObject.SetActive(false);
            }
        }

        if ((StartText != null || ProgressText != null || randomMission)
            && MissionTipBalloon.instance != null)
            MissionTipBalloon.instance.ShowForMission(_missionType, randomMission);

        // Rastrear este botão como o último selecionado
        previouslySelectedButton = this;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Ocultar StartText quando deselecionar
        if (StartText != null)
        {
            StopAllCoroutines();
            StartText.gameObject.SetActive(false);
        }

        // ProgressText permanece ativado se a missão estiver desbloqueada
        if (ProgressText != null)
        {
            bool isMissionUnlocked = CareerPoints.instance != null && CareerPoints.instance.IsMissionUnlocked(missionType);
            ProgressText.gameObject.SetActive(isMissionUnlocked);
        }

        // UnlockedVisuals permanece ativado se a missão estiver desbloqueada
        bool isUnlocked = CareerPoints.instance != null && CareerPoints.instance.IsMissionUnlocked(missionType);
        if (UnlockedVisuals != null)
        {
            foreach (GameObject visual in UnlockedVisuals)
            {
                if (visual != null)
                    visual.SetActive(isUnlocked);
            }
        }
    }

    private void RestoreOriginalText()
    {
        if (StartText != null && !string.IsNullOrEmpty(btnText))
        {
            StopAllCoroutines();
            StartText.text = btnText;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {

        //Debug.Log($"Submited {name}");
    }
}
