using UnityEngine;

public class SoundEffectManager : MonoBehaviour 
{
	public enum PitchToPlay : byte
	{
		Low = 1,
		Med = 2,
		High = 3
	}

    private GameManager _gameManager;

    [SerializeField]
	private AudioClip _explosion1;

    [SerializeField]
    private AudioClip _explosion2;

    [SerializeField]
    private AudioClip _explosion3;

    [SerializeField]
    private AudioClip _gameOver;

    [SerializeField]
    private AudioClip _back;

    [SerializeField]
    private AudioClip _select;


    private int _lastExplosion = 1;
    private int _lastPitch = 1;

    //TODO: think I only need one audio source

    [SerializeField]
    private AudioSource _lowPitch;

    [SerializeField]
    private AudioSource _medPitch;

    [SerializeField]
    private AudioSource _highPitch;

    private float _defaultVolume = .5f;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

	public PitchToPlay NextPitch()
	{
		//0 or 1
		if(Random.Range(0,2) == 0)
		{
			if(++_lastPitch > 3)
                _lastPitch = 1;
		}
		else
		{
			if(--_lastPitch < 1)
                _lastPitch = 3;
		}

		return (PitchToPlay)_lastPitch;
	}

	public void ResetVolume()
	{
		_lowPitch.volume = _defaultVolume;
		_medPitch.volume = _defaultVolume;
		_highPitch.volume = _defaultVolume;
	}

	public void PlayExplosionAtPitch(PitchToPlay pitch)
	{
		//0 or 1
		if(Random.Range(0,2) == 0)
		{
			if(++_lastExplosion > 3)
                _lastExplosion = 1;
		}
		else
		{
			if(--_lastExplosion < 1)
                _lastExplosion = 3;
		}

		switch (_lastExplosion)
		{
			case 1:
				_playAtPitch(pitch, ref _explosion1);
				break;
			case 2:
				_playAtPitch(pitch, ref _explosion2);
				break;
			case 3:
				_playAtPitch(pitch, ref _explosion3);
				break;
		}
	}

	private void _playAtPitch(PitchToPlay pitch, ref AudioClip clip)
	{
		switch(pitch)
		{
		case PitchToPlay.Low:
			PlaySoundOnce(_lowPitch, clip);
			break;
		case PitchToPlay.Med:
			PlaySoundOnce(_medPitch, clip);
			break;
		case PitchToPlay.High:
			PlaySoundOnce(_highPitch, clip);
			break;
		}
	}

	public void PlayGameOver()
	{
		_medPitch.volume = 1;
		PlaySoundOnce(_medPitch, _gameOver);
	}

	public void PlaySelect()
	{
		_medPitch.volume = 1;
		PlaySoundOnce(_medPitch, _select);
	}

	public void PlayBack()
	{
		_medPitch.volume = 1;
		PlaySoundOnce(_medPitch, _back);
	}

	private void PlaySoundOnce(AudioSource source, AudioClip clip)
	{
		if(_gameManager.SfxOn)
		{
			source.PlayOneShot(clip);
		}
	}
}
