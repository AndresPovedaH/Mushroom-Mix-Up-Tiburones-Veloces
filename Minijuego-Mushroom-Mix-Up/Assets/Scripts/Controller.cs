using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Controller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetTheGame()
    {
        SceneManager.LoadScene("Minigame");
        print("Escena atrasada");
        Time.timeScale = 1f;
    }
    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }

}
