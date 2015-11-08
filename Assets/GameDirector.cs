using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameDirector : MonoBehaviour {

	private List<Fish> _fishes;
	private int FISH_INIT = 20;
	private int FISH_MAX = 30;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private GameObject _fishPrefab;

	private int _score = 0;

	[SerializeField]
	private TextMesh _scoreText;

	private float _timer = 60.0f;
	[SerializeField]
	private TextMesh _timeText;

	private AudioManager _audioManager;

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

		_audioManager = GameObject.Find("AudioManager").transform.GetComponent<AudioManager>();

		StartCoroutine("ReadyCoroutine");
	}

	private bool _isDoingReadyCoroutine = false;
	private IEnumerator ReadyCoroutine() {
		_isDoingReadyCoroutine = true;
		TextMesh tm = GameObject.Find("readyCount").GetComponent<TextMesh>();
		tm.text = "";
		yield return new WaitForSeconds(1.0f);
		tm.text = "3";
		_audioManager.Play("button");
		yield return new WaitForSeconds(1.0f);
		tm.text = "2";
		_audioManager.Play("button");
		yield return new WaitForSeconds(1.0f);
		tm.text = "1";
		_audioManager.Play("button");
		yield return new WaitForSeconds(1.0f);
		tm.text = "GO !";
		_audioManager.Play("button", 2.0f);
		yield return new WaitForSeconds(1.0f);
		tm.text = "";
		GetComponent<AudioSource>().Play();
		_isDoingReadyCoroutine = false;
	}

	public bool isGameStart() { return !_isDoingReadyCoroutine; }

	private float _interval = 0.0f;
	void Update() {
		if( _isDoingReadyCoroutine ) return;

		int cnt = 0;
		for(int i=0; i<FISH_MAX; i++) {
			if( _fishes[i].gameObject.activeSelf ) {
				cnt++;
			}
		}
		if( (cnt <= 5) ||
		    	(cnt <= 10 && _interval >= 2.0f) ||
		    	(_interval >= 4.0f) ) {
			AddFish(Random.Range(0, Fish.KIND_NUMS));
			_interval = 0.0f;
		}

		_interval += Time.deltaTime;

		// タイマー更新、描画
		_timer -= Time.deltaTime;
		if( _timer < 10.0f ) {
			_timeText.color = new Color(1.0f, 0.5f, 0.2f, 1.0f);
		} else {
			_timeText.color = Color.white;
		}
		if( _timer < 5.0f && Mathf.Floor(_timer) < Mathf.Floor(_timer+Time.deltaTime) ) {
			_timeText.GetComponent<Animator>().SetTrigger("addScore");
		}
		_timeText.text = ""+(int)(Mathf.Ceil(_timer));

		// ボス登場
		if( _timer < 20.0f && _timer+Time.deltaTime >= 20.0f ) {
			AddFish(Fish.KIND_BIGBOSS);
		}

		// 終了
		if( _timer <= 0 ) {
			_isDoingReadyCoroutine = true;

			StartCoroutine("FinishCoroutine");
		}
	}

	private IEnumerator FinishCoroutine() {
		_audioManager.Play("finish");
		TextMesh tm = GameObject.Find("readyCount").GetComponent<TextMesh>();
		tm.text = "FINISH !";
		GetComponent<AudioSource>().Stop();
		yield return new WaitForSeconds(3.0f);

		// スコア伝達用オブジェクトの生成
		GameObject go = new GameObject();
		go.name = "SCORE";
		GameObject sc = new GameObject();
		sc.name = "" + _score;
		sc.transform.parent = go.transform;
		DontDestroyOnLoad(go);
	
		Application.LoadLevel("ranking");
	}

	private void AddFish(int kind) {
		for(int i=0; i<FISH_MAX; i++) {
			if( !_fishes[i].gameObject.activeSelf ) {
				_fishes[i].setKind( kind );
				_fishes[i].gameObject.SetActive(true);
				break;
			}
		}
	}
	
	public void Check() {
		if( !isGameStart() ) return;
		bool isCaught = false;
		foreach(Fish f in _fishes) {
			if( f.isCaught(_player.transform.position) ) {
				Catch(f);
				isCaught = true;
			}
		}

		if(isCaught) {
			_audioManager.Play("splash");
		}
	}

	private void Catch(Fish f) {
		_score += f.getScore();
		_scoreText.text = "" + _score;
		_scoreText.GetComponent<Animator>().SetTrigger("addScore");
		f.Caught();
	}

	public void IntoWater() {
		foreach(Fish f in _fishes) {
			if( !f.gameObject.activeSelf ) continue;
			Vector3 diff = f.transform.position - _player.transform.position;
			if(-1.2f <= diff.x && diff.x <= 0.0f && -0.3f <= diff.z && diff.z <= 1.4f) {
				f.Escape();
			}
		}
	}
}
