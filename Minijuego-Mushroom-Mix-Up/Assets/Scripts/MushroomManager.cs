using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MushroomManager : MonoBehaviour
{
    public GameObject[] mushrooms; 
    private string[] colors = { "Black", "Lightblue", "Pink", "Yellow", "Red", "Green" }; 
    public string safeColor;
    public float dropSpeed = 2f; 
    public float riseSpeed = 3f; 
    public float resetDelay = 2f; 
    public Transform player; 
    public float roundDelay = 3f; 
    public GameObject water; 
    public GameObject gameOverUI; 
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI scoreText; 
    public GameObject winUI; 
    private Vector3[] initialPositions; 
    private bool isGameOver = false; 
    private Color originalColor; 
    private int score = 0; 
    private float timer = 180f; 
    public AudioSource audioSource; 
    public AudioClip gameOverSound;
    public AudioClip winSound;

    void Start()
    {
        if (mushrooms.Length == 0)
        {
            Debug.LogError("No se han asignado hongos al array en el script MushroomManager.");
            return;
        }

        initialPositions = new Vector3[mushrooms.Length];
        for (int i = 0; i < mushrooms.Length; i++)
        {
            initialPositions[i] = mushrooms[i].transform.position;
        }

        StartNextRound();
    }

    void Update()
    {
        if (isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            WinGame();
        }

        UpdateTimerText();
    }

    void StartNextRound()
    {
        score++;
        UpdateScoreText();

        SetRandomSafeColor();

        StartCoroutine(BlinkSafeMushroom());

        Invoke(nameof(UpdateMushrooms), 1.5f);
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "" + score;
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void SetRandomSafeColor()
    {
        safeColor = colors[Random.Range(0, colors.Length)];
        Debug.Log($"Nuevo color seguro seleccionado: {safeColor}");
    }

    private System.Collections.IEnumerator BlinkSafeMushroom()
    {
        bool isBlinking = true;

       
        GameObject safeMushroom = GetSafeMushroom();
        Renderer mushroomRenderer = safeMushroom.GetComponent<Renderer>();
        originalColor = mushroomRenderer.material.color;

        while (isBlinking)
        {
            mushroomRenderer.material.color = Color.green;
            yield return new WaitForSeconds(0.5f);

            mushroomRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

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
        if (isGameOver) return;

        foreach (GameObject mushroom in mushrooms)
        {
            if (mushroom.CompareTag(safeColor))
            {
                Debug.Log($"{mushroom.name} es seguro, no baja.");
                continue;
            }

            StartCoroutine(DropMushroom(mushroom));
        }

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

        mushroom.transform.position = targetPosition;
    }

    private void CheckPlayerPosition()
    {
        GameObject safeMushroom = GetSafeMushroom();

        if (safeMushroom == null)
        {
            Debug.LogError("No se encontró el hongo seguro en el array. Verifica las etiquetas.");
            return;
        }

        if (IsPlayerOnSafeMushroom(safeMushroom))
        {
            Debug.Log("El jugador está a salvo. Reiniciando ronda...");
            ResetMushrooms();
        }
        else
        {
            Debug.Log("El jugador ha caído al agua. Fin del juego.");
            EndGame();
        }
    }

    private bool IsPlayerOnSafeMushroom(GameObject safeMushroom)
    {
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
        if (isGameOver) return;

        Debug.Log("Reiniciando posición de los hongos...");
        StopAllCoroutines();

        foreach (GameObject mushroom in mushrooms)
        {
            StartCoroutine(RiseMushroom(mushroom));
        }

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

        mushroom.transform.position = targetPosition;
    }

    private System.Collections.IEnumerator WaitForRisingComplete()
    {
        yield return new WaitForSeconds(roundDelay);
        StartNextRound();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el jugador tocó el agua.
        if (other.CompareTag("Water"))
        {
            Debug.Log("¡El jugador tocó el agua! Fin del juego.");
            EndGame();
        }
    }

    private void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        // Reproducir el sonido de fin de juego
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound); // Reproduce el sonido una vez
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    private void WinGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound); // Reproduce el sonido una vez
        }

        if (winUI != null)
        {
            winUI.SetActive(true);
        }

        Debug.Log("¡El jugador ganó el juego!");
    }
}