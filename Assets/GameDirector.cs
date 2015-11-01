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
			int r = Random.Range(0, 2);
			GameObject go;
			go = Instantiate(_fishPrefab, transform.position, Quaternion.identity) as GameObject;
			Fish f = go.GetComponent<Fish>();
			f.setKind(r);
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
		if( (cnt <= 1) ||
		    	(cnt <= 5 && _interval >= 2.0f) ||
		    	(_interval >= 4.0f) ) {
			AddFish();
			_interval = 0.0f;
		}

		_interval += Time.deltaTime;

		_timer -= Time.deltaTime;
		_timeText.text = ""+(int)(Mathf.Ceil(_timer));
	}

	private void AddFish() {
		for(int i=0; i<FISH_MAX; i++) {
			if( !_fishes[i].gameObject.activeSelf ) {
				_fishes[i].gameObject.SetActive(true);
				break;
			}
		}
	}
	
	public void Check() {
		foreach(Fish f in _fishes) {
			if( !f.gameObject.activeSelf ) continue;
			Vector3 diff = f.transform.position - _player.transform.position;
			if(-1.2f <= diff.x && diff.x <= 0.0f && 0.0f <= diff.z && diff.z <= 1.3f) {
				Catch(f);
			}
		}
	}

	private void Catch(Fish f) {
		Debug.Log("Catch " + f.gameObject.name);
		if( f.gameObject.name == "GOLDFISH(Clone)" ) {
			_score += 10;
		}
		else if( f.gameObject.name == "GOLDFISH_red(Clone)" ) {
			_score += 30;
		}
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
