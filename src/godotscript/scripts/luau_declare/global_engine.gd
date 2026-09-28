func load(luau_state: LuaState):

	luau_state.push_dictionary({
		"hi": "100.00"
	})
	luau_state.set_global("test")
	pass
