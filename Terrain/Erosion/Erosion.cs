using Godot;
using System;
using Godot.Collections;

public class Erosion : Node
{

    [Export]
    public int seed = 0;

    [Export]
    public int maxLifetime = 50;
    [Export]
    public float accelerationModifier = 5.0f;

    [Export]
    public float inertia = 0.05f;

    [Export]
    public float erosionModifier = 1.0f;
    [Export]
    public float evaporationSpeed = 0.05f;
    [Export]
    public float sedimentDropSpeed = 0.2f;



    private RandomNumberGenerator random;

    public HeightmapTerrain heightmap;

    public override void _Ready()
    {
        random = new RandomNumberGenerator();
        random.Seed = Convert.ToUInt64(seed);
    }

    public void SimulateErosion(HeightmapTerrain map, int amount) {
        heightmap = map;

        for(int i = 0; i < amount; i++) {
            SimulateDroplet();
        }
    }

    public void SimulateDroplet() {
        Droplet droplet = new Droplet(random, heightmap);

        for(int lifetime = 0; lifetime < maxLifetime; lifetime++) {
            // droplet destruction checks
            // if(droplet.waterAmount <= 0 && droplet.sediment <= 0) break;

            // calculate surface normal
            Vector3 surfaceNormal = CalculateSurfaceNormal(droplet.GetCoord());


            // alter direction based on the steapness of the surface normal
            droplet.moveDirection = droplet.moveDirection.LinearInterpolate(new Vector2(surfaceNormal.x, surfaceNormal.z), 1.0f-inertia).Normalized();


            // move position by the new direction
            float previousHeight = heightmap.terrainGrid[droplet.GetCoord()];
            droplet.position += droplet.moveDirection * droplet.speed;


            // destroy if droplet went outside map
            if(!heightmap.IsValid(droplet.GetCoord())) break;


            // increase/decrease speed based on deltaHeight
            Vector2 oldCoord = droplet.GetCoord();
            float currentHeight = heightmap.terrainGrid[droplet.GetCoord()];
            float deltaHeight = previousHeight - currentHeight;
            droplet.speed += deltaHeight*accelerationModifier;


            // evaporate water
            droplet.waterAmount *= (1-evaporationSpeed);


            // remove sediment from old position based on speed and amount of water
            float sedimentToErode = (droplet.waterAmount) * erosionModifier;

            // if(previousHeight - sedimentToRemove < currentHeight) sedimentToRemove = currentHeight - previousHeight;
            heightmap.terrainGrid[oldCoord] -= sedimentToErode;
            droplet.sediment += sedimentToErode;


            // deposit soil
            float sedimentToDrop = (droplet.sediment*sedimentDropSpeed);
            droplet.sediment -= sedimentToDrop;
            heightmap.terrainGrid[droplet.GetCoord()] += sedimentToDrop;

            // GD.Print("---");
            // GD.Print(surfaceNormal);
            // GD.Print(droplet.position);
            // GD.Print(droplet.moveDirection);
            // GD.Print(droplet.speed);
            // GD.Print(deltaHeight);
            // GD.Print(sedimentToRemove);
        }
    }

    private Vector3 CalculateSurfaceNormal(Vector2 coord) {
        float height1 = GetAverageHeight(coord , new Vector2(-1, -1));
        float height2 = GetAverageHeight(coord , new Vector2(1, -1));
        float height3 = GetAverageHeight(coord , new Vector2(1, 1));
        float height4 = GetAverageHeight(coord , new Vector2(-1, 1));

        Vector3 worldCoord1 = new Vector3(coord.x - 0.5f, height1, coord.y - 0.5f);
        Vector3 worldCoord2 = new Vector3(coord.x + 0.5f, height2, coord.y - 0.5f);
        Vector3 worldCoord3 = new Vector3(coord.x + 0.5f, height3, coord.y + 0.5f);
        Vector3 worldCoord4 = new Vector3(coord.x - 0.5f, height4, coord.y + 0.5f);

        return (CalculateTriangleNormal(worldCoord1,worldCoord2,worldCoord3) + CalculateTriangleNormal(worldCoord1,worldCoord3,worldCoord4)).Normalized();
    }

    private float GetAverageHeight(Vector2 coord, Vector2 scanDirection) {
        Vector2 coord1 = coord + (new Vector2(1,0) * scanDirection);
        Vector2 coord2 = coord + (new Vector2(1,1) * scanDirection);
        Vector2 coord3 = coord + (new Vector2(0,1) * scanDirection);

        float ownHeight = heightmap.terrainGrid[coord];
        float height1 = heightmap.IsValid(coord1) ? heightmap.terrainGrid[coord1] : ownHeight;
        float height2 = heightmap.IsValid(coord2) ? heightmap.terrainGrid[coord2] : ownHeight;
        float height3 = heightmap.IsValid(coord3) ? heightmap.terrainGrid[coord3] : ownHeight;

        return (ownHeight + height1 + height2 + height3)/4f;
    }

    private Vector3 CalculateTriangleNormal(Vector3 corner1, Vector3 corner2, Vector3 corner3) {
        Vector3 direction = (corner2-corner1).Cross(corner3-corner1);
        return -direction.Normalized();
    }

    private class Droplet {
        public Vector2 position = new Vector2(0,0);
        public Vector2 moveDirection = new Vector2(0,0);
        public float speed = 1f;
        public float waterAmount = 1.0f;
        public float sediment = 0.0f;

        public Droplet(RandomNumberGenerator random, HeightmapTerrain heightmap) {
            position.x = random.RandfRange(0,heightmap.mapSize-1);
            position.y = random.RandfRange(0,heightmap.mapSize-1);

            moveDirection.x = random.RandfRange(0,heightmap.mapSize-1);
            moveDirection.y = random.RandfRange(0,heightmap.mapSize-1);
            moveDirection = moveDirection.Normalized();
        }

        public Vector2 GetCoord() {
            return position.Round();
        }
    }
}
