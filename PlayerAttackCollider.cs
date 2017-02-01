using UnityEngine;
using System.Collections;

public class PlayerAttackCollider : MonoBehaviour {

	public bool catchAttacks = false;
	public Player player;
	/*
	void OnCollisionStay2d(Collision2D collision){
		if (catchAttacks) {
			player.ReportCollision (collision);
		}
	}
	*/

	void OnTriggerStay2D(Collider2D collision){
		if (catchAttacks) {
			player.ReportCollision (collision);
		}
	}





}
