using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	private void Awake()
	{
		AudioManager.me = this;
		this.InitializeAudioSources();
	}

	private void Start()
	{
		this.ambienceSource = this.GetSource(null, true);
		this.ambienceSource.spatialBlend = 0f;
		this.ambienceSource.clip = this.DetermineCurrentAmbience();
		this.ambienceSource.volume = this.DetermineCurrentAmbienceVolume();
		this.ambienceSource.reverbZoneMix = 0f;
		this.ambienceSource.outputAudioMixerGroup = this.MusicGroup;
		this.ambienceSource.loop = true;
		this.ambienceSource.Play();
		this.songSource = this.GetSource(null, true);
		this.songSource.spatialBlend = 0f;
		this.songSource.reverbZoneMix = 0f;
		this.songSource.outputAudioMixerGroup = this.MusicGroup;
		this.songSource.volume = 0f;
		this.songSource.time = 0f;
		AudioClipWithVolume audioClipWithVolume = this.DetermineCurrentSong();
		this.songSource.clip = audioClipWithVolume.Clip;
		this.currentSongTargetVolume = audioClipWithVolume.Volume;
		this.songSource.Play();
		this.battleSource = this.GetSource(null, true);
		this.battleSource.spatialBlend = 0f;
		this.battleSource.reverbZoneMix = 0f;
		this.battleSource.outputAudioMixerGroup = this.SfxGroup;
		this.battleSource.volume = 0f;
		this.battleSource.clip = this.BattleLoop;
		this.battleSource.loop = true;
		this.battleSource.Play();
		this.spiritSource = this.GetSource(null, true);
		this.spiritSource.spatialBlend = 0f;
		this.spiritSource.reverbZoneMix = 0f;
		this.spiritSource.outputAudioMixerGroup = this.SfxGroup;
		this.spiritSource.volume = 0f;
		this.spiritSource.clip = this.SpiritLoop;
		this.spiritSource.loop = true;
		this.spiritSource.Play();
	}

	public void SkipSong()
	{
		this.songSource.time = this.songSource.clip.length - 5.1f;
	}

	private AudioClipWithVolume DetermineCurrentSong()
	{
		GameBoard curBoard = WorldManager.instance.GetCurrentBoardSafe();
		if (curBoard.BoardOptions.Songs.Count == 1)
		{
			return curBoard.BoardOptions.Songs.FirstOrDefault<AudioClipWithVolume>();
		}
		List<AudioClipWithVolume> list = curBoard.BoardOptions.Songs.FindAll((AudioClipWithVolume x) => !this.playedSongs.Contains(x.Clip));
		if (list.Count < 1)
		{
			this.playedSongs.RemoveAll((AudioClip x) => curBoard.BoardOptions.Songs.Any<AudioClipWithVolume>((AudioClipWithVolume v) => v.Clip == x) && x != this.songSource.clip);
			list = curBoard.BoardOptions.Songs.FindAll((AudioClipWithVolume x) => !this.playedSongs.Contains(x.Clip));
		}
		AudioClipWithVolume audioClipWithVolume = ((list.Count > 0) ? list.Choose<AudioClipWithVolume>() : null);
		if (audioClipWithVolume != null && !this.playedSongs.Contains(audioClipWithVolume.Clip))
		{
			this.playedSongs.Add(audioClipWithVolume.Clip);
		}
		return audioClipWithVolume;
	}

	private AudioClip DetermineCurrentAmbience()
	{
		return WorldManager.instance.GetCurrentBoardSafe().BoardOptions.Ambience;
	}

	private float DetermineCurrentAmbienceVolume()
	{
		return WorldManager.instance.GetCurrentBoardSafe().BoardOptions.AmbienceVolume;
	}

	private bool CurrentSongShouldStop()
	{
		AudioClip currentSong = this.songSource.clip;
		return WorldManager.instance.CurrentBoard != null && !WorldManager.instance.CurrentBoard.BoardOptions.Songs.Any<AudioClipWithVolume>((AudioClipWithVolume x) => x.Clip == currentSong);
	}

	private bool AnyCardInConflict()
	{
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent && gameCard.Combatable != null && gameCard.InConflict)
			{
				return true;
			}
		}
		return false;
	}

	private bool SpiritOnBoard()
	{
		return WorldManager.instance.CardQuery.GetCard<Spirit>() != null;
	}

	private void LateUpdate()
	{
		this.MusicGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log(OptionsScreen.MusicVol) * 20f);
		this.SfxGroup.audioMixer.SetFloat("SfxVolume", Mathf.Log(OptionsScreen.SfxVol) * 20f);
		float num = this.currentSongTargetVolume;
		if (this.songSource.clip != null)
		{
			if (this.songSource.time >= this.songSource.clip.length - 5f || this.CurrentSongShouldStop())
			{
				num = 0f;
				this.newSongTimer += Time.deltaTime;
			}
			if (WorldManager.instance.CardQuery.GetCardCount<Spirit>() > 0)
			{
				num = 0f;
			}
		}
		if (WorldManager.instance.CurrentGameState == WorldManager.GameState.GameOver)
		{
			num = 0f;
		}
		if (this.muteSong)
		{
			num = 0f;
		}
		if (!this.songSource.isPlaying)
		{
			this.newSongTimer += Time.deltaTime;
		}
		this.battleSource.volume = Mathf.Lerp(this.battleSource.volume, this.AnyCardInConflict() ? 0.01f : 0f, Time.unscaledDeltaTime);
		this.spiritSource.volume = Mathf.Lerp(this.spiritSource.volume, this.SpiritOnBoard() ? 0.5f : 0f, Time.unscaledDeltaTime);
		float num2 = 1f;
		if (WorldManager.instance.Time.SpeedUp == 0f)
		{
			num2 = 0.8f;
		}
		else if (WorldManager.instance.Time.SpeedUp == 5f)
		{
			num2 = 1.2f;
		}
		this.songSource.pitch = Mathf.Lerp(this.songSource.pitch, num2, Time.deltaTime * 12f);
		if (this.newSongTimer >= 4f)
		{
			this.newSongTimer = 0f;
			AudioClipWithVolume audioClipWithVolume = this.DetermineCurrentSong();
			this.songSource.clip = audioClipWithVolume.Clip;
			this.currentSongTargetVolume = audioClipWithVolume.Volume;
			this.songSource.time = 0f;
			this.songSource.Play();
		}
		this.songSource.volume = Mathf.Lerp(this.songSource.volume, num, Time.unscaledDeltaTime);
		this.UpdateAmbience();
		for (int i = 0; i < this.sources.Count; i++)
		{
			if (this.targets[i] != null)
			{
				this.sources[i].transform.position = this.targets[i].transform.position;
			}
		}
	}

	private void UpdateAmbience()
	{
		AudioClip audioClip = this.DetermineCurrentAmbience();
		if (this.ambienceSource.clip != audioClip)
		{
			this.ambienceSource.volume = Mathf.Lerp(this.ambienceSource.volume, 0f, Time.unscaledDeltaTime);
			if (this.ambienceSource.volume < 0.001f)
			{
				this.ambienceSource.clip = audioClip;
				this.ambienceSource.Play();
				return;
			}
		}
		else
		{
			this.ambienceSource.volume = Mathf.Lerp(this.ambienceSource.volume, this.DetermineCurrentAmbienceVolume(), Time.unscaledDeltaTime);
		}
	}

	private void InitializeAudioSources()
	{
		for (int i = 0; i < 100; i++)
		{
			GameObject gameObject = new GameObject("Audio " + i.ToString());
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.zero;
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = this.SfxGroup;
			audioSource.loop = false;
			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 0f;
			audioSource.minDistance = 20f;
			audioSource.priority = 10;
			this.sources.Add(audioSource);
			this.targets.Add(null);
		}
	}

	public AudioSource GetSource(Transform target = null, bool claim = false)
	{
		AudioSource audioSource = null;
		for (int i = 0; i < this.sources.Count; i++)
		{
			if (!this.sources[i].isPlaying)
			{
				audioSource = this.sources[i];
				this.targets[i] = target;
				break;
			}
		}
		if (claim)
		{
			this.sources.Remove(audioSource);
		}
		return audioSource;
	}

	public void PlaySound(List<AudioClip> clips, Vector3 pos, float pitch = 1f, float vol = 1f)
	{
		this.PlaySound(clips[Random.Range(0, clips.Count)], pos, pitch, vol);
	}

	public void PlaySound(AudioClip clip, Vector3 pos, float pitch = 1f, float vol = 1f)
	{
		AudioSource source = this.GetSource(null, false);
		if (source == null)
		{
			return;
		}
		source.pitch = pitch;
		source.clip = clip;
		source.volume = vol;
		source.transform.position = pos;
		source.spatialBlend = 1f;
		source.reverbZoneMix = 1f;
		source.bypassListenerEffects = false;
		source.Play();
	}

	public void PlaySound(AudioClip clip, Transform t, float pitch = 1f, float vol = 1f)
	{
		AudioSource source = this.GetSource(t, false);
		if (source == null)
		{
			return;
		}
		source.pitch = pitch;
		source.clip = clip;
		source.volume = vol;
		source.reverbZoneMix = 0f;
		source.transform.localPosition = Vector3.zero;
		source.spatialBlend = 1f;
		source.bypassListenerEffects = false;
		source.Play();
	}

	public void PlaySound2D(List<AudioClip> clips, float pitch = 1f, float vol = 1f)
	{
		this.PlaySound2D(clips[Random.Range(0, clips.Count)], pitch, vol);
	}

	public void PlaySound2D(AudioClip clip, float pitch, float vol)
	{
		AudioSource source = this.GetSource(null, false);
		if (source == null)
		{
			return;
		}
		source.pitch = pitch;
		source.clip = clip;
		source.volume = vol;
		source.reverbZoneMix = 0f;
		source.spatialBlend = 0f;
		source.bypassListenerEffects = false;
		source.Play();
	}

	public AudioSource PlayLoop2D(AudioClip clip, float pitch, float vol)
	{
		AudioSource source = this.GetSource(null, false);
		if (source == null)
		{
			return null;
		}
		source.pitch = pitch;
		source.clip = clip;
		source.volume = vol;
		source.reverbZoneMix = 0f;
		source.spatialBlend = 0f;
		source.bypassListenerEffects = false;
		source.loop = true;
		source.Play();
		return source;
	}

	public void ReleaseLoopedSource(AudioSource source)
	{
		if (source != null)
		{
			source.loop = false;
			source.Stop();
		}
	}

	public List<AudioClip> GetSoundForPickupSoundGroup(PickupSoundGroup group)
	{
		if (group == PickupSoundGroup.Medium)
		{
			return this.MediumPickup;
		}
		if (group == PickupSoundGroup.Heavy)
		{
			return this.HeavyPickup;
		}
		return this.CardPickup;
	}

	public static AudioManager me;

	public AudioMixer mixer;

	public AudioClip BattleLoop;

	public AudioClip SpiritLoop;

	private AudioSource ambienceSource;

	public List<AudioClip> CancelSadness;

	public List<AudioClip> Click;

	public List<AudioClip> SpiritTransitionExit;

	public List<AudioClip> SpiritTransitionEnter;

	public List<AudioClip> CitiesTransitionEnter;

	public List<AudioClip> CitiesTransitionExit;

	public List<AudioClip> Eat;

	public List<AudioClip> GetSick;

	public List<AudioClip> BecomeTeenager;

	public List<AudioClip> BecomeAdult;

	public List<AudioClip> BecomeOld;

	public List<AudioClip> ConsumeHappiness;

	public List<AudioClip> Unhappy;

	public List<AudioClip> Angry;

	public AudioMixerGroup SfxGroup;

	public AudioMixerGroup MusicGroup;

	public List<AudioClip> DropOnStack;

	public List<AudioClip> CardPickup;

	public List<AudioClip> CardDrop;

	public List<AudioClip> CardDestroy;

	public AudioClip QuestComplete;

	public List<AudioClip> MediumPickup;

	public List<AudioClip> HeavyPickup;

	public AudioClip EndOfMoon;

	public List<AudioClip> CardCreate;

	public List<AudioClip> OpenBooster;

	public List<AudioClip> Coin;

	public List<AudioClip> Dollar;

	public List<AudioClip> AnimalMove;

	public List<AudioClip> Miss;

	public List<AudioClip> Block;

	public List<AudioClip> HitMelee;

	public List<AudioClip> HitRanged;

	public List<AudioClip> HitMagic;

	public List<AudioClip> HitFoot;

	public List<AudioClip> HitArmour;

	public List<AudioClip> HitAir;

	public List<AudioClip> MissCities;

	public List<AudioClip> Buff;

	public List<AudioClip> MagicRelease;

	public List<AudioClip> MagicCharge;

	public List<AudioClip> RangedRelease;

	public List<AudioClip> Crit;

	public List<AudioClip> ShamanSpawn;

	public List<AudioClip> Bleed;

	public List<AudioClip> Poison;

	[Header("Cities")]
	public AudioClip EnergyConnected;

	public AudioClip EnergyStrech;

	public AudioClip EnergyStart;

	public AudioClip SewerConnected;

	public AudioClip SewerStrech;

	public AudioClip SewerStart;

	public AudioClip TransportConnected;

	public AudioClip TransportStrech;

	public AudioClip TransportStart;

	public AudioClip AddWellbeing;

	public AudioClip LostWellbeing;

	public AudioClip WellbeingCounter;

	public AudioClip PowerOutage;

	public AudioClip LandmarkBuild;

	public AudioClip ExtinguishCardSound;

	public AudioClip RepairCardSound;

	public AudioClip DamagedCardSound;

	public AudioClip OnFireCardSound;

	public AudioClip DroughtStart;

	public AudioClip DroughtSolved;

	public AudioClip IndustrialRevolutionCreate;

	public AudioClip PositiveEventSpawn;

	public AudioClip NegativeEventSpawn;

	public AudioClip ColliderRunningSound;

	public AudioClip AdvisorAppears;

	public AudioClip AdvisorWarning;

	public AudioClip AdvisorTalking;

	public AudioClip ClearPollution;

	public AudioClip SpawnPollution;

	public AudioClip LandfillOverflow;

	private AudioSource songSource;

	private AudioSource battleSource;

	private AudioSource spiritSource;

	private List<AudioClip> playedSongs = new List<AudioClip>();

	private float currentSongTargetVolume;

	private float newSongTimer;

	private bool muteSong;

	private List<AudioSource> sources = new List<AudioSource>();

	private List<Transform> targets = new List<Transform>();
}
