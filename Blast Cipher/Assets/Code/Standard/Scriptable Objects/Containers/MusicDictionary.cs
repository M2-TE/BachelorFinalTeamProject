using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicDictionary", menuName = "Audio/MusicDictionary", order = 1)]
public class MusicDictionary : ScriptableObject
{
	[Serializable]
	public class AudioTuple
	{
		public string SceneName;
		public int BuildIndex;
		public AudioContainer Audio;
	}

	[SerializeField] private List<AudioTuple> music;

	private Dictionary<int, AudioTuple> _musicDict;
	public Dictionary<int, AudioTuple> MusicDict
	{
		get
		{
			if (_musicDict == null)
			{
				// build dictionary based on serialized music list
				_musicDict = new Dictionary<int, AudioTuple>();
				for (int i = 0; i < music.Count; i++)
				{
					_musicDict.Add(music[i].BuildIndex, music[i]);
				}
			}

			return _musicDict;
		}
	}
}
