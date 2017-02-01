using UnityEngine;
using System.Collections;
using System;

public class Rockman : MonoBehaviour {


	float attackDistance = 20f;
	float attack1Cooldown = 1.5f;
	float lastAttack1;
	float attack2Cooldown = 2f;
	float lastAttack2;
	float maxHealth = 10f;
	float aggroThreshold = 35f;
	float moveSpeed = 3f;

	public float attack1Damage = 4f;
	public float attack2Damage = 0f;

	public EffectsSummoner effectsSummoner;
	public Animator sprites;
	public Transform spriteTransform;
	public Transform target = null;
	public Transform player;
	public Rigidbody2D body;
	public Formation formationFab;

	public BoxCollider2D myCollider;

	PolygonCollider2D playerCollider;
	SpriteRenderer renderer;
	Player playerClass;
	GameLogic gameLogic;



	float myCollisionRadius;
	float targetCollisionRadius;
	float timeBetweenChecks = 2f;
	float lastCheck;
	float currentHealth;
	float attackAnimationTime = 0.833f;
	float attackLength = 1f;
	float buriedDuration = 1.2f;

	float playerSpeed;

	bool moveLock = false;
	bool attacking = false;
	bool dead = false;
	bool invulnerable = false;

	void Awake(){

	}


	protected void Start(){

		lastCheck = Time.time;

		lastAttack1 = Time.time;
		lastAttack2 = Time.time;

		player = FindObjectOfType<Player>().transform;
		playerCollider = player.gameObject.GetComponent<PolygonCollider2D> ();
		playerClass = player.gameObject.GetComponent<Player> ();
		playerSpeed = playerClass.playerSpeed;
		effectsSummoner = FindObjectOfType<EffectsSummoner> ();

		renderer = spriteTransform.GetComponent<SpriteRenderer> ();
		currentHealth = maxHealth;
		gameLogic = FindObjectOfType<GameLogic> ();


	}

	void FixedUpdate () {
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

		if (!attacking) {
			sprites.SetBool ("Running", false);
			sprites.SetBool ("Idle", true);
		}

		if (target != null) {

			float distanceSqrd = (transform.position - player.position).sqrMagnitude;

			if (!moveLock) {
				sprites.SetBool ("Running", true);
				sprites.SetBool ("Idle", false);
				MoveTowardTarget ();
			}
				
				if (distanceSqrd <= Mathf.Pow (attackDistance, 2f)) {
					FirstAttack ();
				}
		}

	}

	public void HitPlayer(){
		player.gameObject.SendMessage ("TakeHit", attack1Damage);
	
	}

	public void TakeHit(float damage){
		currentHealth -= damage;
		if (currentHealth <= 0 && !dead) {
			Die ();
		}else {
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

	void Die(){
		dead = true;
		sprites.SetFloat ("AttackModifier", 0.001f);
		sprites.SetBool ("Running", false);
		sprites.SetBool ("Idle", false);
		sprites.SetBool ("AttackEnd", false);
		sprites.SetTrigger ("Attack1");


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
		yield return new WaitForSeconds (1f);
		gameLogic.ReportDeath (gameObject);
		GameObject.Destroy (gameObject);
	}

	void MoveTowardTarget(){
		Vector3 difference = (target.position - transform.position);
		float xDif = target.position.x - transform.position.x;
		if (xDif > 0) {renderer.flipX = true;}
		if (xDif < 0) {renderer.flipX = false;}
		Vector2 adjustedVelocity = new Vector2 (difference.x, difference.y).normalized;
		body.MovePosition(body.position + adjustedVelocity * moveSpeed * Time.deltaTime);
	}
		

	void FirstAttack(){
		if (attacking) {return;}
		if (lastAttack1 + attack1Cooldown > Time.time) {return;}
		attacking = true;
		moveLock = true;

		StartCoroutine (RockCandy ());
	}



	IEnumerator RockCandy(){
		float modifier = attackAnimationTime / attackLength;
		float percent = 0f;
		Vector2 playerV = playerClass.GetDeliberateVelocity ();
		Vector3 targetLocation = player.position;

		sprites.SetFloat ("AttackModifier", modifier);
		sprites.SetBool ("AttackEnd", true);
		sprites.SetTrigger ("Attack1");
		sprites.SetBool ("Idle", false);

		effectsSummoner.SummonFollowingDangerCircle (player, 5f, attackLength);

		while (percent < 1f) {
			percent += Time.fixedDeltaTime * 1f / attackLength;
			if (percent < 0.9f) {
				targetLocation = player.position;
			}
			yield return null;
		}

		Formation newFormation = Instantiate (formationFab, targetLocation, Quaternion.identity) as Formation;
		newFormation.SetOwner (this);
		invulnerable = true;
		percent = 0f;

		while (percent < 1f) {
			percent += Time.deltaTime * 1f / buriedDuration;

			yield return null;
		}

		sprites.SetBool ("AttackEnd", false);
		sprites.SetBool ("Idle", true);
		invulnerable = false;
		moveLock = false;
		yield return new WaitForSeconds (attack1Cooldown);
		attacking = false;
	}


	IEnumerator RockCandyPredict(){
		float modifier = attackAnimationTime / attackLength;
		float percent = 0f;
		Vector2 playerV = playerClass.GetDeliberateVelocity ();
		Vector3 targetLocation = player.position + new Vector3 (playerV.x, playerV.y, 0f).normalized * playerSpeed * (attackLength + 0.1f);

		sprites.SetFloat ("AttackModifier", modifier);
		sprites.SetBool ("AttackEnd", true);
		sprites.SetTrigger ("Attack1");
		sprites.SetBool ("Idle", false);

		while (percent < 1f) {
			percent += Time.fixedDeltaTime * 1f / attackLength;
			yield return null;
		}

		Formation newFormation = Instantiate (formationFab, targetLocation, Quaternion.identity) as Formation;
		newFormation.SetOwner (this);
		invulnerable = true;
		percent = 0f;

		while (percent < 1f) {
			percent += Time.deltaTime * 1f / buriedDuration;

			yield return null;
		}

		sprites.SetBool ("AttackEnd", false);
		sprites.SetBool ("Idle", true);
		invulnerable = false;
		moveLock = false;
		yield return new WaitForSeconds (attack1Cooldown);
		attacking = false;
	}


}
