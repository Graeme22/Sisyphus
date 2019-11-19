﻿/*
	ChunkManager
	
	Author: Graeme Holliday
	Purpose: Provide the utility to create, initialize, and load chunks into the game world with any requisite information.
*/

using UnityEngine;

public class ChunkManager : MonoBehaviour {

	public GameObject lava, lavaTop, water, waterTop;

	private GameObject player;

	private BiomeBase currentBiome;
	private BiomeBase grass, snow, desert, hell;

	public float seed;
	private int position;
	private int furthestRendered;

	private float fps;
	private const float minFps = 20f;

	/*
		Start
	
		Purpose: Initialize the class members.
	*/
	void Start() {
		grass = new BiomeGrass();
		snow = new BiomeSnow();
		desert = new BiomeDesert();
		hell = new BiomeHell();
		player = GameObject.FindGameObjectWithTag("Player");
		seed = Random.Range(0f, 1f);
		position = 0;
		furthestRendered = -1;
	}

	/*
		Update

		Purpose: Each tick, update the biome if neccesary, and load a new chunk if neccessary. Additionally, do the stress test.
	*/
	void Update() {
		position = (int)Mathf.Round(player.transform.position.x / Chunk.CHUNK_SIZE);
		if(position % 10 == 0 && position > furthestRendered) {
			//Update biome based on noise function.
			float result = Mathf.PerlinNoise((float)position, this.seed) % 0.01f;
			if(result < 0.0025f)
				currentBiome = grass;
			else if(result < 0.005f)
				currentBiome = desert;
			else if(result < 0.0075f)
				currentBiome = hell;
			else
				currentBiome = snow;
		}

		//Render the chunk we're at if it hasn't been already.
		if(position > furthestRendered)
			RenderChunk(GenerateChunk(position, 0.05f), position);
		furthestRendered = position;

		fps += 1f / Time.deltaTime;
		fps /= 2f;
		if(fps < minFps)
			Debug.Log("Stress test failed.");
		else
			Debug.Log("Stress test passed.");
	}

	/*
		GenerateChunk

		Parameters: Initial chance for a cell to be occupied, number of times to smooth, number of neighbors required for a birth or death.
		Returns: Newly created chunk containing the cells set as generated by cellular automata.
		Purpose: Create, initialize, and return the chunk so it can be rendered and populated later on.
	*/
	public Chunk GenerateChunk(int position, float frequency) {
		int[,] cells = new int[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];

		InitializeCells(cells, position, frequency);

		return new Chunk(cells, currentBiome);
	}

	/*
		InitializeCells

		Purpose: Sets all cells in map to an initial value of either 0, 1, or 2 based on Perlin noise. 0 = air, 1 = top, 2 = fill.
		Parameters: The 2D array of cells to initialize.
	*/
	private void InitializeCells(int[,] grid, int position, float frequency) {
		for(int x = 0; x < Chunk.CHUNK_SIZE; x++) {
			int height = (int)Mathf.Clamp(Mathf.PerlinNoise(frequency * (position * Chunk.CHUNK_SIZE + x), seed) * (float)Chunk.CHUNK_SIZE, 1f, (float)(Chunk.CHUNK_SIZE - 1));
			int y;
			for(y = 0; y < height; y++)
				grid[x, y] = 0;
			grid[x, y++] = 1;
			for(; y < Chunk.CHUNK_SIZE; y++)
				grid[x, y] = 2;
		}
	}

	/*
		RenderChunk

		Parameters: Chunk to render, chunk position to render at.
		Purpose: Render the given chunk where it should be. Translates the chunk's grid into actual GameObject instances.
	*/
	public void RenderChunk(Chunk c, int position) {
		GameObject top = c.getBiome().top;
		GameObject fill = c.getBiome().fill;
		GameObject decoration = c.getBiome().decoration;
		for(int y = 0; y < Chunk.CHUNK_SIZE; y++) {
			for(int x = position * Chunk.CHUNK_SIZE; x < position * Chunk.CHUNK_SIZE + Chunk.CHUNK_SIZE; x++) {
				GameObject tile;
				switch(c.grid[x % Chunk.CHUNK_SIZE, y]) {
					case 1:
						tile = (GameObject)Instantiate(top, this.transform);
						break;
					case 2:
						tile = (GameObject)Instantiate(fill, this.transform);
						break;
					case 3:
						tile = (GameObject)Instantiate(decoration, this.transform);
						break;
					case 4:
						if(c.getBiome().liquid == BiomeBase.Liquid.LAVA)
							tile = (GameObject)Instantiate(lavaTop, this.transform);
						else
							tile = (GameObject)Instantiate(waterTop, this.transform);
						break;
					case 5:
						if(c.getBiome().liquid == BiomeBase.Liquid.LAVA)
							tile = (GameObject)Instantiate(lava, this.transform);
						else
							tile = (GameObject)Instantiate(water, this.transform);
						break;
					default:
						continue;
				}
				tile.transform.position = new Vector2(x, -y);
			}
		}
	}

	/*
		getSeed

		Returns: The seed we're using this playthrough.
		Purpose: Get the read-only seed value.
	*/
	public float getSeed() {
		return seed;
	}

}
