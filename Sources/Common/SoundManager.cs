using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    private Transform _soundGroup = null;
    private AudioSource _bgmSource = null;

    private const string _soundPath = "Prefabs/Sound/";

    private readonly Dictionary<Sound, AudioClip> _clipResources = new Dictionary<Sound, AudioClip>();
    private readonly Stack<AudioSource> _audioSourcePool = new Stack<AudioSource>();

    public void Init(AudioSource bgmSource)
    {
        GameObject groupObj = new GameObject { name = "SoundGroup" };
        _soundGroup = groupObj.transform;
        _bgmSource = bgmSource;
    }
    public void Release()
    {
        _bgmSource.volume = 0;

        for (int i = 0; i < _soundGroup.childCount; i++)
        {
            Object.Destroy(_soundGroup.GetChild(i).gameObject);
        }
        _audioSourcePool.Clear();
    }
    public void PlayBGM(Sound sound)
    {
        _bgmSource.clip = GetClipResource(sound);
        _bgmSource.Play();

        Main.GameMain._coroutineHandler.StartCoroutineCustom(FadeInVolume());
    }

    private AudioClip GetClipResource(Sound sound)
    {
        if (_clipResources.ContainsKey(sound)) return _clipResources[sound];
        else
        {
            AudioClip clip = Resources.Load<AudioClip>(_soundPath + sound.ToString());
            _clipResources.Add(sound, clip);
            return clip;
        }
    }
    public void TurnOffBGM()
    {
        Main.GameMain._coroutineHandler.StartCoroutineCustom(FadeOutVolume());
    }
    private IEnumerator FadeInVolume()
    {
        while(_bgmSource.volume < Const.BGM_VOLUME)
        {
            _bgmSource.volume += Time.deltaTime * 0.5f;
            yield return null;
        }

        _bgmSource.volume = Const.BGM_VOLUME;
    }
    private IEnumerator FadeOutVolume()
    {
        while (_bgmSource.volume > 0)
        {
            _bgmSource.volume -= Time.deltaTime * 0.5f;
            yield return null;
        }

        _bgmSource.volume = 0;
    }
    public void PlaySFX(Sound sound)
    {
        AudioSource source;
        if (_audioSourcePool.Count == 0)
        {
            GameObject soundObj = new GameObject();
            soundObj.transform.SetParent(_soundGroup);
            source = soundObj.AddComponent<AudioSource>();
        }
        else
        {
            source = _audioSourcePool.Pop();
            source.gameObject.SetActive(true);
        }

        source.clip = GetClipResource(sound);
        source.Play();

        Main.GameMain._coroutineHandler.StartCoroutineCustom(ReturnSFX(source));
    }

    private IEnumerator ReturnSFX(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);

        if (source == null) yield break;
        source.gameObject.SetActive(false);

        _audioSourcePool.Push(source);
    }
}
