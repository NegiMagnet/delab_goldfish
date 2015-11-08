using UnityEngine;
using System.Collections;

public class TitleDirector : MonoBehaviour {

	public void GameStart() {
		Application.LoadLevel("main");
	}

	void Update() {
		if( Input.GetKey(KeyCode.Escape) ) {
			Application.Quit();
		}
	}
	
}
