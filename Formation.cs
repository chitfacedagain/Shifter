using UnityEngine;
using System.Collections;

public class Formation : MonoBehaviour {

	float graceTime = 0.1f;
	float birthTime;
	float lifeSpan = 10f;
	float burstTime = 0.417f;
	float damageCooldown = 1f;
	float dangerousForSeconds = 0.6f;

	bool didDamage = false;


	Rockman owner;

	public void SetOwner(Rockman _owner){
		owner = _owner;
	}

	void Start(){
		birthTime = Time.time;
	}

	void Update(){
		if (Time.time > birthTime + lifeSpan) {
			Die ();
		}
	}

	void Die(){
		Animator animator = GetComponent<Animator> ();
		animator.SetBool ("FormationDie", true);
		Destroy (gameObject, 0.875f);
	}

	void OnTriggerStay2D(Collider2D collider){
		if (collider.gameObject.tag == "Player") {
			if (Time.time >= birthTime + graceTime && Time.time <= birthTime + dangerousForSeconds) {
				owner.HitPlayer ();
			}
		}
	}
		

}
