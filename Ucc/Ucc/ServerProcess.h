#pragma once
#include "Server.h"
#include "Message.h"

namespace uc
{
	class CServerProcess : public CServer
	{
		public:
            HANDLE  Pipe;
            CString PipeName;

			UOS_RTTI
			CServerProcess(CNexus * nexus, CServerInstance * si) : CServer(nexus, si)
			{

			}
			~CServerProcess()
			{

			}

			void SystemStart() override
			{
				auto exe = Instance->Release->Registry->Get<CString>(L"Executable");

				STARTUPINFO info = {sizeof(info)};
				PROCESS_INFORMATION processInfo;

				std::wostringstream s;

				CXonTextWriter w;
				
				if(Instance->Command)
				{
					w.Eol = false;
	
					for(auto i : Instance->Command->Nodes)
					{
						w.Write(s, i, 0);
					}
				}

				wchar_t cmd[32768] = {};
				wcscpy_s(cmd, _countof(cmd), (L"\"" + Nexus->MapPathToRelease(Instance->Release->Address, exe) + L"\" " + s.str()).data());

	// 			SetDllDirectory(FrameworkDirectory.data());
	// 			SetCurrentDirectory(FrameworkDirectory.data());

				if(!CreateProcess(null, cmd, null, null, true, 0, null, Nexus->MapPathToRelease(Instance->Release->Address, L"").data(), &info, &processInfo))
				{
					throw CException(HERE, L"Start process failed");
				}

				CloseHandle(processInfo.hProcess);
				CloseHandle(processInfo.hThread);

				PipeName = L"\\\\.\\Pipe\\UOS-A-" + CInt32::ToString(processInfo.dwProcessId);

//  				if(!WaitNamedPipe(PipeName.data(), 3000))
//  				{
// 					auto e = GetLastError();
//  					throw CException(HERE, L"Failed to open pipe");
//  				}

				do 
				{
					Pipe = CreateFile(PipeName.data(), GENERIC_READ|GENERIC_WRITE, 0, null, OPEN_EXISTING, 0, null);

					Sleep(1);
				}
				while(Pipe == INVALID_HANDLE_VALUE);

				DWORD dwMode = PIPE_READMODE_MESSAGE;

				if(!SetNamedPipeHandleState(Pipe, &dwMode, null, null))
				{
					throw CException(HERE, L"Failed to SetNamedPipeHandleState");
				}
            }

			void Send(CMessage & message)
			{
				CBuffer b;

				CMemoryStream s;
				CBinaryWriter w(&s);

				w.Write(message.GetCode());
				message.Write(&w);

				DWORD written;

                if(!WriteFile(Pipe, s.GetBuffer(), (DWORD)s.GetSize(), &written, null))
                {
                    throw CException(HERE, L"Failed to WriteFile");
                }
			}


			IProtocol * Accept(CString const & protocol) override
			{
				throw std::logic_error("The method or operation is not implemented.");
			}

			void Break(IProtocol * protocol) override
			{
				throw std::logic_error("The method or operation is not implemented.");
			}

	};
}