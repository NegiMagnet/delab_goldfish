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
	[SerializeField]
	private Material _matBoss;

	private float _scale = 1.0f;

	// Use this for initialization
	void Start () {
	}

	void OnEnable() {
		transform.localScale = new Vector3(1.0f,1.0f,1.0f)*15.0f * _scale;
		Vector3 v = new Vector3( Random.Range(-7.0f, 7.0f), -0.2f, Random.Range(-4.0f, 3.5f) );
		transform.position = v;
		transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

		if( _kind == KIND_BIGBOSS ) {
			StartCoroutine("BossCoroutine");
		} else {
			StartCoroutine("UpdateCoroutine");
		}
	}

	void OnDisable() {
		StopCoroutine("UpdateCoroutine");
		_caughtTime = 0.0f;
		_isCaught = false;
		_escapeTime = 0.0f;
	}

	private int _kind;
	public const int KIND_NORMAL = 0;
	public const int KIND_FAST = 1;
	public const int KIND_NUMS = 2;
	public const int KIND_BIGBOSS = 3;
	public void setKind(int k) {
		_kind = k;
		if(k == KIND_NORMAL) {
			_vel = 0.75f / 60.0f;
			TURN_INTERVAL_MIN = 3.0f;
			TURN_INTERVAL_MAX = 5.0f;
			_scale = 1.0f;
			_mesh.material = _matNormal;
		} else if(k == KIND_FAST) {
			_vel = 2.25f / 60.0f;
			TURN_INTERVAL_MIN = 0.7f;
			TURN_INTERVAL_MAX = 1.5f;
			_scale = 1.0f;
			_mesh.material = _matFast;
		}
		else if(k == KIND_BIGBOSS) {
			_vel = 0.3f / 60.0f;
			TURN_INTERVAL_MIN = 0.0f;
			TURN_INTERVAL_MAX = 10.0f;
			_scale = 2.5f;
			_mesh.material = _matBoss;
		}
	}

	public int getScore() {
		if( _kind == KIND_NORMAL ) {
			return 10;
		} else if( _kind == KIND_FAST ) {
			return 30;
		} else {
			return 100;
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

	private IEnumerator BossCoroutine() {
		while(true) {
			transform.position = new Vector3(Random.Range(-7.0f, 7.0f), -0.4f, Random.Range(-4.0f, 3.5f));
			transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

			const float floatTime = 0.6f;
			float t = 0.0f;
			while(t < floatTime) {
				_matBoss.color = new Color(0.0f, 0.0f, 0.0f, t);
				t += Time.deltaTime;
				yield return null;
			}
			t = 0.0f;
			while(t < floatTime) {
				_matBoss.color = new Color(0.0f, 0.0f, 0.0f, floatTime-t);
				t += Time.deltaTime;
				yield return null;
			}

			transform.position = new Vector3(100.0f, -0.4f, 100.0f);
			yield return new WaitForSeconds(3.6f);
		}
	}

	Vector3 flyDir = Vector3.zero;
	void Update() {
		if( _isCaught ) {
			float y = 5.0f - (_caughtTime-1.0f) * (_caughtTime-1.0f) * 4.0f;
			transform.localScale = new Vector3(y, y, y) * 15.0f * _scale;
			Vector3 p = transform.position;
			transform.position = new Vector3(p.x, p.y, p.z) + flyDir * Time.deltaTime * 2.0f;
			transform.Rotate(Time.deltaTime * 90.0f * transform.forward);
			_caughtTime += Time.deltaTime;
			if( _caughtTime > 2.0f ) {
				gameObject.SetActive(false);
			}
			if( _kind == KIND_BIGBOSS ) {
				_matBoss.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			}
		} else if(_kind != KIND_BIGBOSS) {
//			_vel *= 0.9f;
			float vel = _vel;
			if( _escapeTime > 0.0f ) {
				_escapeTime -= Time.deltaTime;
				vel *= 8.0f;
			}
			if( _throughTime > 0.0f ) {
				_throughTime -= Time.deltaTime;
			}
			transform.localScale = new Vector3(15.0f,15.0f,15.0f) * _scale;
			Vector3 p = transform.position + transform.forward * vel;
			transform.position = new Vector3( Mathf.Clamp(p.x, -7.0f, 7.0f), -0.2f, Mathf.Clamp(p.z, -4.0f, 3.5f) );
		}
	}

	public void Caught() {
		StopCoroutine("UpdateCoroutine");
		StopCoroutine("BossCoroutine");
		_isCaught = true;
		transform.Translate(5.0f * transform.up);
		float angle = Random.Range(0.0f, 360.0f);
		flyDir = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)) * 7.0f;

		GameObject.Find("SplashEmitter").transform.GetComponent<SplashEmitter>().Splash(transform.position);

		if( _kind == KIND_BIGBOSS ) {
			StartCoroutine("DelayResetColorCoroutine");
		}
	}

	private IEnumerator DelayResetColorCoroutine() {
		yield return null;
		_matBoss.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
	}

	public bool isCaught(Vector3 poiPos) {
		if( !gameObject.activeSelf ) return false;
		if( _throughTime > 0.0f ) return false;
		Vector3 diff = transform.position - poiPos;
		if(-1.2f <= diff.x && diff.x <= 0.0f && -0.3f <= diff.z && diff.z <= 1.4f) {
			return true;
		}
		return false;
	}

	private float _escapeTime = 0.0f;
	private float _throughTime = 0.0f;
	public void Escape() {
		_escapeTime = 1.4f;
		_throughTime = 0.2f;
	}
}

