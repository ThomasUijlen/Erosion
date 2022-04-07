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
    public float gravity = 0.1f;

    [Export]
    public float inertia = 0.05f;
    [Export]
    public float soilCapacityModifier = 1.0f;
    [Export]
    public float erosionModifier = 1.0f;
    [Export]
    public float evaporationSpeed = 0.05f;



    private RandomNumberGenerator random;

    public HeightmapTerrain heightmap;

    public void SimulateErosion(HeightmapTerrain map) {
        heightmap = map;
        random = new RandomNumberGenerator();
        random.Seed = Convert.ToUInt64(seed);

        for(int i = 0; i < 100000; i++) {
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
            droplet.speed += deltaHeight*accelerationModifier + gravity;

            // evaporate water
            droplet.waterAmount *= (1-evaporationSpeed);

            // remove sediment from old position based on speed and amount of water
            float soilCapacity = (droplet.speed + droplet.waterAmount) * soilCapacityModifier;
            float sedimentToRemove = (droplet.speed + droplet.waterAmount) * erosionModifier * 0.1f;

            if(sedimentToRemove + droplet.sediment > soilCapacity) sedimentToRemove = soilCapacity - droplet.sediment;
            heightmap.terrainGrid[droplet.GetCoord()] -= sedimentToRemove;
            droplet.sediment += sedimentToRemove;


            // deposit soil if oversaturated
            if(droplet.sediment > soilCapacity) {
                float excessSoil = droplet.sediment - soilCapacity;
                droplet.sediment -= excessSoil;
                heightmap.terrainGrid[droplet.GetCoord()] += excessSoil;
            }

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
        Vector2 coord1 = coord;
        Vector2 coord2 = coord + new Vector2(1,0);
        Vector2 coord3 = coord + new Vector2(1,1);
        Vector2 coord4 = coord + new Vector2(0,1);
        
        float height1 = heightmap.terrainGrid[coord1];
        float height2 = height1;
        float height3 = height1;
        float height4 = height1;

        if(heightmap.IsValid(coord2)) height2 = heightmap.terrainGrid[coord2];
        if(heightmap.IsValid(coord3)) height3 = heightmap.terrainGrid[coord3];
        if(heightmap.IsValid(coord4)) height4 = heightmap.terrainGrid[coord4];

        Vector3 worldCoord1 = new Vector3(coord1.x, height1, coord1.y);
        Vector3 worldCoord2 = new Vector3(coord2.x, height2, coord2.y);
        Vector3 worldCoord3 = new Vector3(coord3.x, height3, coord3.y);
        Vector3 worldCoord4 = new Vector3(coord4.x, height4, coord4.y);

        return (CalculateTriangleNormal(worldCoord1,worldCoord2,worldCoord3) + CalculateTriangleNormal(worldCoord1,worldCoord3,worldCoord4)).Normalized();
    }

    private Vector3 CalculateTriangleNormal(Vector3 corner1, Vector3 corner2, Vector3 corner3) {
        Vector3 direction = (corner2-corner1).Cross(corner3-corner1);
        return -direction.Normalized();
    }

    private class Droplet {
        public Vector2 position = new Vector2(0,0);
        public Vector2 moveDirection = new Vector2(0,0);
        public float speed = 0.0f;
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
