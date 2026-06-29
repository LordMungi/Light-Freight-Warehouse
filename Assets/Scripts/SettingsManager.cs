using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    private SettingsManager() { }
    public static SettingsManager instance { get; private set; }

    [SerializeField] private AudioMixer mixer;

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

    private void Start()
    {
        SetMixerFromPref("MasterVolume");
        SetMixerFromPref("MusicVolume");
        SetMixerFromPref("SFXVolume");
        SetMixerFromPref("UIVolume");
    }

    public float GetMixerValue(string group)
    {
        // Warning: los nombres de grupos ("MasterVolume", etc.) se repiten como strings magicos en SettingsManager y OptionsHandler; convendria definirlos como constantes para evitar typos silenciosos.
        mixer.GetFloat(group, out float value);
        return Mathf.Pow(10f, value / 20f);
    }

    public void SetMasterVolume(float value)
    {
        UpdateAudioMixer("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        UpdateAudioMixer("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        UpdateAudioMixer("SFXVolume", value);
    }

    public void SetUIVolume(float value)
    {
        UpdateAudioMixer("UIVolume", value);
    }

    private void UpdateAudioMixer(string group, float value)
    {
        if (value > 0)
            mixer.SetFloat(group, Mathf.Log10(Mathf.Clamp(value, 0f, 1f)) * 20);
        else 
            mixer.SetFloat(group, -144f);

        PlayerPrefs.SetFloat("Audio" + group, value);
        PlayerPrefs.Save();
    }

    private void SetMixerFromPref(string group)
    {
        float value = PlayerPrefs.GetFloat("Audio" + group);
        if (value > 0)
            mixer.SetFloat(group, Mathf.Log10(Mathf.Clamp(value, 0f, 1f)) * 20);
        else
            // Error: (!) parentesis mal puesto. Mathf.Log10(0.5f * 20) = Log10(10) = 1 dB (volumen casi inalterado), no es el caso "mudo" que esperas para value<=0. En UpdateAudioMixer el caso else pone -144 dB (silencio); aca deberia hacer lo mismo. Ademas, si nunca se guardo la pref, GetFloat devuelve 0 y este else deja todos los grupos en +1 dB en vez de un valor por defecto razonable.
            mixer.SetFloat(group, Mathf.Log10(0.5f * 20));
        // Warning: Debug.Log de produccion olvidado; quitarlo antes de la entrega/build o encerrarlo en #if UNITY_EDITOR.
        Debug.Log(value);
    }
}
