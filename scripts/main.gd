extends Node
@onready var constant: HSlider = $window/panel/constant
@onready var raw: HScrollBar = $window/panel/raw
@onready var constant_value: Label = $window/panel/constant/value
@onready var raw_value: Label = $window/panel/raw/value

func _ready() -> void:
	Input.joy_connection_changed.connect(_on_joy_connection_changed)

func _physics_process(_delta) -> void:
	FFB.SetForce(constant.value)
	raw.value = Input.get_joy_axis(0, JOY_AXIS_LEFT_X)
	constant_value.text = "%.3f" % constant.value
	raw_value.text = "%.3f" % raw.value

func _on_joy_connection_changed(_device_id: int, connected: bool) -> void:
	if connected:
		await get_tree().create_timer(0.5).timeout #to avoid race conditions
		FFB.InitializeDevices()
