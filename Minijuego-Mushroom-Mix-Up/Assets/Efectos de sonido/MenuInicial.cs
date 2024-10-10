using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public float delayTime = 2f; // Tiempo de retraso antes de cambiar de escena

    public void Jugar()
    {
        StartCoroutine(WaitAndPlay()); // Inicia la corutina para esperar antes de cambiar de escena
    }

    IEnumerator WaitAndPlay()
    {
        yield return new WaitForSeconds(delayTime); // Espera el tiempo especificado
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Cambia a la siguiente escena
    }

    public void Salir()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }

    public float transitionDelay = 2f; // Tiempo de retraso antes del cambio

}
