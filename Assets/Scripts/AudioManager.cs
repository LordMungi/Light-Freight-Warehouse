using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private AudioManager() { }

    public static AudioManager instance { get; private set; }

    [SerializeField] AudioSource ButtonClickSFX;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetSounds();
    }

    private void SetSounds()
    {
        // Error: Resources.FindObjectsOfTypeAll<Button>() devuelve TODOS los Button cargados, incluyendo prefabs y objetos inactivos/de otras escenas, no solo los de la escena actual. Esto agrega listeners a botones que no corresponden.
        // Warning: ademas se llama en cada sceneLoaded y nunca se hace RemoveListener, por lo que un mismo boton puede acumular varios listeners y reproducir el SFX multiples veces. Mejor referenciar los botones explicitamente o suscribir/desuscribir de forma controlada.
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button but in buttons)
        {
            but.onClick.AddListener(PlayButtonClickSFX);
        }
    }

    private void PlayButtonClickSFX()
    {
        ButtonClickSFX.Play();
    }
}
