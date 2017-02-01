
using UnityEngine;
using System.Collections;



public class Player : LivingThing {

	public Animator animator;
	public Vector2 deliberateVelocity;
	public float playerSpeed;
	public Animator attackEffects;
	public float attackLength = 1f;
	public float attackDistance = 5f;
	public MapGenerator map;
	public Animator shiftEffect;
	public Animator[] animators;
	public Transform[] sprites;
	public float invulTime = 2f;
	public float lastInvul = 0f;

	public Transform cameraPos;
	public AudioClip slash;
	public AudioClip dodgeroll;
	public AudioClip death;
	public AudioClip bite;
	public AudioClip shift;

	public PlayerAttackCollider biteCollider;
	public PlayerAttackCollider slashCollider;


	Rigidbody2D body;
	float normalScaleX;
	bool moveLocked;
	bool shifting = false;
	bool dodgeLock = false;
	bool specialLock = false;
	bool invulnerable = false;

	float normalSlashScaleX;

	float lastHit = 0f;
	float hitCool = 1f;

	float slashAnimationLength;
	float shiftAnimationLength;
	float dustAnimationLength;
	float biteAnimationLength;
	Form[] forms;
	Form currentForm;
	Animator currentAnimator;
	Transform currentSprites;

	Transform FelineSprites;
	Transform CanineSprites;


	float lastPrint = 0f;
	float timeBetween = .5f;







	void Awake(){

		forms = new Form[4];
		forms [0] = new Form(new AttackType (1f, "FelineAttack", .25f), "Feline", 30f, /*dodge*/ 14f, 1f, 5f, "SlashEffect");
		forms [1] = new Form(new AttackType (1f, "CanineAttack", .4f), "Canine", 25f, /*dodge*/ 8f, .8f, 10f, "BiteEffect");
		forms [2] = new Form(new AttackType (1f, "UrsineAttack", .6f), "Ursine", 20f, /*dodge*/ 5f, .8f, 1f, "SlashEffect");
		forms [3] = new Form(new AttackType (1f, "SerpentAttack", .3f), "Serpent", 25f, /*dodge*/ 6f, 0.2f, 2f, "StrikeEffect");

		currentForm = forms [0];

		currentSprites = sprites [0];

		FelineSprites = transform.FindChild ("FelineSprites");
		CanineSprites = transform.FindChild ("CanineSprites");

	}

	protected override void Start () {
		base.Start ();
		body = GetComponent<Rigidbody2D> ();

		normalScaleX = currentSprites.localScale.x;
		moveLocked = false;

		foreach (AnimationClip clip in animators[0].runtimeAnimatorController.animationClips) {
			if (clip.name == forms [0].name + "Attack") {
				forms [0].attack.animationLength = clip.length;
			}
			if (clip.name == forms [0].name + "Dodge") {
				forms [0].dodgeAnimationLength = clip.length;
			}
			if (clip.name == forms [0].name + "Special") {
				forms [0].specialAnimationLength = clip.length;
			}

		}

		foreach (AnimationClip clip in animators[1].runtimeAnimatorController.animationClips) {
			if (clip.name == forms [1].name + "Attack") {
				forms [1].attack.animationLength = clip.length;
			}
			if (clip.name == forms [1].name + "Dodge") {
				forms [1].dodgeAnimationLength = clip.length;
			}
		}

	
		foreach(AnimationClip clip in attackEffects.runtimeAnimatorController.animationClips){
			if (clip.name == "SlashEffect") {
				slashAnimationLength = clip.length;
				forms [0].attack.effectAnimLength = clip.length;
			}
			if (clip.name == "BiteEffect") {
				biteAnimationLength = clip.length;
				forms [1].attack.effectAnimLength = clip.length;
			}
		}

		foreach(AnimationClip clip in shiftEffect.runtimeAnimatorController.animationClips){
			if (clip.name == "Shift") {
				shiftAnimationLength = clip.length;
			}
			if (clip.name == "DustEffect") {
				dustAnimationLength = clip.length;
			}
		}

		currentAnimator = animators [0];

	}

	void FixedUpdate () {

		if (!moveLocked) {
			Vector2 adjustedVelocity = deliberateVelocity.normalized;
			body.MovePosition (body.position + adjustedVelocity * Time.fixedDeltaTime * playerSpeed);
		}
	}

	public Vector2 GetDeliberateVelocity(){
		return deliberateVelocity;
	}

	public void UpdateAnimator(Vector3 point){
		//this is all wrong. I could start an attack and flip the sprite mid-strike like this
		if (moveLocked) {

			currentAnimator.SetBool ("Running", false);



			float dif = point.x - transform.position.x;

			if (dif > 0) {
				currentSprites.localScale = new Vector3 (normalScaleX * -1, currentSprites.localScale.y, 0f);
			} else {
				currentSprites.localScale = new Vector3 (normalScaleX * 1, currentSprites.localScale.y, 0f);
			}
		

			
		}
			
		if(!moveLocked){

			if (deliberateVelocity.x == 0 && deliberateVelocity.y == 0) {
				currentAnimator.SetBool ("Idle", true);
				currentAnimator.SetBool ("Running", false);
			} else if (deliberateVelocity.x != 0 || deliberateVelocity.y != 0) {
				currentAnimator.SetBool ("Running", true);
				currentAnimator.SetBool ("Idle", false);
			}
		}

	}

	void LateUpdate(){
		if (!moveLocked) {
			

			if (deliberateVelocity.x > 0) {
			
				currentSprites.localScale = new Vector3 (normalScaleX * -1, currentSprites.localScale.y, 0f);


			} else {
				currentSprites.localScale = new Vector3 (normalScaleX, currentSprites.localScale.y, 0f);

			}
		}

	}

	public void SetDeliberateVelocity(Vector2 velocity){
		deliberateVelocity = velocity;

	}

	public void FelineShift(){
		if (shifting) {return;}

		shifting = true;
		moveLocked = true;
		StartCoroutine (Shifting ());
		shiftEffect.SetTrigger ("ShiftEffect");
		FelineSprites.gameObject.SetActive (true);
		CanineSprites.gameObject.SetActive (false);

		currentAnimator = animators [0];
		currentForm = forms [0];
		currentSprites = sprites [0];


	}

	public void ReportCollision(Collider2D collision){
		if (Time.time < lastHit + hitCool) {return;}

		lastHit = Time.time;

		if (collision.gameObject.tag == "Enemy") {
			if (collision.gameObject != null) {
				collision.gameObject.SendMessage ("TakeHit", 5f);
			}
		}
	}

	public void TakeHit(float damage){
		if (invulnerable == true) {return;}

		health -= damage;
		if (health <= 0 && !dead) {
			Die ();
		}

		invulnerable = true;
		lastInvul = Time.time;
		StartCoroutine (DamageFlash ());
		StartCoroutine (InvulnerableTimer ());
	}

	IEnumerator DamageFlash(){
		float percent = 0;
		float flashTime = 2f;
		float flashSpeed = 10f;
		SpriteRenderer renderer = currentSprites.GetComponent<SpriteRenderer> ();

		while (percent < 1) {
			percent += Time.deltaTime * 1f / flashTime;
			renderer.color = Color.Lerp (Color.white, Color.red, Mathf.PingPong (percent * flashSpeed, 1));
			yield return null;
		}
		renderer.color = Color.white;
	}

	IEnumerator InvulnerableTimer(){
		while (Time.time < lastInvul + invulTime) {
			yield return null;
		}


		invulnerable = false;
	}

	public void CanineShift(){
		if (shifting) {return;}

		shifting = true;
		moveLocked = true;
		StartCoroutine (Shifting ());
		shiftEffect.SetTrigger ("ShiftEffect");
		FelineSprites.gameObject.SetActive (false);
		CanineSprites.gameObject.SetActive (true);

		currentAnimator = animators [1];
		currentForm = forms [1];
		currentSprites = sprites [1];


	}

	IEnumerator Shifting(){
		AudioSource.PlayClipAtPoint (shift, cameraPos.position);
		float finishTime = Time.time + shiftAnimationLength;

		while (Time.time <= finishTime) {

			//maybe do more

			yield return null;
		}

		shifting = false;
		moveLocked = false;

	}

	public void Dodge(Vector3 awayFrom){
		if (moveLocked || shifting || dodgeLock) {return;}

		moveLocked = true;
		dodgeLock = true;
		Vector3 direction = (transform.position - awayFrom).normalized;
		StartCoroutine (Dodging (direction));


	}

	IEnumerator Dodging(Vector3 direction){
		float dodgeTime = .2f;
		float percent = 0f;
		float dodgeModifier = currentForm.dodgeAnimationLength / dodgeTime;
		currentAnimator.SetFloat ("DodgeSpeed", dodgeModifier);
		currentAnimator.SetTrigger ("Dodge");
		Vector3 destination = direction * currentForm.dodgeDistance;
		Vector2 oldPosition = new Vector2 (transform.position.x, transform.position.y);

		while (percent < 1) {
			percent += Time.deltaTime * 1f / dodgeTime;
			Vector2 newPosition = oldPosition + new Vector2 (direction.x, direction.y) * currentForm.dodgeDistance * percent;
			body.MovePosition (newPosition);
			//= transform.position + direction * currentForm.dodgeDistance * percent;
			yield return null;
		}
		moveLocked = false;
		yield return new WaitForSeconds (currentForm.dodgeCooldown);
		dodgeLock = false;
	}

	public void Special(){
		if(currentForm == forms[0]){FelineDodgeRoll ();}
	}

	public void FelineDodgeRoll(){
		if (moveLocked || shifting || dodgeLock) {return;}
		if (deliberateVelocity.x == 0 && deliberateVelocity.y == 0) {return;}

		moveLocked = true;
		dodgeLock = true;
		specialLock = true;
		Vector3 direction = (new Vector3(deliberateVelocity.x, deliberateVelocity.y, 0)).normalized;
		AudioSource.PlayClipAtPoint (dodgeroll, cameraPos.position);
		StartCoroutine (DodgeRoll (direction));
	}

	IEnumerator DodgeRoll(Vector3 direction){
		float dodgeTime = .4f;
		float percent = 0f;
		float slideTime = 0.5f;
		float dodgeModifier = currentForm.specialAnimationLength / dodgeTime;
		currentAnimator.SetFloat ("SpecialSpeed", dodgeModifier);
		currentAnimator.SetTrigger ("Special");
		Vector2 oldPosition = new Vector2 (transform.position.x, transform.position.y);

		while (percent < 1) {
			percent += Time.deltaTime * 1f / dodgeTime;
			Vector2 newPosition = oldPosition + new Vector2 (direction.x, direction.y) * 19f * percent;
			body.MovePosition (newPosition);
			//= transform.position + direction * currentForm.dodgeDistance * percent;
			if(percent > .95){currentAnimator.SetBool ("EndDodgeRoll", true);}
			yield return null;
		}

		percent = 0f;

		float dustModifier = dustAnimationLength / slideTime;
		shiftEffect.SetFloat ("DustModifier", dustModifier);
		currentAnimator.SetBool ("EndDodgeRoll", true);
		shiftEffect.SetTrigger ("DustEffect");
		oldPosition = new Vector2 (transform.position.x, transform.position.y);

		while (percent < 1) {
			percent += Time.deltaTime * 1f / slideTime;
			Vector2 newPosition = oldPosition + new Vector2 (direction.x, direction.y) * 3f * percent;
			body.MovePosition (newPosition);
			//= transform.position + direction * currentForm.dodgeDistance * percent;
			yield return null;
		}

		currentAnimator.SetBool ("EndDodgeRoll", false);
		moveLocked = false;
		dodgeLock = false;
		yield return new WaitForSeconds (currentForm.specialCooldown);
		specialLock = false;
	}


	public void PrimaryAttack(Vector3 point){

		if(!moveLocked){
			
			moveLocked = true;
			currentAnimator.SetBool ("Running", false);
			Vector3 direction = new Vector3 (point.x, point.y, transform.position.z) - transform.position;
			float angle = (Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg);

			if (currentForm == forms [0]) {
				slashCollider.catchAttacks = true;
				AudioSource.PlayClipAtPoint (slash, cameraPos.position);

			}
			if (currentForm == forms [1]) {
				biteCollider.catchAttacks = true;
				AudioSource.PlayClipAtPoint (bite, cameraPos.position);
			}

			attackEffects.transform.rotation = Quaternion.Euler (0, 0, angle);
			attackEffects.transform.position = transform.position + (point - transform.position).normalized * attackDistance;

			StartCoroutine (Attack());

		/*
		Quaternion hitRotation = new Quaternion ();
		hitRotation.SetFromToRotation (Vector3.zero, point);
		hitRotation.eulerAngles += new Vector3 (0, 0, 75);
*/
	
		}


	}

	IEnumerator Attack(){

		float playerModifier = currentForm.attack.animationLength / currentForm.attack.desiredAttackLength;
		currentAnimator.SetFloat ("AttackSpeed", playerModifier);
		float effectModifier = currentForm.attack.effectAnimLength / currentForm.attack.desiredAttackLength;
		attackEffects.SetFloat ("SlashModifier", effectModifier);
		attackEffects.SetFloat ("BiteModifier", effectModifier);

		float finishTime = Time.time + currentForm.attack.desiredAttackLength;

		attackEffects.SetTrigger (currentForm.effectName);
		currentAnimator.SetTrigger ("Attack");

		while (Time.time <= finishTime) {

			//maybe do more

			yield return null;
		}

		moveLocked = false;
		slashCollider.catchAttacks = false;
		biteCollider.catchAttacks = false;


	}



	public class AttackType{
		public float desiredAttackLength;
		public float animationLength;
		public float effectAnimLength;
		public string animationString;

		public AttackType(float length, string aString, float desLength){
			animationLength = length;
			animationString = aString;
			desiredAttackLength = desLength;
			effectAnimLength = 1f;
		}
	}

	public class Form{
		public AttackType attack;
		public string name;
		public string effectName;
		public float baseSpeed;
		public float dodgeDistance;
		public float dodgeAnimationLength;
		public float dodgeCooldown;
		public float specialAnimationLength;
		public float specialCooldown;

		public Form(AttackType _attack, string _name, float speed, float dodge, float _dodgeCooldown, float _specialCooldown, string _effectName){
			attack = _attack;
			name = _name;
			baseSpeed = speed;
			dodgeDistance = dodge;
			dodgeAnimationLength = 1f;
			dodgeCooldown = _dodgeCooldown;
			specialAnimationLength = 1f;
			specialCooldown = _specialCooldown;
			effectName = _effectName;
		}
	}



		

	public override void Die(){
		base.Die ();
		currentSprites.GetComponent<SpriteRenderer>().enabled = false;
		AudioSource.PlayClipAtPoint (death, cameraPos.position);
		moveLocked = true;
		dodgeLock = true;
		specialLock = true;
		shifting = true;
	}
}

