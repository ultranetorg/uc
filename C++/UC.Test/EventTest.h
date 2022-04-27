#pragma once
#include "../Aximion.Framework/FastDelegate.h"
#include "../Aximion.Framework/Delegate.h"
#include "../Aximion.Framework/UniversalDelegate.h"

using namespace mw;
using namespace fastdelegate;

struct EventSource
{
	Event<EventSource &>		Fired;
};

struct Subscriber
{
	CString		Name;
	EventSource & source;
	int Count;

	Subscriber() : source(EventSource())
	{
	}
		
	Subscriber(const CString & name, EventSource & s) : source(s)
	{
		Name = name;
		s.Fired += ThisHandler(Subscriber::Hander);
	}

	void Hander(EventSource & s)
	{
		wprintf(L"");
	}

};

struct AutoUnsubscriber
{
	CString		Name;
	EventSource & source;
	
	AutoUnsubscriber(const CString & name, EventSource & s) : source(s)
	{
		Name = name;
		s.Fired += ThisHandler(AutoUnsubscriber::Hander);
	}

	void Hander(EventSource & s)
	{
		assert(s.Fired.HasDelegate(ThisHandler(AutoUnsubscriber::Hander)));

		s.Fired -= ThisHandler(AutoUnsubscriber::Hander);
		
		assert(!s.Fired.HasDelegate(ThisHandler(AutoUnsubscriber::Hander)));
	}
};

Subscriber ss;
Subscriber cc;


void TestEventWithFunctor()
{
	EventSource s1;
	EventSource s2;


	AutoUnsubscriber a(L"a", s1);
	Subscriber		 b(L"b", s1);
	AutoUnsubscriber c(L"c", s1);

	s1.Fired(s1);

	auto r = rand();

	CTimer t;
	t.Start();


	auto q = new Subscriber();
	auto w = new Subscriber();

	auto es1 = new EventSource();
	auto es2 = new EventSource();

	for(int i=0; i<1000000; i++)
	{
		auto v = i % 1000 == 0 ? q : w;
		FastDelegate<void(EventSource &)> a(v, &Subscriber::Hander);
		FastDelegate<void(EventSource &)> b(v, &Subscriber::Hander);
		
		if(r % 100000 == 0)
			a(*es1);
		if(r % 100001 == 0)
			b(*es2);
	}
	t.Pause();
	wprintf(L"FastDelegate ctor:   %f\r\n", t.GetTime());


	t.Start();
	for(int i=0; i<1000000; i++)
	{
		auto v = i % 1000 == 0 ? q : w;
		UniversalDelegate<void(EventSource &)> a(v, &Subscriber::Hander);
		UniversalDelegate<void(EventSource &)> b(v, &Subscriber::Hander);
		
		if(r % 100000 == 0)
			a(*es1);
		if(r % 100001 == 0)
			b(*es2);
	}
	t.Pause();
	wprintf(L"UniversalDelegate:   %f\r\n", t.GetTime());

	t.Start();
	for(int i=0; i<1000000; i++)
	{
		auto v = i % 1000 == 0 ? q : w;
		Delegate<void(EventSource &)> a(v, &Subscriber::Hander);
		Delegate<void(EventSource &)> b(v, &Subscriber::Hander);
		
		if(r % 100000 == 0)
			a(*es1);
		if(r % 100001 == 0)
			b(*es2);
	}
	t.Pause();
	wprintf(L"C++14 Delegate call: %f\r\n", t.GetTime());



	auto & fd = FastDelegate<void(EventSource &)>(&ss, &Subscriber::Hander);
	t.Start();
	for(int i=0; i<1000000; i++)
	{
		fd(s1);
	}
	t.Pause();
	wprintf(L"FastDelegate call:   %f\r\n", t.GetTime());

	auto & ud = UniversalDelegate<void(EventSource &)>(&ss, &Subscriber::Hander);
	t.Start();
	for(int i=0; i<1000000; i++)
	{
		ud(s1);
	}
	t.Pause();
	wprintf(L"UniversalDelegate:   %f\r\n", t.GetTime());

	auto & dd = Delegate<void(EventSource &)>(&ss, &Subscriber::Hander);
	t.Start();
	for(int i=0; i<1000000; i++)
	{
		dd(s1);
	}
	t.Pause();
	wprintf(L"C++14 Delegate call: %f\r\n", t.GetTime());

	int x = 0;
	ud = [s1, &x](EventSource &) mutable
	{
		x = 1;
	};
	ud(s1);
	assert(x == 1);

	wprintf(L"Event OK\r\n");
}


void TestEventWithLambda()
{
	EventSource s1;
	EventSource s2;

	auto r = rand();

	CTimer t;
	t.Start();

	int x = 6; 
	int y = 7;

	auto q = new Subscriber();
	auto w = new Subscriber();

	auto es1 = new EventSource();
	auto es2 = new EventSource();

	for(int i=0; i<1000000; i++)
	{
		auto v = i % 1000 == 0 ? q : w;
		std::function<void(EventSource &)> a = [&x, &y](auto &){ auto c = x + y; };
		std::function<void(EventSource &)> b = [&x, &y](auto &){ auto c = x + y; };
		
		if(r % 100000 == 0)
			a(*es1);
		if(r % 100001 == 0)
			b(*es2);
	}
	t.Pause();
	wprintf(L"std::function ctor:   %f\r\n", t.GetTime());


	t.Start();
	for(int i=0; i<1000000; i++)
	{
		auto v = i % 1000 == 0 ? q : w;
		Delegate<void(EventSource &)> a =[&x, &y](auto &){ auto c = x + y; };
		Delegate<void(EventSource &)> b =[&x, &y](auto &){ auto c = x + y; };
		
		if(r % 100000 == 0)
			a(*es1);
		if(r % 100001 == 0)
			b(*es2);
	}
	t.Pause();
	wprintf(L"Delegate ctor:        %f\r\n", t.GetTime());

	{
		std::function<void(EventSource &)> fd = [&x, &y](auto &){ auto c = x + y; };
		t.Start();
		for(int i=0; i<1000000; i++)
		{
			fd(s1);
		}
		t.Pause();
		wprintf(L"std::function call:   %f\r\n", t.GetTime());
	}

	{
		Delegate<void(EventSource &)> fd = [&x, &y](auto &){ auto c = x + y; };
		t.Start();
		for(int i=0; i<1000000; i++)
		{
			fd(s1);
		}
		t.Pause();
		wprintf(L"Delegate call:        %f\r\n", t.GetTime());
	}

}

