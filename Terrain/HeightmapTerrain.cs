using Godot;
using System;
using Godot.Collections;

public class HeightmapTerrain : Spatial
{
    private Dictionary<Vector2, float> terrainGrid;

    [Export]
    private int mapSize = 100;

    [Export]
    private int noiseSeed = 0;
    [Export]
    private float frequency = 1.0f;
    [Export]
    private Texture heightmap;

    private enum MAP_TYPE {
        NOISE,
        HEIGHT_MAP
    };

    [Export]
    private MAP_TYPE mapType = MAP_TYPE.NOISE;

    public override void _Ready()
    {
        CreateGrid();
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

            break;

            case MAP_TYPE.HEIGHT_MAP:

            break;
        }
    }
}
