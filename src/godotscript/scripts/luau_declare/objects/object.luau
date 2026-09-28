--!nocheck

-- Why we should have LuauTS checking on file, where *.d.luau does not exists?

const private = {}
const class: object = {
	type = "object",

	new = function <T,U> (self: T, that: U?)
		const T = private.new(self)

		private.load_to_new(T, that)
		private.bootload_new(T, self) 
		private.wake(T)

		return T :: any
	end,

	is = function(this: object, that: string): boolean
		if that == "anyt" then
			return true
		end

		local base = this

		while base do
			const state, cached = private.get_cached_direct(base)
			if state then
				if cached.__type == that then
					return true
				end
			end

			base = private.ever[base]
		end

		return false
	end,

	it = function(this: object, that: string): boolean
		const state, cached = private.get_cached(this)
		return state and cached.__type == that or false
	end,

	eq = function(self: object, that: object) : boolean
		return self == that or self.it(that, self.type)
	end, 



	
	base = function <T> (self: T & object, expected: string|nil) : any
		const base = private.ever[self]

		if expected then 
			if self.is(base, expected) then
				return base
			end
		else
			return base
		end

		return nil
	end,

	host = function <T> (self: T & object, f: (any) -> (), last: boolean?)
		const state, cached = private.get_cached<<T,unknown>>(self)

		if state then
			const pos = table.find(cached.hosts, f)
			if pos then
				table.remove(cached.hosts, pos)
				return
			else
				table.insert(cached.hosts, f)
			end
		else
			return
		end
	end,

	as = function <T> (self: unknown, as: string) : T
		assert((self :: object):is(as), "cannot convert this object to required type")
		return self :: any
	end,

	that = function<T>(self: object, that: any) : T?
		return self.is(that, "object") and that or nil
	end,

	iden = function(self: object) : unknown
		return private.identity[self] or nil
	end,
}

do --private_region
	type cached <T = object, U = unknown> = {
		__index: U,
		__metatable: nil|string,
		__type: string,

		self: unknown,
		hosts: { (new: T) -> () },
	}
	
	private.frozen = table.freeze {__mode = "ks"}
	private.cached = setmetatable({} :: { [unknown]: cached }, private.frozen)
	private.ever = setmetatable({} :: { [object ]: object }, private.frozen)
	private.identity = setmetatable({} :: {[any]: any}, private.frozen)
	private.counter = 0



	function private.get_cached_by_instance<T,U>(this: object): (boolean, cached<T,U>)
		const ever = private.ever[this]
		if ever then
			const cached = private.cached[ever]
			if cached then
				return true, cached
			end
		end

		return false, nil :: never
	end

	function private.get_cached_direct <T,U> (this: unknown) : (boolean, cached<T,U>)
		const cached = private.cached[this]
		const state = cached ~= nil
		return state, cached
	end

	function private.get_cached<T,U> (this: object) : (boolean, cached<T,U>)
		local state, cached = private.get_cached_direct <<T,unknown>>(this)
		if not state then state, cached = private.get_cached_by_instance <<T, unknown>>(this) end
		return state, cached
	end

	function private.lazyget_cached(this: unknown | { [unknown]: unknown }, parent): cached
		const Ttype = typeof(this)
		const meta: cached = private.cached[this]
			or {
				__index = parent,
				__metatable = "<metatable locked>",
				__type = Ttype == "table" and tostring((this :: any).type) or Ttype,

				self = this,
				hosts = {},
			}

		private.cached[this] = meta
		return meta
	end

	function private.wake_hosts(inside: cached, to: object)
		for _, v in inside.hosts do
			pcall(v, to)
		end
	end

	function private.new (of: any) : object
		return table.clone(of)
	end

	function private.load_to_new (new: object, load: {[unknown]: unknown}|unknown|nil)
		const T = type(load)

		if T == "table" then
			for i, v in load :: {[unknown]: unknown} do
				(new :: {[unknown]: unknown})[i] = v
			end
		end
	end

	function private.get_hierarchy<T,U>(from: object): {cached<T,U>}
		const result = {}
		local state, cached = private.get_cached(from)

		while state do
			table.insert(result, cached)

			if cached.__index ~= nil then
				state, cached = private.get_cached(cached.__index :: any)

				if table.find(result, cached) then
					break
				end
			else
				break
			end
		end

		return result
	end
 
	function private.wake (new: object)
		const hierarchy =  private.get_hierarchy <<object, unknown>> (new)
		const count = #hierarchy

		for i = count, 1, -1 do
			const cached = hierarchy[i]
			private.wake_hosts(cached, new)
		end
	end

	function private.generator (new: object)
		private.counter += 1
		return private.counter
	end

	function private.bootload_new(new: object, parent: object?)
		const gen_iden = private.generator(new)	
		private.identity[new] = gen_iden
		private.ever[new] = parent
		private.lazyget_cached(new, parent)
	end

	private.bootload_new(class)
end

return class
