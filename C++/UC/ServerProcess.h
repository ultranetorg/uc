#pragma once
#include "Server.h"

namespace uc
{
	class CServerProcess : public CServer
	{
		public:
			UOS_RTTI
			CServerProcess(CNexus * nexus, CServerInstance * si) : CServer(nexus, si)
			{

			}
			~CServerProcess()
			{

			}

			void Start() override
			{
	        	auto exe = Instance->Release->Registry->Get<CString>(L"Server");

 				STARTUPINFO info = {sizeof(info)};
 				PROCESS_INFORMATION processInfo;
 	
 				std::wostringstream s;
 				CXonTextWriter w;
 				w.Eol = false;

                for(auto i : Instance->Command->Nodes)
                {
                    w.Write(s, i, 0);
                }
 					
 				wchar_t cmd[32768] = {};
 				wcscpy_s(cmd, _countof(cmd), (L"\"" + Nexus->MapPathToRelease(Instance->Release->Address, exe) + L"\" " + s.str()).data());
 	
 	// 			SetDllDirectory(FrameworkDirectory.data());
 	// 			SetCurrentDirectory(FrameworkDirectory.data());
 	
 				if(CreateProcess(null, cmd, null, null, true, 0, null, Nexus->MapPathToRelease(Instance->Release->Address, L"").data(), &info, &processInfo))
 				{
 					//auto e = GetLastError();
 					CloseHandle(processInfo.hProcess);
 					//si->Process = processInfo.hProcess;
 					CloseHandle(processInfo.hThread);
 				}
			}
    };
}