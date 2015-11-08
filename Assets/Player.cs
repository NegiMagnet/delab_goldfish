using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	[SerializeField]
	Camera cam;

	[SerializeField]
	private GameDirector _director;

	private AudioManager _audioManager;
	private Animator _animator;

	// Use this for initialization
	void Start () {
		_audioManager = GameObject.Find("AudioManager").transform.GetComponent<AudioManager>();
		_animator = GetComponent<Animator>();
	}

	bool _prevMouseDown = false;
	
	// Update is called once per frame
	void Update () {
		bool isMouseButtonDown = Input.GetMouseButton(0);

		Vector3 v = cam.ScreenToWorldPoint(Input.mousePosition);
		float y = 3.0f;
		if( isMouseButtonDown ) {
			y = 0.0f;
		}
		transform.position = new Vector3(v.x, y, v.z);

		if( !_prevMouseDown && isMouseButtonDown ) {
			_director.IntoWater();
			_audioManager.Play("drop");
			_animator.SetTrigger("sink");
		} else if( _prevMouseDown && !isMouseButtonDown ) {
			_director.Check();
			_animator.SetTrigger("float");
		}

		_prevMouseDown = isMouseButtonDown;
	}
}
