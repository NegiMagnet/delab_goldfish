using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

	public List<AudioClip> clips;

	private const int NUMS = 10;
	private List<AudioSource> _source;

	// Use this for initialization
	void Start () {
		_source = new List<AudioSource>();
		for(int i=0; i<NUMS; i++) {
			_source.Add( gameObject.AddComponent<AudioSource>() as AudioSource );
		}
	}
	
	public void Play(string name, float pitch=1.0f) {
		int idx = -1;
		for(int i=0; i<NUMS; i++) {
			if( !_source[i].isPlaying ) {
				idx = i;
				break;
			}
		}
		if(idx == -1) {
			Debug.LogWarning("audio source is full");
			return;
		}

		int cidx = -1;
		for(int i=0; i<clips.Count; i++) {
			if( clips[i].name == name ) {
				cidx = i;
				break;
			}
		}
		if(cidx == -1) {
			Debug.LogWarning("audio clip isn't found : " + name);
			return;
		}

		_source[idx].clip = clips[cidx];
		_source[idx].pitch = pitch;
		_source[idx].Play();
	}
}
