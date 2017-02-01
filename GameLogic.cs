using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameLogic : MonoBehaviour {

	public Image fader;
	public Text levelText;
	public Canvas levelCanvas;

	List<GameObject> activeMonsters;
	MapGenerator mapGen;
	Player player;
	GameObject interludeUI;

	int currentLevel;
	int l1Monsters = 3;
	int l2Monsters = 5;

	int totalJungleTiles = 500;
	int totalDesoTiles = 850;

	string l1Name = "Jungle";
	string l2Name = "Desolation";


	void Start(){
		activeMonsters = new List<GameObject> ();
		mapGen = FindObjectOfType<MapGenerator> ();
		player = FindObjectOfType<Player> ();
		interludeUI = GameObject.Find ("Interlude UI");
		currentLevel = 1;
		StartNextLevel ();
	}

	public void ReportDeath(GameObject monster){
		activeMonsters.Remove (monster);

		if (activeMonsters.Count < 1) {
			LevelFinish ();
		}
	}

	void LevelFinish(){
		if (currentLevel == 1) {
			currentLevel++;
			StartNextLevel ();
		}
	}

	void StartNextLevel(){
		if (currentLevel == 1) {
			interludeUI.SetActive (true);
			levelText.text = "- Level 1: " + l1Name + " -";
			mapGen.DestroyCurrentLevel ();
			StartCoroutine (fadeOut ());
		}
		if (currentLevel == 2) {
			interludeUI.SetActive (true);
			levelText.text = "- Level 2: " + l2Name + " -";
			mapGen.DestroyCurrentLevel ();
			StartCoroutine (fadeOut ());
		}
	}

	void GenerateNextLevel(){
		if (currentLevel == 1) {
			mapGen.CreateJungleLevel (totalJungleTiles);
			activeMonsters = mapGen.PopulateMonsters (l1Monsters);
			player.transform.position = mapGen.CoordsToWorldPosition (mapGen.firstSquare.x, mapGen.firstSquare.y);
		}
		if (currentLevel == 2) {
			mapGen.CreateDesoLevel (totalDesoTiles);
			activeMonsters = mapGen.PopulateMonsters (l2Monsters);
			player.transform.position = mapGen.CoordsToWorldPosition (mapGen.firstSquare.x, mapGen.firstSquare.y);
		}
	}

	IEnumerator fadeOut(){
		float solidTime = 3f;
		float fadeTime = 2f;
		float percent = 0f;

		bool loaded = false;

		yield return new WaitForSeconds (solidTime);

		while (percent < 1f) {
			percent += Time.deltaTime / fadeTime;
			fader.color = Color.Lerp (new Color (0, 0, 0, 1f), Color.clear, percent);
			levelText.color = Color.Lerp (new Color (1f, 1f, 1f, 1f), Color.clear, percent);
			if (percent > 0.1f && loaded == false) {
				loaded = true;
				GenerateNextLevel ();
			}
			yield return null;

		}

		interludeUI.SetActive (false);
		fader.color = new Color (0, 0, 0, 1f);
		levelText.color = (new Color (1f, 1f, 1f, 1f));
	}

}
