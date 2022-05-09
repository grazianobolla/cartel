tool
extends Viewport

#FIXME: not do this every frame
func _physics_process(_delta):
	size = $Label.rect_size