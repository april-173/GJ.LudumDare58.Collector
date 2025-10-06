using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource radarAudioSource;
    public AudioClip radarClip;

    private bool subscribed = false;

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
}
