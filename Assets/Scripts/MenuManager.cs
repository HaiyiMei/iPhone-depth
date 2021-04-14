using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;


public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    static void LoadScene(string sceneName)
    {
        LoaderUtility.Initialize();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ClientButtonPressed()
    {
        LoadScene("ClientScene");
    }
    public void ServerButtonPressed()
    {
        LoadScene("ServerScene");
    }

}
