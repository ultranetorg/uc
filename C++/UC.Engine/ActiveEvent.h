#pragma once

namespace uc
{
	enum class EListen : int
	{
		Null				= 0,
		Primary				= 0b0001,
		PrimaryRecursive	= 0b0010,
		PrimaryAll			= 0b0011,
		Normal				= 0b0100,
		NormalRecursive		= 0b1000,
		NormalAll			= 0b1100
	};

	EnumBitwiseOperators(EListen);

	template<class ...A> struct CActiveDelegate
	{
		EListen				Mask;
		CEvent<A...>		Event;
	};

	template<class ...A> class CActiveEvent
	{
		public:
			auto & operator [] (EListen l)
			{
				for(auto & i : Events)
				{
					if(i.Mask == l)
					{
						return i.Event;
					}
				}

				Events.push_back(CActiveDelegate<A...>{l});
				return Events.back().Event;
			}

			inline void operator () (EListen mask, A... args)
			{
				if(!Events.empty())
				{
					auto f = Events;
	
					for(auto & i : f)
					{
						if((i.Mask & mask) != EListen::Null)
							i.Event(std::forward<A>(args)...);
					}
				}
			}

			template<class P> inline void operator () (P p, A ... args)
			{
				if(!Events.empty())
				{
					auto f = Events;
			
					for(auto & i : f)
					{
						if(p(i.Mask))
							i.Event(std::forward<A>(args)...);
					}
				}
			}

			void operator -= (CDelegate<void(A ...)> & d)
			{
				Events.Find([&d](auto & i){ return i.Event.HasDelegate(d); }).Event -= d;
			}

			bool Contains(EListen l)
			{
				return Events.Has([l](auto & i){ return (i.Mask & l) != EListen::Null; });
			}

		protected:
			CArray<CActiveDelegate<A ...>>	Events;
	};

/* !!!!!!!!!!!!!!!! IMPORTANT !!!!!!!!!!!!!!!!!!

namespace fn {

	template<typename Sign, typename R, typename... Args>
	struct traits_base {
		using signature = Sign;
		using cpp11_signature = R(Args...);
		using is_method = std::is_member_function_pointer<signature>;
		using args = std::tuple<Args...>;
		using arity = std::integral_constant<unsigned, sizeof...(Args)>;
		using result = R;
	};

	template<typename T>
	struct traits : traits<decltype(&T::operator())> {};
	template<typename R, typename... Args>
	struct traits<R(*)(Args...)> : traits_base<R(*)(Args...), R, Args...> {};
	template<typename R, typename... Args>
	struct traits<R(* const)(Args...)> : traits_base<R(* const)(Args...), R, Args...> {};
	#if __cplusplus >= 201703
	template<typename R, typename... Args>
	struct traits<R(*)(Args...) noexcept> : traits_base<R(*)(Args...), R, Args...> {};
	template<typename R, typename... Args>
	struct traits<R(* const)(Args...) noexcept> : traits_base<R(* const)(Args...), R, Args...> {};
	#endif // __cplusplus >= 201703
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: *)(Args...)> : traits_base<R(C:: *)(Args...), R, Args...> {};
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: * const)(Args...)> : traits_base<R(C:: * const)(Args...), R, Args...> {};
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: *)(Args...) const> : traits_base<R(C:: *)(Args...) const, R, Args...> {};
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: * const)(Args...) const> : traits_base<R(C:: * const)(Args...) const, R, Args...> {};
	#if __cplusplus >= 201703
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: *)(Args...) noexcept> : traits_base<R(C:: *)(Args...), R, Args...> {};
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: * const)(Args...) noexcept> : traits_base<R(C:: * const)(Args...), R, Args...> {};
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: *)(Args...) const noexcept> : traits_base<R(C:: *)(Args...) const, R, Args...> {};
	template<typename R, typename C, typename... Args>
	struct traits<R(C:: * const)(Args...) const noexcept> : traits_base<R(C:: * const)(Args...) const, R, Args...> {};
	#endif // __cplusplus >= 201703

}

	enum class EListen : int
	{
		Null				= 0,
		Primary				= 0b0001,
		PrimaryRecursive	= 0b0010,
		PrimaryAll			= 0b0011,
		Normal				= 0b0100,
		NormalRecursive		= 0b1000,
		NormalAll			= 0b1100
	};


	template<typename S> struct CActiveDelegate
	{
		fastdelegate::FastDelegate<S>	Function;
		std::function<S>				Lambda;
		EListen							Listen;

		CActiveDelegate()
		{
		}
	};

	template<class X, class Y, typename...A> auto CreateActiveDelegate(Y* x, void (X::*func)(A...), EListen l)
	{ 
		CActiveDelegate<void(A...)> d;
		d.Function = fastdelegate::FastDelegate<void(A...)>(x, func);
		d.Listen = l;
		return d;
	}


	template<class L> auto LambdaSubscriber(L la, EListen l)
	{ 
		CActiveDelegate<typename fn::traits<L>::cpp11_signature> d;
		d.Lambda = la;
		d.Listen = l;
		return d;
	}

	#define FunctionSubscriber(method, l)	CreateActiveDelegate(this, &std::remove_pointer<decltype(this)>::type::method, l)


	template<class ...A> class CActiveEvent
	{
		public:
			void operator += (CActiveDelegate<void(A...)> & d)
			{
				Subscribers.push_back(d);
			}

			inline void operator () (EListen mask, A... args)
			{
				if(!Subscribers.empty())
				{
					auto f = Subscribers;
	
					for(auto & i : f)
					{
						if(int(i.Listen) & int(mask))
						{
							if(i.Function)
								i.Function(std::forward<A>(args)...);
							if(i.Lambda)
								i.Lambda(std::forward<A>(args)...);
						}
					}
				}
			}

			void operator -= (CActiveDelegate<void(A...)> & d)
			{
				for(auto & i : Subscribers)
				{
					if(i.Delegate == d)
					{
						Subscribers.Remove(d);
						return;
					}
				}
			}


		protected:
			std::list<CActiveDelegate<void(A...)>>	Subscribers;
	};


	class A
	{
		public:
			CActiveEvent<std::string &> aaa;

		A()
		{
			aaa += FunctionSubscriber(AAA, EListen::Normal );
			aaa += LambdaSubscriber([](std::string &)
									{
										std::cout << "2";
									}, EListen::Primary);
		}

		void AAA(std::string &)
		{
			std::cout << "1";
		}
	};

int _tmain(int argc, _TCHAR* argv[])
{
	A a;

	a.aaa(EListen(EListen::PrimaryRecursive), std::string());

	return 0;
}



*/
}