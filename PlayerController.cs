using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {


	public Rigidbody2D body;


	Vector2 velocity;
	Player myPlayer;
	Plane sightPlane;
	Camera mainCamera;

	// Use this for initialization
	void Start () {

		myPlayer = GetComponent<Player> ();
		mainCamera = Camera.main;
		sightPlane = new Plane (Vector3.back, myPlayer.transform.position.z * Vector3.forward);
	}

	void Update(){
		velocity = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		myPlayer.SetDeliberateVelocity (velocity);
	}


	void LateUpdate () {
		



		Ray mouseRay = mainCamera.ScreenPointToRay (Input.mousePosition);
		float rayDistance;

		if (sightPlane.Raycast (mouseRay, out rayDistance)) {
			Vector3 lookPoint = mouseRay.GetPoint (rayDistance);
			//myPlayer.LookAt (lookPoint);
			myPlayer.UpdateAnimator(lookPoint);

			if(Input.GetMouseButton(0)){
				myPlayer.PrimaryAttack(lookPoint);
			}

			if (Input.GetButtonDown ("Feline Shift")) {
				myPlayer.FelineShift ();
			}
			if (Input.GetButtonDown ("Canine Shift")) {
				myPlayer.CanineShift ();
			}

			if (Input.GetButtonDown ("Dodge")) {
				myPlayer.Dodge (lookPoint);
			}
			if (Input.GetButtonDown ("Special")) {
				myPlayer.Special ();
			}
		}




	}

	void FixedUpdate(){


	}





}
