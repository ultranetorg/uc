// Mightywill.Framework.Test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#ifdef _DEBUG
	#define _CRTDBG_MAP_ALLOC
	#include <stdlib.h>
	#include <crtdbg.h>
#endif

#include <iostream>
#include "EventTest.h"

using namespace mw;

int main()
{
	TestEventWithLambda();

	std::cin.get();

    return 0;
}

