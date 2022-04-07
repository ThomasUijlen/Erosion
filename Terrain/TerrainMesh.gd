extends MeshInstance

export var tileSize = 1.0
export var intensity = 1.0

var surfaceTool = SurfaceTool.new()

func constructMesh(terrainGrid, mapSize):
	surfaceTool = SurfaceTool.new()
	surfaceTool.begin(Mesh.PRIMITIVE_TRIANGLES)
	surfaceTool.add_smooth_group(true)
	
	generateVertices(terrainGrid, mapSize)
	generateMesh()

func generateVertices(terrainGrid, mapSize):
	for coord1 in terrainGrid:
		var coord2 = coord1 + Vector2(1,0)
		var coord3 = coord1 + Vector2(1,1)
		var coord4 = coord1 + Vector2(0,1)
		
#		print("---")
#		print(coord1)
#		print(coord2)
#		print(coord3)
#		print(coord4)
		
		if validCoord(coord2,terrainGrid, mapSize) and validCoord(coord3,terrainGrid, mapSize) and validCoord(coord4,terrainGrid, mapSize):
			addVertex(terrainGrid, coord1)
			addVertex(terrainGrid, coord2)
			addVertex(terrainGrid, coord3)
			
			addVertex(terrainGrid, coord1)
			addVertex(terrainGrid, coord3)
			addVertex(terrainGrid, coord4)

func validCoord(coord,terrainGrid, mapSize):
	return coord.x >= 0 and coord.y >= 0 and coord.x < mapSize and coord.y < mapSize

func addVertex(terrainGrid, coord):
	var vertex = Vector3(coord.x*tileSize, terrainGrid[coord]*intensity, coord.y*tileSize)
	surfaceTool.add_vertex(vertex)

func generateMesh():
	surfaceTool.index()
	surfaceTool.generate_normals(false)
	mesh = surfaceTool.commit()
