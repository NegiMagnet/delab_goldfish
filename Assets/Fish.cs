using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour {

	private float _vel;

	public float TURN_INTERVAL_MIN = 3.0f;
	public float TURN_INTERVAL_MAX = 5.0f;

	private bool _isCaught = false;
	private float _caughtTime = 0.0f;

	[SerializeField]
	private MeshRenderer _mesh;

	[SerializeField]
	private Material _matNormal;
	[SerializeField]
	private Material _matFast;

	// Use this for initialization
	void Start () {
	}

	void OnEnable() {
		StartCoroutine("UpdateCoroutine");
		transform.localScale = new Vector3(1.0f,1.0f,1.0f)*15.0f;
		Vector3 v = new Vector3( Random.Range(-8.0f, 8.0f), -0.2f, Random.Range(-4.0f, 4.0f) );
		transform.position = v;
		transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
	}

	void OnDisable() {
		StopCoroutine("UpdateCoroutine");
		_caughtTime = 0.0f;
		_isCaught = false;
	}

	private int _kind;
	public const int KIND_NORMAL = 0;
	public const int KIND_FAST = 1;
	public const int KIND_NUMS = 2;
	public void setKind(int k) {
		_kind = k;
		if(k == KIND_NORMAL) {
			_vel = 0.75f / 60.0f;
			TURN_INTERVAL_MIN = 3.0f;
			TURN_INTERVAL_MAX = 5.0f;
			_mesh.material = _matNormal;
		} else if(k == KIND_FAST) {
			_vel = 2.25f / 60.0f;
			TURN_INTERVAL_MIN = 0.7f;
			TURN_INTERVAL_MAX = 1.5f;
			_mesh.material = _matFast;
		}
	}

	public int getScore() {
		if( _kind == KIND_NORMAL ) {
			return 10;
		} else {
			return 30;
		}
	}

	private IEnumerator UpdateCoroutine() {
		while(true) {
			yield return new WaitForSeconds( Random.Range(TURN_INTERVAL_MIN, TURN_INTERVAL_MAX) );
			float angle = Random.Range(0.0f, 360.0f);
			transform.Rotate(0.0f, angle, 0.0f);
//			_vel = VELOCITY;
		}
	}

	Vector3 flyDir = Vector3.zero;
	void Update() {
		if( _isCaught ) {
			float y = 4.0f - (_caughtTime-1.0f) * (_caughtTime-1.0f) * 3.0f;
			transform.localScale = new Vector3(y, y, y) * 15.0f;
			Vector3 p = transform.position;
			transform.position = new Vector3(p.x, p.y, p.z) + flyDir * Time.deltaTime;
			transform.Rotate(Time.deltaTime * 90.0f * transform.forward);
			_caughtTime += Time.deltaTime;
			if( _caughtTime > 3.0f ) {
				gameObject.SetActive(false);
			}
		} else {
//			_vel *= 0.9f;
			float vel = _vel;
			if( _escapeTime > 0.0f ) {
				_escapeTime -= Time.deltaTime;
				vel *= 8.0f;
			}
			Vector3 p = transform.position + transform.forward * vel;
			transform.position = new Vector3( Mathf.Clamp(p.x, -8.0f, 8.0f), -1.0f, Mathf.Clamp(p.z, -4.5f, 4.5f) );
		}
	}

	public void Caught() {
		StopCoroutine("UpdateCoroutine");
		_isCaught = true;
		transform.Translate(5.0f * transform.up);
		float angle = Random.Range(0.0f, 360.0f);
		flyDir = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)) * 7.0f;

		GameObject.Find("SplashEmitter").transform.GetComponent<SplashEmitter>().Splash(transform.position);
	}

	private float _escapeTime = 0.0f;
	public void Escape() {
		_escapeTime = 1.4f;
	}
}
