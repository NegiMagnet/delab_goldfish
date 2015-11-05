using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplashEmitter : MonoBehaviour {

	public GameObject prSplash;
	const int NUMS = 20;
	private List<GameObject> _list;

	void Start() {
		_list = new List<GameObject>();
		for(int i=0; i<NUMS; i++) {
			GameObject go = Instantiate(prSplash, transform.position, Quaternion.identity) as GameObject;
			go.SetActive(false);
			_list.Add(go);
		}
	}

	public void Splash(Vector3 pos) {
		for(int i=0; i<NUMS; i++) {
			if( !_list[i].activeSelf ) {
				_list[i].transform.position = pos;
				StartCoroutine("SplashCoroutine", _list[i]);
				break;
			}
		}
	}

	private IEnumerator SplashCoroutine(GameObject go) {
		go.SetActive(true);
		yield return new WaitForSeconds(4.0f);
		go.SetActive(false);
	}
}

