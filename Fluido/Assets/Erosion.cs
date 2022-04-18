using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiSolver))]
public class Erosion : MonoBehaviour
{
	int hmWidth; // heightmap width
	int hmHeight; // heightmap height

	int posXInTerrain; // position of the game object in terrain width (x axis)
	int posYInTerrain; // position of the game object in terrain height (z axis)

	public int size = 1; // the diameter of terrain portion that will raise under the game object
	public float desiredHeight = 0; // the height we want that portion of terrain to be

	private ObiSolver solver;

	Terrain terr;

    private void Start()
    {
		GameObject clone = GameObject.Find("Terrain");
		terr = clone.GetComponentInChildren<Terrain>();
		hmWidth = terr.terrainData.heightmapResolution;
		hmHeight = terr.terrainData.heightmapResolution;
	}

    void Awake()
	{
		solver = GetComponent<Obi.ObiSolver>();
	}

	void OnEnable()
	{
		solver.OnCollision += Solver_OnCollision;
	}

	void OnDisable()
	{
		solver.OnCollision -= Solver_OnCollision;
	}

	void Solver_OnCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
	{
		var colliderWorld = ObiColliderWorld.GetInstance();

		for (int i = 0; i < e.contacts.Count; ++i)
		{
			if (e.contacts.Data[i].bodyA != e.contacts.Data[i].bodyB)
			{
				var col = colliderWorld.colliderHandles[e.contacts.Data[i].bodyB].owner;
				if (col.tag == "Terrain")
				{
					var punto = e.contacts.Data[i].pointB;


					//               int res = terrain.terrainData.heightmapResolution;
					//               float[,] heights = terrain.terrainData.GetHeights(0, 0, res, res);
					//for (int k = 0; k < res; k++)
					//{
					//	for (int j = 0; j < res; j++)
					//	{
					//		if (k == (int)punto.x / res && j == (int)punto.y / res)
					//		{
					//			heights[k, j] -= erosion;
					//		}
					//		//print(heights[i,j]);
					//	}
					//}
					//               // set the new height
					//               terrain.terrainData.SetHeights(0, 0, heights);

					// get the normalized position of this game object relative to the terrain
					Vector3 tempCoord = (Vector3) punto;
					Vector3 coord;
					coord.x = tempCoord.x / terr.terrainData.size.x;
					coord.y = tempCoord.y / terr.terrainData.size.y;
					coord.z = tempCoord.z / terr.terrainData.size.z;

					// get the position of the terrain heightmap where this game object is
					posXInTerrain = (int)(coord.x * hmWidth);
					posYInTerrain = (int)(coord.z * hmHeight);

					// we set an offset so that all the raising terrain is under this game object
					int offset = size / 2;

					// get the heights of the terrain under this game object
					float[,] heights = terr.terrainData.GetHeights(posXInTerrain - offset, posYInTerrain - offset, size, size);

					// we set each sample of the terrain in the size to the desired height
					for (int k = 0; k < size; k++)
						for (int j = 0; j < size; j++)
							heights[k, j] = desiredHeight;

					// go raising the terrain slowly
					desiredHeight += Time.deltaTime;

					// set the new height
					terr.terrainData.SetHeights(posXInTerrain - offset, posYInTerrain - offset, heights);
				}
			}
		}

	}

}