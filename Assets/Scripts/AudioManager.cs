using UnityEngine;

public class AudioManager : MonoBehaviour 
{
	public AudioClip menuMusic, gameMusic;
	bool fadeIn, fadeOut;
    float desiredVolume; 
	float fadeRate = .5f;

	bool muted = false;
	float unmuteVolume = 1;

	
	public void Initialize() 
	{
		GetComponent<AudioSource>().clip = menuMusic;
	}

	public void FadeIn(float toVolume)
	{
		desiredVolume = toVolume;
		fadeIn = true;
		fadeOut = false;

		if(!GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Play();
	}

	public void SwitchToGameMusic()
	{
		CopyIntoAudio(ref gameMusic);
	}


	public void SwitchToMenuMusic()
	{
		CopyIntoAudio(ref menuMusic);
	}

	
	public void FadeOut(float toVolume)
	{
		desiredVolume = toVolume;
		fadeIn = false;
		fadeOut = true;
	}

	public void Mute()
	{
		muted = true;
		unmuteVolume = GetComponent<AudioSource>().volume;
		GetComponent<AudioSource>().volume = 0;
	}

	public void Play()
	{
		if(muted)
		{
			fadeIn = true;
			desiredVolume = unmuteVolume;
			muted = false;
		}

		if(!GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Play();
	}

	public void Stop()
	{
		GetComponent<AudioSource>().Stop();
	}

	public bool IsMuted()
	{
		return muted;
	}


	public void Run(float deltaTime) 
	{
		if(muted)
            return;

		if(fadeIn)
		{
			if(GetComponent<AudioSource>().volume < desiredVolume)
			{
				GetComponent<AudioSource>().volume += fadeRate * deltaTime;
			}
			else
			{
				GetComponent<AudioSource>().volume = desiredVolume;
				fadeIn = false;
			}
		}
		else if(fadeOut)
		{
			if(GetComponent<AudioSource>().volume > desiredVolume)
			{
				GetComponent<AudioSource>().volume -= fadeRate * deltaTime;
			}
			else
			{
				GetComponent<AudioSource>().volume = desiredVolume;
				fadeOut = false;
				//sound.Stop();  //ONLY IF VOLUME IS ZERO
			}
		}
	}

	void CopyIntoAudio(ref AudioClip newClip)
	{
		if(GetComponent<AudioSource>().clip != newClip)
		{
			bool audioWasPlaying = GetComponent<AudioSource>().isPlaying;
			GetComponent<AudioSource>().clip = newClip;

			if(audioWasPlaying)
                GetComponent<AudioSource>().Play();
		}
	}
}
