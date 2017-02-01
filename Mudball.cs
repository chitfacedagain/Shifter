using UnityEngine;
using System.Collections;

public class Mudball : MonoBehaviour {

	public Camera1 camera;
	Mudman owner;

	float lifespan = 5f;
	float speed = 55f;
	Vector3 velocity;

	void Start(){
		Destroy (gameObject, lifespan);
		camera = FindObjectOfType<Camera1>();
	}

	void TakeHit(float damage){
	}

	void OnTriggerEnter2D(Collider2D collider){
		

		if (collider.gameObject.tag == "Enemy") {
			return;
		}

		if (collider.gameObject.tag == "Player") {

			owner.MudballHit ();
			camera.MudSplat ();
			Splat ();
		}
	}

	void Splat(){
		Destroy (gameObject);
	}

	void Update(){
		if (velocity != null) {
			transform.position = transform.position + velocity * Time.deltaTime * speed;
		}
	}

	public void SetVelocity(Vector2 _velocity){
		velocity = new Vector3 (_velocity.x, _velocity.y, 0);
	}

	public void SetVelocity(Vector3 _velocity){
		velocity = _velocity;
	}

	public void SetOwner(Mudman monster){
		owner = monster;
	}

}
