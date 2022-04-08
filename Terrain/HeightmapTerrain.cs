using Godot;
using System;
using Godot.Collections;

public class HeightmapTerrain : Spatial
{
    public Dictionary<Vector2, float> terrainGrid;
    [Export]
    private int dropletsPerSecond = 1;

    [Export]
    public int mapSize = 100;

    [Export]
    private MAP_TYPE mapType = MAP_TYPE.NOISE;

    [Export]
    private int noiseSeed = 0;
    [Export]
    private float frequency = 1.0f;
    [Export]
    private Image heightmap;

    private enum MAP_TYPE {
        NOISE,
        HEIGHT_MAP
    };

    //Threading
    private Thread thread;
    private Semaphore semaphore = new Semaphore();


    public override void _Ready()
    {
        CreateGrid();
        GenerateMap();
        
        thread = new Thread();
        thread.Start(this, "ThreadFunction", "data", Thread.Priority.High);
        semaphore.Post();
    }

    int dropletsToRun = 0;
    ulong lastRunTime = 0;
    public void ThreadFinished() {
        ulong currentTime = OS.GetTicksMsec();
        float timeElapsed = currentTime - lastRunTime;
        lastRunTime = currentTime;

        dropletsToRun = Mathf.RoundToInt(dropletsPerSecond*timeElapsed/1000);
        semaphore.Post();
    }

    public void ThreadFunction(String data) {
        while(true) {
            semaphore.Wait();
            GD.Print("Thread start!");
            GetNode("ErosionSettings").Call("SimulateErosion",this, dropletsToRun);
            UpdateMesh();
            CallDeferred("ThreadFinished");
        }
    }

    private void CreateGrid() {
        terrainGrid = new Dictionary<Vector2, float>();

        for(int x = 0; x < mapSize; x++) {
            for(int y = 0; y < mapSize; y++) {
                terrainGrid.Add(new Vector2(x,y), 0f);
            }
        }
    }

    private void GenerateMap() {
        switch(mapType) {
            case MAP_TYPE.NOISE:
            ApplyNoise();
            break;

            case MAP_TYPE.HEIGHT_MAP:
            ApplyHeightmap();
            break;
        }
    }

    private void ApplyNoise() {
        OpenSimplexNoise noise = new OpenSimplexNoise();
        noise.Seed = noiseSeed;

        float lowestValue = -1.0f;

        for(int x = 0; x < mapSize; x++) {
            for(int y = 0; y < mapSize; y++) {
                float n = (noise.GetNoise2d(x*frequency,y*frequency) + 1.0f)/2.0f;
                terrainGrid[new Vector2(x,y)] = n;

                if(n < lowestValue || lowestValue == -1.0f) {
                    lowestValue = n;
                }

            }
        }

        //Lower everything so the lowest point is 0.0
        for(int x = 0; x < mapSize; x++) {
            for(int y = 0; y < mapSize; y++) {
                terrainGrid[new Vector2(x,y)] -= lowestValue;
            }
        }
    }

    private void ApplyHeightmap() {
        int mapWidth = heightmap.GetWidth();
        int mapHeight = heightmap.GetHeight();
        heightmap.Lock();

        for(int x = 0; x < mapSize; x++) {
            for(int y = 0; y < mapSize; y++) {
                Vector2 coord = new Vector2(x,y);
                terrainGrid[coord] = heightmap.GetPixel(Mathf.RoundToInt(x*mapWidth/mapSize), Mathf.RoundToInt(y*mapHeight/mapSize)).r;
            }
        }

        heightmap.Unlock();
    }

    public bool IsValid(Vector2 coord) {
        return coord.x >= 0f && coord.y >= 0f && coord.x < mapSize && coord.y < mapSize;
    }

    

    private void UpdateMesh() {
        Node mesh = GetNode("TerrainMesh");
        mesh.Call("constructMesh", terrainGrid, mapSize);
    }
}
