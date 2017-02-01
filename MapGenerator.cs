using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {



	public int area;
	public float squareSize = 4;

	public TileType[] tiles;

	public float chanceToTurnLeft = 0.7f;
	public float chanceToTurnRight = 0.3f;
	public float chanceToTurn180 = 0.1f;
	public float chanceForFlorescence = 0.2f;
	public float florescencePosMax = 20f;
	public float florescenceRotMax = 360f;


	public int[,] intMap;
	public MapSquare[,] mapGrid;
	public List<MapSquare> openSquares;
	//public List<MapSquare> allSquares;
	public Map currentMap;
	public Angle[] angles;
	public Angle[,] angleHolder;
	public MapSquare firstSquare;
	public Transform floorTile;
	public Transform closedTile;
	public Transform wallTile;
	public Transform topperTile;

	public SpriteLibrary spriteLibrary;
	public Camera1 camera1;

	public Mudman mudman;
	public Rockman rockman;


	List<MapSquare> bottomWalls;
	List<MapSquare> leftAndRightWalls;

	Sprite minimap;
	Texture2D minimapTexture;
	Transform mapHolder;

	int totalForestTiles;
	int mapSizeX;
	int mapSizeY;
	float totalChance;
	Color jungleBackground;
	Color desoBackground;

	string holderName = "Current Map";





	public Vector2 IntToPosition(int x, int y){
		return new Vector2 ((-mapSizeX / 2f + 0.5f + x) * squareSize, (-mapSizeY / 2f + 0.5f + y) * squareSize);
	}

	public Vector3 CoordsToWorldPosition(int x, int y){
		return new Vector3 ((-mapSizeX / 2f + 0.5f + x) * squareSize, (-mapSizeY / 2f + 0.5f + y) * squareSize, 0 );
	}

	public int[] PositionToGridInt(Vector3 position){

		int x = Mathf.RoundToInt(position.x/squareSize + mapSizeX / 2f - 0.5f);
		int y = Mathf.RoundToInt(position.y/squareSize + mapSizeY / 2f - 0.5f);
		return new int[] { x, y };
	}

	public void GenerateForestMap(){

		int count = 0;
		bool[,] filledTracker = new bool[totalForestTiles * 2, totalForestTiles * 2];
		List<MapSquare> newSquares = new List<MapSquare>();
		Vector2 currentPaintingDirection = new Vector2 (1, -1);
		Vector2 currentGraphicalDirection = new Vector2 (1, 0);
		Vector2 currentPosition = new Vector2 (0, 0);

		int randomStartDir = UnityEngine.Random.Range (0, 3);
		Angle startAngle = angles [randomStartDir];
		currentPaintingDirection = startAngle.paint;
		currentGraphicalDirection = startAngle.graph;

		while (count < totalForestTiles) {
			float tileRoll = UnityEngine.Random.Range (0, totalChance);

			TileType nextType = GetRandomTileType (tileRoll);
			currentPosition = currentPosition + currentGraphicalDirection;
			Vector2 tempPos = currentPosition;
			for (int x = 0; x < nextType.xSize; x++) {

				for (int y = 0; y < nextType.ySize; y++) {

					tempPos = currentPosition + new Vector2 (currentPaintingDirection.x * x, currentPaintingDirection.y * y);

					if (!filledTracker [Mathf.RoundToInt(tempPos.x) + totalForestTiles, Mathf.RoundToInt(tempPos.y) + totalForestTiles]) {

						MapSquare newSquare = new MapSquare (tempPos, 0, Mathf.RoundToInt(tempPos.x), Mathf.RoundToInt(tempPos.y));
						newSquares.Add (newSquare);
						openSquares.Add (newSquare);
						filledTracker [Mathf.RoundToInt(tempPos.x) + totalForestTiles, Mathf.RoundToInt(tempPos.y) + totalForestTiles] = true;
						if (count == 0) {
							firstSquare = newSquare;
						}
						count++;
					}



				}

			}

			currentPosition = tempPos;

			Vector2[] newDirections = RollNewDirections (currentPaintingDirection, currentGraphicalDirection);
			currentGraphicalDirection = newDirections [1];
			currentPaintingDirection = newDirections [0];

		}

		int topX = 0;
		int topY = 0;
		int bottomX = 0;
		int bottomY = 0;
		foreach (MapSquare square in newSquares) {
			if(square.x > topX){ topX = square.x;}
			if(square.y > topY){ topY = square.y;}
			if(square.x < bottomX){ bottomX = square.x;}
			if(square.y < bottomY){ bottomY = square.y;}
		}
		int totalX = topX - bottomX +3;
		int totalY = topY - bottomY +3;
		mapGrid = new MapSquare[totalX, totalY];
		for (int x = 0; x < totalX; x++) {
			for (int y = 0; y < totalY; y++) {
				mapGrid [x, y] = new MapSquare (new Vector2 (x, y), 1, x, y);
			}
		}

		foreach (MapSquare square in newSquares) {
			square.x = square.x - bottomX + 1;
			square.y = square.y - bottomY + 1;
			square.position = new Vector2 (square.x, square.y);
			mapGrid [square.x, square.y] = square;
		}

	}

	public void MarkWalls(){

		foreach (MapSquare square in openSquares) {
			if (mapGrid [square.x, square.y + 1].filled == 1) {
				mapGrid [square.x, square.y + 1].filled = 2;
			}
			if (mapGrid [square.x, square.y - 1].filled == 1) {
				bottomWalls.Add (mapGrid [square.x, square.y - 1]);
			}
			if (mapGrid [square.x + 1, square.y].filled == 1) {
				leftAndRightWalls.Add (mapGrid [square.x + 1, square.y]);
			}
			if (mapGrid [square.x - 1, square.y].filled == 1) {
				leftAndRightWalls.Add (mapGrid [square.x - 1, square.y]);
			}
		}

	}

	public void GenerateJungleTiles(){
		
		MarkWalls ();

		foreach (MapSquare square in mapGrid) {
			
			if (square.filled == 0) {
				Transform newTile = Instantiate (floorTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTile.parent = mapHolder;
				newTile.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer sprite = newTile.gameObject.GetComponent<SpriteRenderer> ();
				sprite.sprite = spriteLibrary.getRandomJungleFloor ();
			}
			if (square.filled == 1) {
				Transform newTile = Instantiate (closedTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTile.parent = mapHolder;
				newTile.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer sprite = newTile.gameObject.GetComponent<SpriteRenderer> ();
				sprite.sprite = spriteLibrary.getRandomJungleClosed ();
				sprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);


			}

			if (square.filled == 2) {
				Transform newTile = Instantiate (wallTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTile.parent = mapHolder;
				newTile.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer sprite = newTile.gameObject.GetComponent<SpriteRenderer> ();
				sprite.sprite = spriteLibrary.getRandomJungleWall ();
				sprite.color = new Color (0.9f, 0.9f, 0.9f, 1f);
				newTile.parent = mapHolder;

				Transform newTopper = Instantiate (topperTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTopper.parent = mapHolder;
				newTopper.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer topperSprite = newTopper.gameObject.GetComponent<SpriteRenderer> ();
				topperSprite.sprite = spriteLibrary.getRandomJungleTopper ();
				newTopper.position = newTopper.position + new Vector3 (0, 0.7f * squareSize, 0);
				topperSprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);


			}
		}

		foreach (MapSquare square in bottomWalls) {
			if (square.filled == 1) {
				Transform newTopper = Instantiate (topperTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTopper.parent = mapHolder;
				newTopper.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer topperSprite = newTopper.gameObject.GetComponent<SpriteRenderer> ();
				topperSprite.sprite = spriteLibrary.getRandomJungleTopper ();
				newTopper.position = newTopper.position + new Vector3 (0, 0.7f * squareSize, 0);
				topperSprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);


			}
		}
		foreach (MapSquare square in leftAndRightWalls) {
			if (square.filled == 1) {
				Transform newTopper = Instantiate (topperTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTopper.parent = mapHolder;
				newTopper.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer topperSprite = newTopper.gameObject.GetComponent<SpriteRenderer> ();
				topperSprite.sprite = spriteLibrary.getRandomJungleTopper ();
				//newTopper.position = newTopper.position + new Vector3 (0, 0.9f * squareSize, 0);
				topperSprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);


			}
		}

		camera1.SetBackgroundColor (jungleBackground);

	}

	public void GenerateDesoTiles(){

		MarkWalls ();

		foreach (MapSquare square in mapGrid) {

			if (square.filled == 0) {
				Transform newTile = Instantiate (floorTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTile.parent = mapHolder;
				newTile.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer sprite = newTile.gameObject.GetComponent<SpriteRenderer> ();
				sprite.sprite = spriteLibrary.getRandomDesoFloor ();


			}
			if (square.filled == 1) {
				Transform newTile = Instantiate (closedTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTile.parent = mapHolder;
				newTile.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer sprite = newTile.gameObject.GetComponent<SpriteRenderer> ();
				sprite.sprite = spriteLibrary.getRandomDesoTopper ();
				//sprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);



			}

			if (square.filled == 2) {
				Transform newTile = Instantiate (wallTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTile.parent = mapHolder;
				newTile.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer sprite = newTile.gameObject.GetComponent<SpriteRenderer> ();
				sprite.sprite = spriteLibrary.getRandomDesoWall ();
				//sprite.color = new Color (0.9f, 0.9f, 0.9f, 1f);



				Transform newTopper = Instantiate (topperTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTopper.parent = mapHolder;
				newTopper.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer topperSprite = newTopper.gameObject.GetComponent<SpriteRenderer> ();
				topperSprite.sprite = spriteLibrary.getRandomDesoTopper ();
				newTopper.position = newTopper.position + new Vector3 (0, 0.7f * squareSize, 0);
				//topperSprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);


			}
		}

		foreach (MapSquare square in bottomWalls) {
			if (square.filled == 1) {
				Transform newTopper = Instantiate (topperTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTopper.parent = mapHolder;
				newTopper.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer topperSprite = newTopper.gameObject.GetComponent<SpriteRenderer> ();
				topperSprite.sprite = spriteLibrary.getRandomDesoTopper ();
				newTopper.position = newTopper.position + new Vector3 (0, 0.7f * squareSize, 0);
				//topperSprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);


			}
		}
		foreach (MapSquare square in leftAndRightWalls) {
			if (square.filled == 1) {
				Transform newTopper = Instantiate (topperTile, CoordsToWorldPosition (square.x, square.y), Quaternion.identity) as Transform;
				newTopper.parent = mapHolder;
				newTopper.localScale = new Vector3 (squareSize, squareSize, 0);
				SpriteRenderer topperSprite = newTopper.gameObject.GetComponent<SpriteRenderer> ();
				topperSprite.sprite = spriteLibrary.getRandomDesoTopper ();
				//newTopper.position = newTopper.position + new Vector3 (0, 0.9f * squareSize, 0);
				//topperSprite.color = new Color (0.8f, 0.8f, 0.8f, 1f);

			}
		}

		camera1.SetBackgroundColor (desoBackground);
	}

	public void FindOpenSpaces(){

		MapSquare start = firstSquare;

		foreach (MapSquare square in openSquares) {
			if (mapGrid [square.x, square.y + 1].filled == 2) {
				start = mapGrid [square.x, square.y];
				break;
			}
		}

		Queue<MapSquare> searchQueue = new Queue<MapSquare> ();
		searchQueue.Enqueue (start);
		bool[,] closedSquares = new bool[mapGrid.GetLength (0), mapGrid.GetLength (1)];

		while (searchQueue.Count > 0) {
			MapSquare currentSquare = searchQueue.Dequeue();
			int neighboringWalls = 0;
			int distanceFromWall = 20;

			for (int x = -1; x <= 1; x ++) {
				for (int y = -1; y <= 1; y ++) {
					int neighbourX = currentSquare.x + x;
					int neighbourY = currentSquare.y + y;
					if (mapGrid [neighbourX, neighbourY].filled == 1 || mapGrid [neighbourX, neighbourY].filled == 2) {
						neighboringWalls++;
						distanceFromWall = 0;
					}
					if (x == 0 || y == 0) {
						if (mapGrid [neighbourX, neighbourY].filled == 0 && closedSquares[neighbourX,neighbourY] == false) {
							searchQueue.Enqueue (mapGrid [neighbourX, neighbourY]);
							closedSquares [neighbourX, neighbourY] = true;
						}
					}

				}
			}

			if (distanceFromWall == 0) {
				currentSquare.distanceFromWall = 0;
				currentSquare.neighboringWalls = neighboringWalls;
			}

			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					int neighbourX = currentSquare.x + x;
					int neighbourY = currentSquare.y + y;
					if (currentSquare.distanceFromWall + 1 < mapGrid [neighbourX, neighbourY].distanceFromWall) {
						mapGrid [neighbourX, neighbourY].distanceFromWall = currentSquare.distanceFromWall + 1;
					}
				}
			}

		}


	}

	public void FindOpenSpacePerfect(){

		List<MapSquare> openList = new List<MapSquare> ();
		List<MapSquare> adjacentList = new List<MapSquare> ();

		foreach (MapSquare square in openSquares) {
			openList.Add (square);
		}

		//find those next to walls
		foreach (MapSquare currentSquare in openList) {
			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					int neighbourX = currentSquare.x + x;
					int neighbourY = currentSquare.y + y;
					if (mapGrid [neighbourX, neighbourY].filled == 1 || mapGrid [neighbourX, neighbourY].filled == 2) {
						if (x == 0 || y == 0) {
							currentSquare.distanceFromWall = 0;
						}
					}
				}
			}
			if (currentSquare.distanceFromWall == 0) {
				adjacentList.Add (currentSquare);
			}
		}

		foreach (MapSquare square in adjacentList) {
			openList.Remove (square);
		}

		List<MapSquare> tempList1 = adjacentList;


		while (openList.Count > 0) {
			List<MapSquare> tempList2 = new List<MapSquare> ();

			//find those next to the tempList, who are not in the tempList
			foreach (MapSquare currentSquare in tempList1) {
				for (int x = -1; x <= 1; x++) {
					for (int y = -1; y <= 1; y++) {
						int neighbourX = currentSquare.x + x;
						int neighbourY = currentSquare.y + y;
						if (mapGrid [neighbourX, neighbourY].filled == 0 && mapGrid [neighbourX, neighbourY].distanceFromWall == -1) {
							if (x == 0 || y == 0) {
								mapGrid [neighbourX, neighbourY].distanceFromWall = currentSquare.distanceFromWall + 1;
								openList.Remove (mapGrid [neighbourX, neighbourY]);
								tempList2.Add (mapGrid [neighbourX, neighbourY]);
							}
						}
					}
				}
			}

			tempList1.Clear ();
			foreach (MapSquare square in tempList2) {
				tempList1.Add (square);
			}

		}


	}

	Vector2[] RollNewDirections( Vector2 currentPaintingDirection, Vector2 currentGraphicalDirection){

		Vector2 newPainting = new Vector2 (0, 0);
		Vector2 newGraphical = new Vector2 (0, 0);
		Vector2[] newDirections = new Vector2[2];

		float roll = UnityEngine.Random.Range (0, 1f);

		//print ("current X: " + currentGraphicalDirection.x + "current Y: " + (currentGraphicalDirection.y));
		int cgdX = Mathf.RoundToInt (currentGraphicalDirection.x);
		int cgdY = Mathf.RoundToInt (currentGraphicalDirection.y);


		if (roll > chanceToTurnRight && roll <= chanceToTurnLeft) {
			newPainting = angleHolder [cgdX + 1, cgdY + 1].left.paint;
			newGraphical = angleHolder [cgdX + 1, cgdY + 1].left.graph;
		} else if(roll > chanceToTurn180 && roll <= chanceToTurnRight) {
			newPainting = angleHolder [cgdX + 1, cgdY + 1].right.paint;
			newGraphical = angleHolder [cgdX + 1, cgdY + 1].right.graph;
		} else if (roll < chanceToTurn180) {
			newPainting = currentPaintingDirection * -1f;
			newGraphical = currentGraphicalDirection * -1f;
		} else {
			newDirections[0] = currentPaintingDirection;
			newDirections [1] = currentGraphicalDirection;

			return newDirections;
		}

		newDirections[0] = newPainting;
		newDirections [1] = newGraphical;


		return newDirections;

	}

	TileType GetRandomTileType(float roll){
		//print (roll);
		float against = 0;
		TileType chosenType = tiles[0];
		foreach (TileType type in tiles) {
			if (roll >= against) {
				chosenType = type;
			}
			against += type.chance;
		}
		return chosenType;
	}



	void Awake () {
		/*
		tiles = new TileType[5];

		TileType tile1 = new TileType (1, 1, 0.5f);
		TileType tile2 = new TileType (2, 2, 6f);
		TileType tile3 = new TileType (3, 3, 8.5f);
		TileType tile4 = new TileType (4, 5, 1f);
		TileType tile5 = new TileType (4, 10, 0.2f);

		tiles [0] = tile1;
		tiles [1] = tile2;
		tiles [2] = tile3;
		tiles [3] = tile4;
		tiles [4] = tile5;
*/
		totalChance = 0;

		foreach (TileType type in tiles) {
			totalChance += type.chance;
			//print (type.chance);

		}

		jungleBackground = new Color (26f/255f, 31f/255f, 24f/255f, 1f);
		desoBackground = new Color (39f/255f, 33f/255f, 45f/255f, 1f);

		openSquares = new List<MapSquare>();
		bottomWalls = new List<MapSquare> ();
		leftAndRightWalls = new List<MapSquare> ();

		angleHolder = new Angle[3, 3];
		angles = new Angle[4];
		angles [0] = new Angle (1, 0, 1, -1, null, null);
		angles [1] = new Angle (0, -1, -1, -1, angles[0], null);
		angles [2] = new Angle (-1, 0, -1, 1, angles[1], null);
		angles [3] = new Angle (0, 1, 1, 1, angles[2], angles[0]);
		angles [0].left = angles [3];
		angles [0].right = angles [1];
		angles [1].right = angles [2];
		angles [2].right = angles [3];
		angleHolder [2, 1] = angles [0];
		angleHolder [1, 0] = angles [1];
		angleHolder [0, 1] = angles[2];
		angleHolder [1, 2] = angles [3];



		//Transform newMudman = Instantiate (mudman, CoordsToWorldPosition (highestOpen.x, highestOpen.y), Quaternion.identity) as Transform;

		/*
		minimapTexture = Texture2D.whiteTexture;
		minimapTexture.Resize (mapGrid.GetLength (0), mapGrid.GetLength (1));

		for (int y = 0; y < mapSizeY; y++) {
			for (int x = 0; x < mapSizeX; x++) {
				allSquares.Add (mapGrid [x, y]);
			}
		}

		Color[] colors = new Color[mapSizeX * mapSizeY];

		int count = 0;
		foreach (MapSquare square in allSquares) {
			Color newColor = Color.black;
			if (square.filled == 0) {
				newColor = Color.white;
			}
			colors [count] = newColor;
			count++;
		}

		Rect newRect = new Rect (0, 0, mapSizeX, mapSizeY);
		minimapTexture.SetPixels (colors);
		minimap = Sprite.Create (minimapTexture, newRect, new Vector2 (0, 0));
		minimapSprite.sprite = minimap;
		*/
	}
		
	public void CreateJungleLevel(int totalTiles){
		totalForestTiles = totalTiles;

		mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;
		mapHolder.localScale = Vector3.one;
		
		GenerateForestMap ();
		GenerateJungleTiles ();
		FindOpenSpacePerfect ();

	}

	public void CreateDesoLevel(int totalTiles){
		totalForestTiles = totalTiles;

		mapHolder = new GameObject (holderName).transform;
		mapHolder.parent = transform;
		mapHolder.localScale = Vector3.one;

		GenerateForestMap ();
		GenerateDesoTiles ();
		FindOpenSpacePerfect ();

	}

	public void DestroyCurrentLevel(){
		if (transform.FindChild (holderName)) {
			DestroyImmediate (transform.FindChild (holderName).gameObject);
		}

		openSquares = new List<MapSquare>();
		bottomWalls = new List<MapSquare> ();
		leftAndRightWalls = new List<MapSquare> ();

	}

	public List<GameObject> PopulateMonsters(int number){
	
		MapSquare highestOpen = firstSquare;
		List<GameObject> monsterList = new List<GameObject> ();
		List<MapSquare> monsterPlacement = new List<MapSquare> ();

		foreach (MapSquare square in openSquares) {
			monsterPlacement.Add (square);
		}

		for (int i = 0; i < number; i++) {
			//find the most open square that doesn't already have a monster


			int highest = 0;

			foreach (MapSquare square in monsterPlacement) {
				if (square.distanceFromWall > highest) {
					if (square.hasMonster == 0) {
						highest = square.distanceFromWall;
						highestOpen = square;
					}
				}
			}
			for (int x = -2; x <= 2; x++) {
				for (int y = -2; y <= 2; y++) {
					mapGrid [highestOpen.x + x, highestOpen.y + y].hasMonster = 1;
				}
			}
			float roll = UnityEngine.Random.Range (0, 1f);
			if (roll > .5f) {
				Rockman newRockman = Instantiate (rockman, CoordsToWorldPosition (highestOpen.x, highestOpen.y), Quaternion.identity) as Rockman;
				monsterList.Add (newRockman.gameObject);
			} else {
				Mudman newMudman = Instantiate (mudman, CoordsToWorldPosition (highestOpen.x, highestOpen.y), Quaternion.identity) as Mudman;
				monsterList.Add (newMudman.gameObject);
			}
		}

		return monsterList;
	}

	[System.Serializable]
	public class TileType {
		public int xSize;
		public int ySize;
		[Range(0,10)]
		public float chance;

		public TileType(int _x, int _y, float _chance){
			xSize = _x;
			ySize = _y;
			chance = _chance;
		}
	}

	public class Map {

		int sizeX = 60;
		int sizeY = 40;
		int area;
		float squareSize = 2;

		int[,] intMap;
		MapSquare[,] mapGrid;

	}



	public class Angle {
		public int ident;
		public int xGraph;
		public int yGraph;
		public int xPaint;
		public int yPaint;
		public Angle left;
		public Angle right;
		public Vector2 graph;
		public Vector2 paint;

		public Angle(int _xGraph, int _yGraph, int _xPaint, int _yPaint, Angle _left, Angle _right){
			xGraph = _xGraph;
			yGraph = _yGraph;
			xPaint = _xPaint;
			yPaint = _yPaint;
			left = _left;
			right = _right;

			graph = new Vector2(xGraph, yGraph);
			paint = new Vector2(xPaint, yPaint);
		}
	}

	public class MapSquare : IHeapItem<MapSquare>{

		public int filled;
		public Vector2 position;
		public int x;
		public int y;
		public int gCost;
		public int hCost;
		public MapSquare cameFrom;
		public int neighboringWalls;
		public int distanceFromWall;
		public int hasMonster;

		int heapIndex;

		public int fCost{
			get { return gCost + hCost; }
		}


		public MapSquare(Vector2 _position, int _filled){
			position = _position;
			filled = _filled;
			distanceFromWall = -1;
			hasMonster = 0;

		}

		public MapSquare(Vector2 _position, int _filled, int _x, int _y){
			position = _position;
			filled = _filled;
			x = _x;
			y = _y;
			distanceFromWall = -1;
			hasMonster = 0;

		}

		public int HeapIndex{
			get{ 
				return heapIndex;
			}
			set{ 
				heapIndex = value;
			}
		}

		public int CompareTo(MapSquare squareToCompare){
			int compare = fCost.CompareTo (squareToCompare.fCost);
			if (compare == 0) {
				compare = hCost.CompareTo (squareToCompare.hCost);

			}
			return -compare;
		}

	}

	void OnDrawGizmos(){
		
		if (!Application.isPlaying)
			return;

		if (mapGrid != null) {
			for (int x = 0; x < mapGrid.GetLength (0); x++) {
				for (int y = 0; y < mapGrid.GetLength (1); y++) {

					Vector3 center = CoordsToWorldPosition (mapGrid [x, y].x, mapGrid [x, y].y);
					//new Vector3 (mapGrid [x, y].position.x, mapGrid [x, y].position.y, 1f);
					Vector3 size = new Vector3 (.9f * squareSize, .9f * squareSize, .9f * squareSize);

					float blueAmount = .1f * mapGrid [x, y].distanceFromWall;
					float redAmount = .5f - .1f * mapGrid [x, y].distanceFromWall;
					Color newColor = new Color (redAmount, .1f, blueAmount);

					Gizmos.color = Color.white;
					if (mapGrid [x, y].filled == 0) {
						Gizmos.color = newColor;
						Gizmos.DrawWireCube (center, size);
					}





				}


			}
		}


	}


}
