extends Control

export(NodePath) onready var game_logic = get_node(game_logic)
signal key_pressed(id, data)

func _on_ShakeButton_pressed():
	emit_signal("key_pressed", game_logic.currentPlayerId,  {"instruction": "shake", "action": ""})

func _on_OmitButton_pressed():
	emit_signal("key_pressed", game_logic.currentPlayerId,  {"instruction": "interact", "action":"omit"})

func _on_BuyButton_pressed():
	emit_signal("key_pressed", game_logic.currentPlayerId,  {"instruction": "interact", "action":"buy"})

func _on_BuyHouseButton_pressed():
	emit_signal("key_pressed", game_logic.currentPlayerId,  {"instruction": "interact", "action":"buy-house"})
