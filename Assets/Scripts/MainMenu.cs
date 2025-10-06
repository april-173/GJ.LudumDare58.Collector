using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Image image;
    public PressProgressButton dive;
    public PressProgressButton quit;

    private void Start()
    {
        image.enabled = false;
        dive.onHoldComplete += HandleDive;
        quit.onHoldComplete += HandleQuit;
    }

    private void HandleDive()
    {
        StartCoroutine(Dive());
    }

    private IEnumerator Dive()
    {
        image.enabled = true;
        int t = 16;
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("QuitGame() called — 游戏退出请求已触发。");
    }
}
