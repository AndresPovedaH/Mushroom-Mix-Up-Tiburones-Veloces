using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomManager : MonoBehaviour
{
    public GameObject[] mushrooms; // Array con todos los hongos.
    private string[] colors = { "Black", "Lightblue", "Pink", "Yellow", "Red", "Green" }; // Colores disponibles.
    public string safeColor; // Color seguro actual.
    public float dropSpeed = 2f; // Velocidad de caída de los hongos.
    public float riseSpeed = 3f; // Velocidad de subida de los hongos.
    public float resetDelay = 2f; // Tiempo antes de que los hongos vuelvan a subir.
    public Transform player; // Referencia al jugador.
    public float roundDelay = 3f; // Retraso entre rondas para asegurar que la animación termine.
    public GameObject water; // El objeto que representa el agua.
    public GameObject gameOverUI; // UI que aparece cuando el juego termina (si tienes una).

    private Vector3[] initialPositions; // Posiciones originales de los hongos.
    private bool isGameOver = false; // Variable que indica si el juego ha terminado.
    private Color originalColor; // Color original del hongo seguro.

    void Start()
    {
        // Verifica que todos los hongos estén asignados.
        if (mushrooms.Length == 0)
        {
            Debug.LogError("No se han asignado hongos al array en el script MushroomManager.");
            return;
        }

        // Guarda las posiciones originales de los hongos.
        initialPositions = new Vector3[mushrooms.Length];
        for (int i = 0; i < mushrooms.Length; i++)
        {
            initialPositions[i] = mushrooms[i].transform.position;
        }

        // Inicia la primera ronda.
        StartNextRound();
    }

    void StartNextRound()
    {
        // Selecciona un nuevo color seguro.
        SetRandomSafeColor();

        // Cambia el color del hongo seguro.
        StartCoroutine(BlinkSafeMushroom());

        // Baja los hongos no seguros después de un corto retraso.
        Invoke(nameof(UpdateMushrooms), 1.5f);
    }

    public void SetRandomSafeColor()
    {
        safeColor = colors[Random.Range(0, colors.Length)];
        Debug.Log($"Nuevo color seguro seleccionado: {safeColor}");
    }

    // Corrutina que hace que el hongo seguro parpadee.
    private System.Collections.IEnumerator BlinkSafeMushroom()
    {
        bool isBlinking = true;

        // Obtén el hongo seguro y su color original.
        GameObject safeMushroom = GetSafeMushroom();
        Renderer mushroomRenderer = safeMushroom.GetComponent<Renderer>();
        originalColor = mushroomRenderer.material.color;

        while (isBlinking)
        {
            // Cambia el color a verde brillante.
            mushroomRenderer.material.color = Color.green;
            yield return new WaitForSeconds(0.5f);

            // Vuelve al color original.
            mushroomRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Devuelve el hongo seguro basado en el color.
    private GameObject GetSafeMushroom()
    {
        foreach (GameObject mushroom in mushrooms)
        {
            if (mushroom.CompareTag(safeColor))
            {
                return mushroom;
            }
        }

        return null;
    }

    public void UpdateMushrooms()
    {
        if (isGameOver) return; // Si el juego ha terminado, no hacemos nada.

        foreach (GameObject mushroom in mushrooms)
        {
            if (mushroom.CompareTag(safeColor))
            {
                Debug.Log($"{mushroom.name} es seguro, no baja.");
                continue; // Este hongo es seguro.
            }

            // Los hongos no seguros bajan.
            StartCoroutine(DropMushroom(mushroom));
        }

        // Verifica la posición del jugador tras un corto tiempo.
        Invoke(nameof(CheckPlayerPosition), 1.5f);
    }

    private System.Collections.IEnumerator DropMushroom(GameObject mushroom)
    {
        Vector3 initialPosition = mushroom.transform.position;
        Vector3 targetPosition = initialPosition + Vector3.down * 5f;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            mushroom.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * dropSpeed;
            yield return null;
        }

        mushroom.transform.position = targetPosition; // Asegura la posición final.
    }

    private void CheckPlayerPosition()
    {
        GameObject safeMushroom = GetSafeMushroom();

        if (safeMushroom == null)
        {
            Debug.LogError("No se encontró el hongo seguro en el array. Verifica las etiquetas.");
            return;
        }

        // Verifica si el jugador está sobre el hongo seguro usando Raycast.
        if (IsPlayerOnSafeMushroom(safeMushroom))
        {
            Debug.Log("El jugador está a salvo. Reiniciando ronda...");
            ResetMushrooms(); // Asegura que se reinicien los hongos.
        }
        else
        {
            Debug.Log("El jugador ha caído al agua. Fin del juego.");
            EndGame(); // Llama a la función para finalizar el juego.
        }
    }

    private bool IsPlayerOnSafeMushroom(GameObject safeMushroom)
    {
        // Dispara un rayo hacia abajo desde el centro del jugador.
        Ray ray = new Ray(player.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            Debug.Log($"El jugador está sobre: {hit.collider.gameObject.name}");
            return hit.collider.gameObject == safeMushroom;
        }

        Debug.Log("El jugador no está sobre ningún hongo.");
        return false;
    }

    private void ResetMushrooms()
    {
        if (isGameOver) return; // Si el juego ha terminado, no hacemos nada.

        Debug.Log("Reiniciando posición de los hongos...");

        // Detiene todas las corrutinas activas para evitar conflictos.
        StopAllCoroutines();

        // Llama a la animación de subida para cada hongo.
        foreach (GameObject mushroom in mushrooms)
        {
            StartCoroutine(RiseMushroom(mushroom));
        }

        // Espera a que todas las animaciones de subida terminen antes de iniciar la siguiente ronda.
        StartCoroutine(WaitForRisingComplete());
    }

    private System.Collections.IEnumerator RiseMushroom(GameObject mushroom)
    {
        Vector3 initialPosition = mushroom.transform.position;
        Vector3 targetPosition = initialPositions[System.Array.IndexOf(mushrooms, mushroom)];

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            mushroom.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * riseSpeed;
            yield return null;
        }

        mushroom.transform.position = targetPosition; // Asegura la posición final.
    }

    private System.Collections.IEnumerator WaitForRisingComplete()
    {
        // Espera un tiempo para que las animaciones de subida terminen.
        yield return new WaitForSeconds(roundDelay);

        // Inicia la siguiente ronda después del retraso.
        StartNextRound();
    }

    // Detecta cuando el jugador toca el agua.
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto con el que el jugador colisiona es el agua.
        if (other.CompareTag("Water"))
        {
            Debug.Log("¡El jugador tocó el agua! Fin del juego.");
            EndGame(); // Finaliza el juego.
        }
    }

    // Finaliza el juego (puedes agregar más lógica aquí).
    private void EndGame()
    {
        if (isGameOver) return; // Si el juego ya está terminado, no hacemos nada.

        isGameOver = true; // Establece que el juego ha terminado.

        // Aquí puedes hacer que aparezca una UI de fin de juego o realizar cualquier otra acción.
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // Muestra una interfaz de fin de juego.
        }

        // Detén el juego.
        Time.timeScale = 0f; // Detiene el tiempo del juego (puedes usar esto para pausar el juego).
    }
}