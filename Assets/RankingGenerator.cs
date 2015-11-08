using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankingGenerator : MonoBehaviour {

	private List<int> _listScore;

	[SerializeField]
	private GameObject _prRankText;

	private int _idx = -1;

	private const int RANK_NUMS = 9;

	public GameObject scoreTextStatic;
	public TextMesh scoreText;
	public TextMesh rankText;

	private int _score = 0;

	private AudioManager _audioManager;

	// Use this for initialization
	void Start () {
		_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		_listScore = new List<int>();
		for(int i=0; i<RANK_NUMS; i++) {
			_listScore.Add( PlayerPrefs.GetInt("score_"+i) );
		}
		GameObject go = GameObject.Find("SCORE");
		if(go==null) return;
		_score = int.Parse(go.transform.GetChild(0).gameObject.name);
		_listScore.Add( _score );

		_listScore.Sort((a,b)=>b-a);

		Destroy(go);

		StartCoroutine("DispRankingCoroutine");

/*
		for(int i=0; i<RANK_NUMS; i++) {
			PlayerPrefs.SetInt("score_"+i, _listScore[i]);
			go = Instantiate(_prRankText, _prRankText.transform.position, _prRankText.transform.rotation) as GameObject;
			go.transform.Translate(go.transform.forward * 1.5f * (i%5));
			go.transform.Translate(go.transform.right * 8.0f * (i/5));
			TextMesh tm = go.transform.GetComponent<TextMesh>();
			tm.text = "" + (i+1) + ": " + _listScore[i];
			if( _idx==-1 && _listScore[i] == _score ) {
				_idx = i;
				tm.color = new Color(1.0f, 0.5f, 0.2f, 1.0f);
				StartCoroutine("FlashCoroutine", tm);
			}
		}

		if( _idx == -1 ) {
			// スコアを別で表示
			go = Instantiate(_prRankText, _prRankText.transform.position, _prRankText.transform.rotation) as GameObject;
			go.transform.position = new Vector3(0.5f, 5.0f, -4.0f);
			TextMesh tm = go.transform.GetComponent<TextMesh>();
			tm.text = "Score" + ": " + _score;
			tm.color = new Color(1.0f, 0.5f, 0.2f, 1.0f);
			StartCoroutine("FlashCoroutine", tm);
		}
*/
	}

	private string[] ranks = new string[]{"見 習 い", "修 行 中", "凡 人", "一 人 前", "名 人", "達 人", "神"};
	
	private bool isRunningCoroutine = false;
	private IEnumerator DispRankingCoroutine() {
		isRunningCoroutine = true;
		yield return new WaitForSeconds(0.5f);
		_audioManager.Play("taiko01", 1.0f);
		scoreTextStatic.SetActive(true);
		yield return new WaitForSeconds(1.5f);
		_audioManager.Play("taiko01");
		scoreText.text = ""+_score;
		scoreText.gameObject.SetActive(true);
		yield return new WaitForSeconds(1.0f);

		int r;
		if( _score < 200 ) {
			r = 0;
		} else if( _score < 300 ) {
			r = 1;
		} else if( _score < 400 ) {
			r = 2;
		} else if( _score < 700 ) {
			r = 3;
		} else if( _score < 900 ) {
			r = 4;
		} else if( _score < 1200 ) {
			r = 5;
		} else {
			r = 6;
		}
		_audioManager.Play("taiko02");
		rankText.text = ranks[r];
		rankText.gameObject.SetActive(true);

		isRunningCoroutine = false;

		yield return new WaitForSeconds(1.0f);
		GetComponent<AudioSource>().Play();
	}

	private IEnumerator FlashCoroutine(TextMesh tm) {
		while(true) {
			yield return new WaitForSeconds(0.5f);
			tm.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.5f);
			tm.gameObject.SetActive(true);
		}
	}

	void Update() {
		if( Input.GetMouseButtonDown(0) && !isRunningCoroutine ) {
			Application.LoadLevel("title");
		}
	}
	
}
