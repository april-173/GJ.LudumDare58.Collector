using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource radarAudioSource;
    public AudioClip radarClip;
    public AudioSource buttonAudioSource;
    public AudioClip buttonAudioClip;

    private bool subscribed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        TrySubscribeInput();
    }

    private void Start()
    {
        TrySubscribeInput();
    }

    private void OnDisable()
    {
        TryUnsubscribeInput();
    }

    #region < ÊÂ¼þ¶©ÔÄ >
    private void TrySubscribeInput()
    {
        if (subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPing += HandlePingEvent;
            subscribed = true;
        }
    }

    private void TryUnsubscribeInput()
    {
        if (!subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPing -= HandlePingEvent;
        }
        subscribed = false;
    }

    private void HandlePingEvent()
    {
        if (!radarAudioSource.isPlaying)
            StartCoroutine(PingAudio());
    }

    private IEnumerator PingAudio()
    {
        radarAudioSource.Play();

        yield return new WaitForSeconds(2f);

        radarAudioSource.Stop();
    }
    #endregion

    public void PlayButtonSound()
    {
        buttonAudioSource.PlayOneShot(buttonAudioClip,0.2f);
    }
}
