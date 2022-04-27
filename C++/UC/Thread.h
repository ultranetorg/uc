#pragma once
#include "IType.h"
#include "Event.h"
#include "Timer.h"
#include "String.h"

namespace uc
{
	class CCore;

	class UOS_LINKING CThread : public IType
	{
		public:
			std::function<void()>						Exited;
			std::function<void()>						Main;
			CTimer										Timer;

			CString										Name;
			CCore	*									Core;								
			HANDLE										Handle = null;

			UOS_RTTI
			CThread(CCore * e);
			~CThread();

			void										Start();
			void										Join();
			void										Terminate();
			bool										IsRunning();
			float										GetTime();

			unsigned int static __stdcall	 			Run(void * p);
	};
}