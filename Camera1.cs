using UnityEngine;
using System.Collections;

public class Camera1 : MonoBehaviour {

	public Transform watchedEntity;
	public float distanceFromEntity = 30f;
	Vector3 offset;
	Camera camera;

	public SpriteRenderer mudsplash;

	void Awake () {
		camera = GetComponent<Camera> ();
	}
	
	void Update () {
		if (watchedEntity != null) {
			transform.position = watchedEntity.transform.position + new Vector3 (0, 0, -distanceFromEntity);

			if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
				distanceFromEntity = Mathf.Clamp (distanceFromEntity - 5f, 10f, 100f);
			}
			if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
				distanceFromEntity = Mathf.Clamp (distanceFromEntity + 5f, 10f, 100f);
			}
		}
	}

	public void SetBackgroundColor (Color _color){
		camera.backgroundColor = _color;
	}

	public void MudSplat(){
		mudsplash.enabled = true;
		StartCoroutine (killSplash ());
	}

	IEnumerator killSplash(){
		yield return new WaitForSeconds (2f);
		mudsplash.enabled = false;
	}
}
