#pragma once

namespace uos
{
	class CSource
	{
		public:
			HINSTANCE			Instance;
			CString				DestFilePath; 
			ExpInterface	*	MaxExpInterface; 
			Interface		*	MaxInterface;
			BOOL				SuppressPromts;
			DWORD				Options;
	};
}