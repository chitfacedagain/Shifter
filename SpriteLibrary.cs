using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SpriteLibrary : MonoBehaviour {

	public Sprite[] jungleFloorTiles;
	public Sprite[] jungleWallTiles;
	public Sprite[] jungleTopperTiles;
	public Sprite[] jungleClosedTiles;

	public Sprite[] desoFloorTiles;
	public Sprite[] desoWallTiles;
	public Sprite[] desoTopperTiles;


	public Sprite getRandomDesoFloor(){
		float fRoll = UnityEngine.Random.Range (0, 1f);
		if (fRoll > 0.15f) {
			return desoFloorTiles [0];
		} 
		int roll = UnityEngine.Random.Range (0, desoFloorTiles.Length);
		return desoFloorTiles [roll];

	}

	public Sprite getRandomDesoWall(){
		int roll = UnityEngine.Random.Range (0, desoWallTiles.Length);
		return desoWallTiles [roll];
	}

	public Sprite getRandomDesoTopper(){
		return desoTopperTiles [0];
	}


	public Sprite getRandomJungleFloor(){
		int roll = UnityEngine.Random.Range (0, jungleFloorTiles.Length);
		return jungleFloorTiles [roll];
	}

	public Sprite getRandomJungleWall(){
		int roll = UnityEngine.Random.Range (0, jungleWallTiles.Length);
		return jungleWallTiles [roll];
	}

	public Sprite getRandomJungleClosed(){
		return jungleClosedTiles [0];
		float roll = UnityEngine.Random.Range (0, 1f);
		if (roll > .2f) {
			return jungleClosedTiles [0];
		} else {
			int roll2 = UnityEngine.Random.Range (1, jungleClosedTiles.Length);
			return jungleClosedTiles [roll2];
		}
	}

	public Sprite getRandomJungleTopper(){
		return jungleTopperTiles [0];
		float roll = UnityEngine.Random.Range (0, 1);
		if (roll > .2f) {
			return jungleTopperTiles[0];
		}
		int roll2 = UnityEngine.Random.Range (1, jungleTopperTiles.Length);
		return jungleTopperTiles [roll2];
	}

}
