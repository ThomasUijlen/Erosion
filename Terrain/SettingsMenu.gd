extends Control

onready var terrain = get_parent()
onready var erosion = get_parent().get_node("ErosionSettings")

var simulationActive = true

func _ready():
	startStop()

func worldSeed(new_text):
	terrain.noiseSeed = int(new_text)



func resetWorld():
	terrain.call("StopThread")




func DropsPerIteration(new_text):
	dropSpeed = int(new_text)
	if simulationActive:
		terrain.dropletsPerSecond = dropSpeed




func intertia(new_text):
	erosion.inertia = int(new_text)


func erosionSpeed(new_text):
	erosion.erosionModifier = int(new_text)




func sedimentDropSpeed(new_text):
	erosion.sedimentDropSpeed = int(new_text)




func evaporationSpeed(new_text):
	erosion.evaporationSpeed = int(new_text)

onready var dropSpeed = terrain.dropletsPerSecond
func startStop():
	simulationActive = !simulationActive
	
	if simulationActive:
		terrain.dropletsPerSecond = dropSpeed
		$StartStop/Button.text = "Stop Simulation"
	else:
		terrain.dropletsPerSecond = 0
		$StartStop/Button.text = "Start Simulation"
