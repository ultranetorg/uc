#pragma once

namespace uc
{
	class IPerformanceCounter
	{
		public:
			virtual void								BeginMeasure()=0;
			virtual void								EndMeasure()=0;

			virtual ~IPerformanceCounter(){}
	};
}