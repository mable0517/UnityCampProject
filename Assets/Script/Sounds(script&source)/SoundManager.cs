using UnityEngine;

[System.Serializable]
public class Sound
{
    public string soundName;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("사운드 클립 등록")]
    public Sound[] bgmSounds;
    public Sound[] sfxSounds;
    public Sound walkSound;

    [Header("오디오 소스")]
    public AudioSource bgmPlayer;
    public AudioSource sfxPlayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(string name)
    {
        for (int i = 0; i < bgmSounds.Length; i++)
        {
            if (bgmSounds[i].soundName == name)
            {
                if (bgmPlayer.clip == bgmSounds[i].clip) return;

                bgmPlayer.clip = bgmSounds[i].clip;
                bgmPlayer.Play();
                return;
            }
        }
        Debug.LogWarning("BGM을 찾을 수 없습니다: " + name);
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    // [업그레이드] randomPitch 값을 true로 주면 소리 높낮이가 살짝 랜덤하게 변합니다.
    public void PlaySFX(string name, bool randomPitch = false)
    {
        for (int i = 0; i < sfxSounds.Length; i++)
        {
            if (sfxSounds[i].soundName == name)
            {
                if (randomPitch)
                {
                    sfxPlayer.pitch = Random.Range(0.85f, 1.15f); // 원래 소리보다 약간 낮거나 높게
                }
                else
                {
                    sfxPlayer.pitch = 1f; // 원본 소리
                }

                sfxPlayer.PlayOneShot(sfxSounds[i].clip);

                // 다음 재생을 위해 피치를 원상태로 돌려놓음 (PlayOneShot이 끝난 후 영향을 주지 않기 위해)
                Invoke("ResetPitch", 0.5f);
                return;
            }
        }
        Debug.LogWarning("SFX를 찾을 수 없습니다: " + name);
    }

    public void PlayWalk()
    {
        if (walkSound.clip != null)
        {
            sfxPlayer.pitch = Random.Range(0.9f, 1.1f);
            sfxPlayer.PlayOneShot(walkSound.clip);
            Invoke("ResetPitch", 0.2f);
        }
    }

    // 피치 초기화용 내부 함수
    private void ResetPitch()
    {
        sfxPlayer.pitch = 1f;
    }
}