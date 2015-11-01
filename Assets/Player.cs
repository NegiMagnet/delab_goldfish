using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	[SerializeField]
	Camera cam;

	[SerializeField]
	private GameDirector _director;

	// Use this for initialization
	void Start () {
	}

	bool _prevMouseDown = false;
	
	// Update is called once per frame
	void Update () {
		Vector3 v = cam.ScreenToWorldPoint(Input.mousePosition);
		float y = 3.0f;
		if( Input.GetMouseButton(0) ) {
			y = 0.0f;
		}
		transform.position = new Vector3(v.x, y, v.z);

		if( !_prevMouseDown && Input.GetMouseButton(0) ) {
			_director.IntoWater();
		} else if( _prevMouseDown && !Input.GetMouseButton(0) ) {
			_director.Check();
		}

		_prevMouseDown = Input.GetMouseButton(0);
	}
}
