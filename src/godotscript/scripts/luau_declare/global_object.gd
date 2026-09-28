func load(luau_state: LuaState):
	var bytecode = Luau.compile(FileAccess.get_file_as_string("res://src/godotscript/scripts/luau_declare/objects/object.luau"))
	luau_state.load_bytecode(bytecode,"object")
	luau_state.pcall(0, 1)
	luau_state.set_global("object")
	pass
