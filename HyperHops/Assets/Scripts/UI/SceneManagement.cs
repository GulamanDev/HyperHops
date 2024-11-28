using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{

    public void SingleScene(string sceneName)
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void MultiplayerScene(string sceneName)
    {
        SceneManager.LoadScene("Lobby");
    }

    public void Quit()
    {
        Application.Quit();


        //FOR TESTING IN EDITOR ONLY
        //UnityEditor.EditorApplication.isPlaying = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
