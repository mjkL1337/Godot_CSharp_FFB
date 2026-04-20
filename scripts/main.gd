extends Node
@onready var constant: HSlider = $window/panel/constant
@onready var raw: HScrollBar = $window/panel/raw
@onready var constant_value: Label = $window/panel/constant/value
@onready var raw_value: Label = $window/panel/raw/value
@onready var force: CheckBox = $window/panel/hbox/force
@onready var proportional: CheckBox = $window/panel/hbox/proportional

const device_index: int = 0

func _ready() -> void:
	Input.joy_connection_changed.connect(_on_joy_connection_changed)

func _physics_process(_delta) -> void:
	if force.button_pressed:
		FFB.SetForce(constant.value)
	elif proportional.button_pressed:
		var p: float = constant.value - Input.get_joy_axis(device_index, JOY_AXIS_LEFT_X)
		FFB.SetForce(p * 8.0)
	raw.value = Input.get_joy_axis(device_index, JOY_AXIS_LEFT_X)
	constant_value.text = "%.3f" % constant.value
	raw_value.text = "%.3f" % raw.value

func _on_joy_connection_changed(_device_id: int, connected: bool) -> void:
	if connected:
		await get_tree().create_timer(0.5).timeout #to avoid race conditions
		FFB.InitializeDevices()


func _on_force_pressed() -> void:
	proportional.button_pressed = false


func _on_proportional_pressed() -> void:
	force.button_pressed = false
