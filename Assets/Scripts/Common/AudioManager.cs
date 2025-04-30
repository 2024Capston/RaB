using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;  // AudioMixer 관련 네임스페이스

/// <summary>
/// 배경음악 enum
/// </summary>
public enum BGM
{
    Main,
    Count
}

/// <summary>
/// 효과음 enum
/// </summary>
public enum SFX
{
    ButtonClick,
    Count
}

/*
 * 새롭게 추가하고 싶은 BGM이나 SFX가 있다면
 * Resources/Audio 폴더에 해당 소리를 저장하고 둘 중 해당하는 enum에 저장한 이름으로 등록한다.
 * 현재 따로 Audio가 없기 때문에 IS_DEV를 통해 실제 함수가 실행되지 않게 설정했습니다.
 * PlayBGM, PlaySFX, LoadBGMPlayer, LoadSFXPlayer 함수에 해당 코드가 있으므로 추후 Audio 테스트의 경우
 * 해당 코드들을 제거하거나 IS_DEV = false로 바꿔주세요
 */

/// <summary>
/// Audio를 재생하고 중지하는 Singleton Class
/// </summary>
public class AudioManager : SingletonBehavior<AudioManager>
{
    [SerializeField]
    private Transform _bgm;

    [SerializeField]
    private Transform _sfx;

    [SerializeField]
    private AudioMixer _audioMixer;  // 🎛 AudioMixer 추가

    [SerializeField]
    private AudioMixerGroup _bgmGroup;  // BGM AudioMixerGroup
    
    [SerializeField]
    private AudioMixerGroup _sfxGroup;  // SFX AudioMixerGroup

    private readonly string AUDIO_PATH = "Audio";

    private readonly bool IS_DEV = false;

    private Dictionary<BGM, AudioSource> _bgmPlayer = new Dictionary<BGM, AudioSource>();
    
    private float _masterValue=0;
    private float _bgmValue=0;
    private float _sfxValue=0;
    
    private const string MasterValueKey = "MasterValue";
    private const string BGMValueKey = "BGMValue";
    private const string SFXValueKey = "SFXValue";



    /// <summary>
    /// 현재 재생중인 BGM
    /// </summary>
    private AudioSource _currentBGMSource;

    private Dictionary<SFX, AudioSource> _sfxPlayer = new Dictionary<SFX, AudioSource>();

    protected override void Init()
    {
        base.Init();

        LoadBGMPlayer();
        LoadSFXPlayer();

        LoadValues();
    }
    
    public void ApplyAudioMixerValues()
    {
        _audioMixer.SetFloat("Master", _masterValue);
        _audioMixer.SetFloat("BGM", _bgmValue);
        _audioMixer.SetFloat("SFX", _sfxValue);
    
        Debug.Log($"AudioMixer Applied: Master({_masterValue}), BGM({_bgmValue}), SFX({_sfxValue})");
    }

    public void ConnectAudioMixer(string audioMixerName, float value)
    {
        _audioMixer.SetFloat(audioMixerName, value);
    }
    
    private void LoadValues()
    {
        if (PlayerPrefs.HasKey(MasterValueKey))
        {
            _masterValue = PlayerPrefs.GetFloat(MasterValueKey);
        }
        if (PlayerPrefs.HasKey(BGMValueKey))
        {
            _bgmValue = PlayerPrefs.GetFloat(BGMValueKey);
        }
        if (PlayerPrefs.HasKey(SFXValueKey))
        {
            _sfxValue = PlayerPrefs.GetFloat(SFXValueKey);
        }
    }
    
    private void SaveValues()
    {
        PlayerPrefs.SetFloat(MasterValueKey, _masterValue);
        PlayerPrefs.SetFloat(BGMValueKey, _bgmValue);
        PlayerPrefs.SetFloat(SFXValueKey, _sfxValue);
        PlayerPrefs.Save();
    }
    
    public float GetValue(string Parameter)
    {
        LoadValues();
        return Parameter switch
        {
            "Master" => _masterValue,
            "BGM" => _bgmValue,
            "SFX" => _sfxValue,
            ""=> 0,
        };
    }

    public void SetValue(float master, float bgm, float sfx)
    {
        _masterValue = master;
        _bgmValue = bgm;
        _sfxValue = sfx;
        
        SaveValues();
        
        ApplyAudioMixerValues();
    }
    
    /// <summary>
    /// BGM enum에 해당하는 BGM을 재생한다.
    /// </summary>
    /// <param name="bgm"></param>
    public void PlayBGM(BGM bgm)
    {
        if (IS_DEV) return;

        // 현재 재생 중인 음원이 있으면 해당 음원의 재생을 중지한다.
        if (_currentBGMSource)
        {
            _currentBGMSource.Stop();
            _currentBGMSource = null;
        }

        // bgm에 해당하는 음원이 없으면 return 한다.
        if (!_bgmPlayer.ContainsKey(bgm))
        {
            Logger.LogError($"Invaild clip name. {bgm}");
            return;
        }

        // 현재 재생 중인 음원을 bgm으로 바꾼다.
        _currentBGMSource = _bgmPlayer[bgm];
        _currentBGMSource.Play();
    }

    /// <summary>
    /// BGM 재생을 일시 중지한다.
    /// </summary>
    public void PauseBGM()
    {
        if (_currentBGMSource)
        {
            _currentBGMSource.Pause();
        }
    }

    /// <summary>
    /// BGM 재생을 재개한다.
    /// </summary>
    public void ResumeBGM()
    {
        if (_currentBGMSource)
        {
            _currentBGMSource.UnPause();
        }
    }

    /// <summary>
    /// BGM 재생을 끝낸다.
    /// </summary>
    public void StopBGM()
    {
        if (_currentBGMSource)
        {
            _currentBGMSource.Stop();
        }
    }

    /// <summary>
    /// SFX enum에 해당하는 SFX 재생한다.
    /// </summary>
    /// <param name="sfx"></param>
    public void PlaySFX(SFX sfx)
    {
        if (IS_DEV) return;

        if (!_sfxPlayer.ContainsKey(sfx))
        {
            Logger.LogError($"Invalid clip name. {sfx}");
            return;
        }

        _sfxPlayer[sfx].Play();
    }

    /// <summary>
    /// 볼륨을 끈다.
    /// </summary>
    public void Mute()
    {
        foreach (AudioSource ac in _bgmPlayer.Values)
        {
            ac.volume = 0;
        }

        foreach (AudioSource ac in _sfxPlayer.Values)
        {
            ac.volume = 0;
        }
    }

    /// <summary>
    /// 볼륨을 킨다.
    /// </summary>
    public void UnMute()
    {
        foreach (AudioSource ac in _bgmPlayer.Values)
        {
            ac.volume = 1;
        }

        foreach (AudioSource ac in _sfxPlayer.Values)
        {
            ac.volume = 1;
        }

    }

    /// <summary>
    /// Resources/Audio에 저장된 BGM을 불러와 _bgmPlayer에 저장한다.
    /// </summary>
    private void LoadBGMPlayer()
    {
        if (IS_DEV) return;

        for (int i = 0; i < (int)BGM.Count; i++)
        {
            string audioName = ((BGM)i).ToString();
            string audioPath = $"{AUDIO_PATH}/{audioName}";

            AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
            if (audioClip == null)
            {
                Logger.LogError($"{audioName} clip has does not exist");
                continue;
            }

            GameObject audioObject = new GameObject(audioName);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.transform.parent = _bgm;

            // 🎛 AudioSource에 AudioMixerGroup 연결
            audioSource.outputAudioMixerGroup = _bgmGroup;

            _bgmPlayer[(BGM)i] = audioSource;
        }
    }

    /// <summary>
    /// Resources/Audio에 저장된 SFX을 불러와 _sfxPlayer에 저장한다.
    /// </summary>
    private void LoadSFXPlayer()
    {
        if (IS_DEV) return;

        for (int i = 0; i < (int)SFX.Count; i++)
        {
            string audioName = ((SFX)i).ToString();
            string audioPath = $"{AUDIO_PATH}/{audioName}";

            AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
            if (audioClip == null)
            {
                Logger.LogError($"{audioName} clip has does not exist");
                continue;
            }

            GameObject audioObject = new GameObject(audioName);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.transform.parent = _sfx;

            // 🎛 AudioSource에 AudioMixerGroup 연결
            audioSource.outputAudioMixerGroup = _sfxGroup;

            _sfxPlayer[(SFX)i] = audioSource;
        }
    }
}
