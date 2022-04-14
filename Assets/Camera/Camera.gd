extends Spatial

var moveSpeed = 20

func _process(delta):
	if Input.is_mouse_button_pressed(2): 
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	else:
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		return
	
	var moveDirection = Vector3.ZERO
	
	if Input.is_action_pressed("forward"):
		moveDirection.z -= 1
	if Input.is_action_pressed("backward"):
		moveDirection.z += 1
	
	if Input.is_action_pressed("left"):
		moveDirection.x -= 1
	if Input.is_action_pressed("right"):
		moveDirection.x += 1
	
	if Input.is_action_pressed("space"):
		moveDirection.y += 1
	if Input.is_action_pressed("shift"):
		moveDirection.y -= 1
	
	translation += $Camera.global_transform.basis.z*moveDirection.z*moveSpeed*delta
	translation += $Camera.global_transform.basis.y*moveDirection.y*moveSpeed*delta
	translation += $Camera.global_transform.basis.x*moveDirection.x*moveSpeed*delta

func _input(event):
	if !Input.is_mouse_button_pressed(2):
		return
	else:
		$Control.grab_focus()
	
	if event is InputEventMouseMotion:
		var amount = event.relative
		$Camera.rotation.y -= amount.x*0.002
		$Camera.rotation.x -= amount.y*0.002
