using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Image image;
    public PressProgressButton dive;
    public PressProgressButton quit;
    public AudioSource buttonAudioSource;
    public AudioClip buttonAudioClip;

    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;
    }

    private void Start()
    {
        image.enabled = false;
        dive.onHoldComplete += HandleDive;
        quit.onHoldComplete += HandleQuit;
        StartCoroutine(StartMainMenu());
    }

    private IEnumerator StartMainMenu()
    {
        image.enabled = true;
        int t = 24;
        for (int i = t; i >= 0; i--) 
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, (float)i / (float)t);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        image.enabled = false;
    }

    private void Update()
    {
        if (keyboard[Key.Escape].wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }

    private void HandleDive()
    {
        StartCoroutine(Dive());
    }

    private IEnumerator Dive()
    {
        PlayButtonAudio();
        image.enabled = true;
        int t = 24;
        for (int i = 0; i <= t; i++)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, (float)i / (float)t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("SampleScene");
    }

    private void HandleQuit()
    {
        StartCoroutine(Quit());
    }

    private IEnumerator Quit()
    {
        PlayButtonAudio();
        yield return new WaitForSeconds(0.2f);
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit();
//#endif
    }

    private void PlayButtonAudio()
    {
        buttonAudioSource.PlayOneShot(buttonAudioClip, 0.5f);
    }
}
