declare extern type Game with
	is_server: boolean
end

export type 

export type Subscription {
	
}

export type Event <T... = ...unknown> {
	read subscribe: (self: Event<T>, ...(T...) -> ()) -> ...Subscription
}

declare extern type Engine with
	
end
