#pragma once
#include "CpuMonitor.h"

namespace uc
{
	struct CLevel1Timings
	{
		//float			TickTime;
		//int				TickP1SCounter;
		//int				Ticks;
		int				TotalTicks;
		int				EventP1SCounter;
		int				MessageP1SCounter;

		CCpuMonitor		CpuMonitor;
	};
}