extends MeshInstance

export var tileSize = 1.0
export var intensity = 1.0

var surfaceTool = SurfaceTool.new()
var vertexArray

func constructMesh(terrainGrid):
	surfaceTool = SurfaceTool.new()
	surfaceTool.begin(Mesh.PRIMITIVE_TRIANGLE_FAN)
	vertexArray = []
	
	generateVertices(terrainGrid)

func generateVertices(terrainGrid):
	for coord1 in terrainGrid:
		var coord2 = coord1 + Vector2(0,1)
		var coord3 = coord1 + Vector2(1,1)
		var coord4 = coord1 + Vector2(1,0)
		
		if validCoord(coord2,terrainGrid) and validCoord(coord3,terrainGrid) and validCoord(coord4,terrainGrid):
			addVertex(vertexArray, terrainGrid, coord1)
			addVertex(vertexArray, terrainGrid, coord2)
			addVertex(vertexArray, terrainGrid, coord3)
			addVertex(vertexArray, terrainGrid, coord4)

func validCoord(coord,terrainGrid):
	return coord.x >= 0 and coord.y >= 0 and coord.x < terrainGrid.size() and coord.y < terrainGrid.size()

func addVertex(vertexArray, terrainGrid, coord):
	var vertex = Vector3(coord.x*tileSize, terrainGrid[coord]*intensity, coord.y*tileSize)
	surfaceTool.add_vertex(vertex)

func generateMesh():
	surfaceTool.generate_normals(true)
	var mesh = surfaceTool.commit()
