using UnityEngine;
using System.Collections;
using System;

public class Mudman : MonoBehaviour {


	public float attackDistance = 40f;
	public float attackAnimationTime = 0.6f;
	public float attack1Cooldown = 2f;
	public float lastAttack1;
	public float attack2Cooldown = 2f;
	public float lastAttack2;
	public float maxHealth = 10f;
	public float aggroThreshold = 35f;
	public float moveSpeed = 10f;

	public float attack1Damage = 0f;
	public float attack2Damage = 4f;

	public Animator sprites;
	public Animator effects;
	public Transform spriteTransform;
	public Transform cameraPos;
	public Transform target = null;
	public Transform player;
	public Rigidbody2D body;
	public Mudball mudballFab;
	public BoxCollider2D myCollider;
	public AudioClip roar;
	public AudioClip splat;
	public AudioClip death;

	PolygonCollider2D playerCollider;
	GameLogic gameLogic;

	float myCollisionRadius;
	float targetCollisionRadius;
	float timeBetweenChecks = 2f;
	float lastCheck;
	float currentHealth;

	bool moveLock = false;
	bool attacking = false;
	bool gnashing = false;
	bool dead = false;

	void Awake(){

	}


	protected void Start(){
		
		lastCheck = Time.time;

		lastAttack1 = Time.time;
		lastAttack2 = Time.time;

		cameraPos = FindObjectOfType<Camera> ().transform;
		player = FindObjectOfType<Player>().transform;
		playerCollider = player.gameObject.GetComponent<PolygonCollider2D> ();
		currentHealth = maxHealth;
		gameLogic = FindObjectOfType<GameLogic> ();
	}
		
	void Update () {
		if(dead){return; }
		
		if (Time.time >= lastCheck + timeBetweenChecks) {
			float distanceSqrd = (transform.position - player.position).sqrMagnitude;
			if (distanceSqrd <= Mathf.Pow (aggroThreshold, 2f)) {
				target = player;
			}

			if (distanceSqrd > Mathf.Pow (aggroThreshold, 2f)) {
				target = null;
			}
		}

		if (target != null) {
			
			float distanceSqrd = (transform.position - player.position).sqrMagnitude;

			if (gnashing) {
				moveSpeed = 40f;

			}

			if (!gnashing) {
				if (distanceSqrd > 100f) {
					moveSpeed = 10f;
					MoveTowardTarget ();
				}
				if (distanceSqrd <= Mathf.Pow (attackDistance - 2f, 2f)) {
				
					FirstAttack ();

				}
			}

		}

		if (gnashing) {
			if (myCollider.IsTouching (playerCollider)) {
				player.gameObject.SendMessage ("TakeHit", attack2Damage);
			}
		}

	}

	public void TakeHit(float damage){
		currentHealth -= damage;
		if (currentHealth <= 0 && !dead) {
			Die ();
		} else {
			Vector2 direction = new Vector2(transform.position.x - player.position.x, transform.position.y - player.position.y).normalized;
			body.AddForce (direction * 5f, ForceMode2D.Impulse);
		}

	}

	public void TakeHit(float damage, Vector2 point){
		currentHealth -= damage;
		if (currentHealth <= 0 && !dead) {
			Die ();
		} else {
			//StartCoroutine (Pushed (15f, 1f));
			Vector2 direction = new Vector2( transform.position.x - point.x, transform.position.y - point.y).normalized;
			body.AddForce (direction * 500f, ForceMode2D.Impulse);
		}

	}

	IEnumerator Pushed(float force, float time, Vector2 direction){
		moveLock = true;
		float percent = 0f;

		while (percent < 1f) {
			yield return null;
		}
	}

	void Die(){
		dead = true;
		AudioSource.PlayClipAtPoint (death, cameraPos.position);

		StartCoroutine (Death ());
	}

	IEnumerator Death(){
		float percent = 0f;
		float deathTime = 1.5f;
		SpriteRenderer renderer = spriteTransform.GetComponent<SpriteRenderer> ();


		while (percent < 1) {
			percent += Time.deltaTime * 1f / deathTime;
			renderer.color = Color.Lerp (Color.white, Color.red, percent * deathTime);
			yield return null;
		}

		spriteTransform.GetComponent<SpriteRenderer> ().enabled = false;
		AudioSource.PlayClipAtPoint (roar, cameraPos.position);
		yield return new WaitForSeconds (1f);
		gameLogic.ReportDeath (gameObject);
		GameObject.Destroy (gameObject);
	}

	void MoveTowardTarget(){
		Vector3 difference = (target.position - transform.position);
		float xDif = target.position.x - transform.position.x;
		SpriteRenderer renderer = spriteTransform.GetComponent<SpriteRenderer> ();
		if (xDif > 0) {renderer.flipX = true;}
		if (xDif < 0) {renderer.flipX = false;}
		Vector2 adjustedVelocity = new Vector2 (difference.x, difference.y).normalized;
		body.MovePosition(body.position + adjustedVelocity * moveSpeed * Time.deltaTime);
	}

	public void MudballHit(){
		if(dead){return; }
		SecondAttack ();
	}



	void FirstAttack(){
		if (attacking) {return;}
		if (lastAttack1 + attack1Cooldown > Time.time) {return;}
		attacking = true;
		sprites.SetTrigger ("Attack1");
		Mudball newMudball = Instantiate (mudballFab, transform.position, Quaternion.identity) as Mudball;
		Vector3 direction = (target.position - transform.position).normalized;
		//Mudball mudball = newMudball.GetComponent<Mudball> ();
		AudioSource.PlayClipAtPoint(splat, cameraPos.position);
		newMudball.SetVelocity (direction);
		newMudball.SetOwner (this);
		lastAttack1 = Time.time;
		attacking = false;
	}

	void SecondAttack(){
		if (attacking) {return;}
		if (target == null) {return;}

		sprites.SetBool ("Idle", false);
		sprites.SetBool ("Attack2", true);

		float xDif = target.position.x - transform.position.x;
		SpriteRenderer renderer = spriteTransform.GetComponent<SpriteRenderer> ();
		if (xDif > 0) {renderer.flipX = true;}
		if (xDif < 0) {renderer.flipX = false;}

		gnashing = true;
		Vector3 direction = (target.position - transform.position).normalized;
		AudioSource.PlayClipAtPoint (roar, cameraPos.position);
		StartCoroutine (ChargeAttack(direction));

	}

	IEnumerator ChargeAttack(Vector3 direction){
		float chargeTime = 1f;
		float percent = 0f;
		float chargeDistance = 25f;

		Vector2 oldPosition = new Vector2 (transform.position.x, transform.position.y);

		while (percent < 1) {
			percent += Time.deltaTime * 1f / chargeTime;
			Vector2 newPosition = oldPosition + new Vector2 (direction.x, direction.y) * chargeDistance * percent;
			body.MovePosition (newPosition);

			yield return null;
		}

		gnashing = false;
		sprites.SetBool ("Idle", true);
		sprites.SetBool ("Attack2", false);
	}
		


}
