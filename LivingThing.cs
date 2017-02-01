using UnityEngine;
using System.Collections;

public class LivingThing : MonoBehaviour {

	public float maxHealth = 10f;
	public float health;
	protected bool dead;

	public event System.Action<object> OnDeath;


	protected virtual void Start () {
		health = maxHealth;
	}

	/*
	public virtual void TakeHit(float rawDamage){
		health -= rawDamage;

		if (health <= 0 && !dead) {
			Die ();
		}
	}
	*/

	[ContextMenu("Kill")]
	public virtual void Die(){
		dead = true;
		if (OnDeath != null) {
			OnDeath (this);
		}

	}
}
