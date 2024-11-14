using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textLoading;
    float delayLoad;

    public void Start()
    {
        textLoading.enabled = false;
    }

    public void Update()
    {
        if (delayLoad > 0)
        {
            delayLoad -= Time.deltaTime;
            if (delayLoad < 0)
            {
                SceneManager.LoadSceneAsync("MainScene");
            }
        }
    }

    public void OnStartClick()
    {
        textLoading.enabled = true;
        delayLoad = 0.1f;
    }
}
