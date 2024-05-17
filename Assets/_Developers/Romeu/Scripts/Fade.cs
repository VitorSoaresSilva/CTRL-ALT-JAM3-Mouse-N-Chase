using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : Singleton<Fade>
{
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private Canvas fadeCanvas;
    [field: SerializeField, Range(0, 1)] public float animationSpeed { get; private set; } = 0.75f;

    // Start is called before the first frame update
    void Start()
    {
        if (fadeAnimator == null)
            fadeAnimator = GetComponent<Animator>();

        if (fadeCanvas == null)
            fadeCanvas = GetComponent<Canvas>();

        fadeCanvas.enabled = true;
        fadeAnimator.speed = animationSpeed;

        FadeOut();
    }

    public void FadeIn() // Deixa a tela preta, sem o círculo
    {
        fadeAnimator.SetTrigger("trClose");
    }

    public void FadeOut() // Deixa a tela visível, com o círculo
    {
        fadeAnimator.SetTrigger("trOpen");
    }
}
