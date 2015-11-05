using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankingGenerator : MonoBehaviour {

	private List<int> _listScore;

	[SerializeField]
	private GameObject _prRankText;

	private int _idx = -1;

	private const int RANK_NUMS = 9;

	// Use this for initialization
	void Start () {
		_listScore = new List<int>();
		for(int i=0; i<RANK_NUMS; i++) {
			_listScore.Add( PlayerPrefs.GetInt("score_"+i) );
		}
		GameObject go = GameObject.Find("SCORE");
		if(go==null) return;
		int score = int.Parse(go.transform.GetChild(0).gameObject.name);
		_listScore.Add( score );

		_listScore.Sort((a,b)=>b-a);

		Destroy(go);

		for(int i=0; i<RANK_NUMS; i++) {
			PlayerPrefs.SetInt("score_"+i, _listScore[i]);
			go = Instantiate(_prRankText, _prRankText.transform.position, _prRankText.transform.rotation) as GameObject;
			go.transform.Translate(go.transform.forward * 1.5f * (i%5));
			go.transform.Translate(go.transform.right * 8.0f * (i/5));
			TextMesh tm = go.transform.GetComponent<TextMesh>();
			tm.text = "" + (i+1) + ": " + _listScore[i];
			if( _idx==-1 && _listScore[i] == score ) {
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
			tm.text = "Score" + ": " + score;
			tm.color = new Color(1.0f, 0.5f, 0.2f, 1.0f);
			StartCoroutine("FlashCoroutine", tm);
		}
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
		if( Input.GetMouseButtonDown(0) ) {
			Application.LoadLevel("main");
		}
	}
	
}
