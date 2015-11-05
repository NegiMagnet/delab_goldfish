using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameDirector : MonoBehaviour {

	private List<Fish> _fishes;
	private int FISH_INIT = 10;
	private int FISH_MAX = 20;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private GameObject _fishPrefab;

	private int _score = 0;

	[SerializeField]
	private TextMesh _text;

	private float _timer = 60.0f;
	[SerializeField]
	private TextMesh _timeText;

	// Use this for initialization
	void Start () {
		_fishes = new List<Fish>();
		for(int i=0; i<FISH_MAX; i++) {
			GameObject go;
			go = Instantiate(_fishPrefab, transform.position, Quaternion.identity) as GameObject;
			Fish f = go.GetComponent<Fish>();
			f.setKind(Fish.KIND_NORMAL);
			_fishes.Add(f);
		}
		for(int i=FISH_INIT; i<FISH_MAX; i++) {
			_fishes[i].gameObject.SetActive(false);
		}

	}

	private float _interval = 0.0f;
	void Update() {
		int cnt = 0;
		for(int i=0; i<FISH_MAX; i++) {
			if( _fishes[i].gameObject.activeSelf ) {
				cnt++;
			}
		}
		if( (cnt <= 3) ||
		    	(cnt <= 7 && _interval >= 2.0f) ||
		    	(_interval >= 4.0f) ) {
			AddFish();
			_interval = 0.0f;
		}

		_interval += Time.deltaTime;

		_timer -= Time.deltaTime;
		_timeText.text = ""+(int)(Mathf.Ceil(_timer));

		if( _timer <= 0 ) {
			GameObject go = new GameObject();
			go.name = "SCORE";
			GameObject sc = new GameObject();
			sc.name = "" + _score;
			sc.transform.parent = go.transform;
			DontDestroyOnLoad(go);
			Application.LoadLevel("ranking");
		}
	}

	private void AddFish() {
		for(int i=0; i<FISH_MAX; i++) {
			if( !_fishes[i].gameObject.activeSelf ) {
				_fishes[i].setKind( Random.Range(0, Fish.KIND_NUMS) );
				_fishes[i].gameObject.SetActive(true);
				break;
			}
		}
	}
	
	public void Check() {
		bool isCaught = false;
		foreach(Fish f in _fishes) {
			if( !f.gameObject.activeSelf ) continue;
			Vector3 diff = f.transform.position - _player.transform.position;
			if(-1.2f <= diff.x && diff.x <= 0.0f && 0.0f <= diff.z && diff.z <= 1.3f) {
				Catch(f);
				isCaught = true;
			}
		}

		if(isCaught) {
			GetComponent<AudioSource>().Play();
		}
	}

	private void Catch(Fish f) {
		_score += f.getScore();
		_text.text = "Score: " + _score;
		f.Caught();
	}

	public void IntoWater() {
		foreach(Fish f in _fishes) {
			if( !f.gameObject.activeSelf ) continue;
			Vector3 diff = f.transform.position - _player.transform.position;
			if(-1.2f <= diff.x && diff.x <= 0.0f && 0.0f <= diff.z && diff.z <= 1.3f) {
				f.Escape();
			}
		}
	}
}
