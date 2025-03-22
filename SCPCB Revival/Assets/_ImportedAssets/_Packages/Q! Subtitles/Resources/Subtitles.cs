using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Subtitles : MonoBehaviour
{
    public static Subtitles subtitles;
    [Tooltip("System and CC prefab, please do not modify these variables.")] public GameObject system, prefab;
    [Header("Settings")]
    [Tooltip("Hide the subtitle system in hierarchy")] public bool hideObject;
    [Tooltip("Speed of the type effect. This value can be updated in runtime.")] public float typeSpeed = 0;

    /// <summary>
    /// Global method. Shows a close caption (subtitle). Only valid when only one instance of the system is in the scene.
    /// </summary>
    /// <param name="text">Text to show.</param>
    /// <param name="duration">Amount of seconds to show the text.</param>
    /// <param name="effect">Effect of the text.</param>
    /// <param name="size">Size of the font.</param>
    /// <param name="font">Font to use.</param>
    /// <param name="clip">Clip to play alongside subtitle. You may use clip.length in duration.</param>
    public static void Show(string text, float duration = 5f, SubtitleEffect effect = SubtitleEffect.Fade, int size = 20, Font font = null, AudioClip clip = null)
    {
        if (subtitles == null) { Debug.LogError("Subtitle system not detected"); return; }
        else if (FindObjectsOfType<Subtitles>().Length != 1)
        { Debug.LogError("Cannot use global methods as more than one system was detected"); return; }
        else subtitles.ShowThis(text, duration, effect, size, font, clip);
    }

    /// <summary>
    /// Referenced method. Shows a close caption (subtitle) on this specific system.
    /// </summary>
    /// <param name="text">Text to show.</param>
    /// <param name="duration">Amount of seconds to show the text.</param>
    /// <param name="effect">Effect of the text.</param>
    /// <param name="size">Size of the font.</param>
    /// <param name="font">Font to use.</param>
    /// <param name="clip">Clip to play alongside subtitle. You may use clip.length in duration.</param>
    public void ShowThis(string text, float duration, SubtitleEffect effect, int size, Font font, AudioClip clip)
    {
        GameObject dialogue = Instantiate(prefab, m_Transform);
        Text textComponent = dialogue.GetComponentInChildren<Text>();
        textComponent.fontSize = size;
        if (font != null) textComponent.font = font;
        if (clip != null) { AudioSource source = dialogue.AddComponent<AudioSource>(); source.clip = clip; source.Play(); }
        switch (effect)
        {
            case SubtitleEffect.None: textComponent.text = text; Destroy(dialogue, duration); break;
            case SubtitleEffect.Fade: textComponent.text = text; StartCoroutine(Fade(dialogue, duration)); break;
            case SubtitleEffect.Type: StartCoroutine(Type(text, textComponent, duration)); break;
            case SubtitleEffect.Both: StartCoroutine(FadeType(dialogue, text, textComponent, duration)); break;
        }
    }

    #region CORE
    private GameObject m_System;
    private Transform m_Transform;
    private Image m_Image;

    private void Start()
    {
        if (m_System == null)
        {
            m_System = Instantiate(system, transform);
            if (hideObject) m_System.gameObject.hideFlags = HideFlags.HideInHierarchy;
            m_Image = m_System.GetComponent<Image>();
            m_Transform = m_System.transform;
            m_Transform.transform.SetAsFirstSibling();
        }

        if (subtitles != null) { Debug.LogWarning("Multiple subtitle systems detected, global static methods will not work"); return; }
        else subtitles = this;

    }

    private void Update()
    {
        if (m_Transform.childCount == 0) m_Image.enabled = false;
        else m_Image.enabled = true;
    }

    private IEnumerator Fade(GameObject dialogue, float duration)
    {
        Animator animator = dialogue.GetComponent<Animator>();
        animator.Play("In");
        yield return new WaitForSeconds(duration);
        animator.Play("Out");
        Destroy(dialogue, 1f);
    }

    private IEnumerator Type(string text, Text textComponent, float duration)
    {
        textComponent.text = "";
        foreach (char letter in text.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
        Destroy(textComponent.transform.parent.gameObject, duration);
    }

    private IEnumerator FadeType(GameObject dialogue, string text, Text textComponent, float duration)
    {
        Animator animator = dialogue.GetComponent<Animator>();
        animator.Play("In");
        textComponent.text = "";
        foreach (char letter in text.ToCharArray())
        {
            textComponent.text += letter;
            yield return typeSpeed == 0 ? null : new WaitForSeconds(typeSpeed);
        }
        yield return new WaitForSeconds(duration);
        animator.Play("Out");
        Destroy(dialogue, 1f);
    }
    #endregion
}
