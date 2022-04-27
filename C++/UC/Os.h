#pragma once
#include "IOs.h"
#include "IType.h"
#include "Uxx.h"

namespace uc
{
	class UOS_LINKING COs : public IOs, public IType
	{
		public:
			virtual	void								SaveDCToFile(LPCTSTR FileName, HDC hsourcedc, int Width, int Height);

			virtual CVersion							GetVersion();
			virtual bool								Is64();
	
			CList<CString>								OpenFileDialog(DWORD options, CArray<std::pair<CString,CString>> & types = CArray<std::pair<CString,CString>>());

			CString										ComputerName();
			CString										GetUserName();

			void										RegisterUrlProtocol(const CString & pwProtocolName, const CString & wd, const CString & pwAppPath);

			BOOL										SetPrivilege(HANDLE hToken, /* access token handle */ LPCTSTR lpszPrivilege, /* name of privilege to enable/disable */ BOOL bEnablePrivilege /* to enable or disable privilege */);
			
			static CString								GetEnvironmentValue(CString const & name);

			static CList<CUrq>						ParseCommandLine(CString const & cmd);

			UOS_RTTI
			COs();
			~COs();

		protected:
			CString										WindowClass;
			CVersion		 							Version;
	};
}
