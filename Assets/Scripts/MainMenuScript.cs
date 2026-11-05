using UnityEngine;
using UnityEngine.SceneManagement;
//The library for transitioning between scenes and such


public class MainMenuScript : MonoBehaviour
{
    public static string firstSceneLoaded;
    public void LoadScene(string sceneName)
    {
        //Tell the built-in Scene Manager to load the specified scene
        SceneManager.LoadScene(sceneName);


    }

   
}
