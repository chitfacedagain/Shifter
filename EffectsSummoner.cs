using UnityEngine;
using System.Collections;

public class EffectsSummoner : MonoBehaviour {

	public GameObject danger_Circle;

	public void SummonDangerCircle(Vector3 _position, float _scale, float duration){
		Transform newCircle = Instantiate (danger_Circle, _position, Quaternion.identity) as Transform;
		Vector3 scale = new Vector3(_scale, _scale, 0f);
		newCircle.localScale = scale;
		StartCoroutine (HandleThisCircle (newCircle, duration));
	}

	IEnumerator HandleThisCircle(Transform circle, float duration){
		SpriteRenderer renderer = circle.GetComponent<SpriteRenderer> ();
		float fullRedTime = 0.3f;
		float percent = 0f;

		while (percent < 1f) {
			percent += Time.fixedDeltaTime * 1f / duration;
			renderer.color = Color.Lerp (Color.white, Color.red, percent / fullRedTime);
			yield return null;
		}

		Destroy (circle.gameObject);
	}


	public void SummonFollowingDangerCircle(Transform target, float _scale, float duration){
		GameObject newCircleObj = Instantiate (danger_Circle, target.position, Quaternion.identity) as GameObject;
		Transform newCircle = newCircleObj.transform;
		Vector3 scale = new Vector3(_scale, _scale, 0f);
		newCircle.localScale = scale;
		StartCoroutine (HandleThisFollowingCircle (newCircle, target, duration));
	}

	IEnumerator HandleThisFollowingCircle(Transform circle, Transform target, float duration){
		SpriteRenderer renderer = circle.GetComponent<SpriteRenderer> ();
		float fullRedTime = 0.3f;
		float percent = 0f;

		while (percent < 1f) {
			percent += Time.fixedDeltaTime * 1f / duration;
			//renderer.color = Color.Lerp (Color.white, Color.red, percent / fullRedTime);
			renderer.color = Color.Lerp (Color.white, Color.red, percent);
			circle.position = target.position - new Vector3(0, 1.5f, 0);
			yield return null;
		}

		Destroy (circle.gameObject);
	}

}
