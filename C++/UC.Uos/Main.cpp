#include "stdafx.h"

using namespace uc;

/// /Core/Database?Action=Rollback
/// /UC.World?Mode=Mobile.Emulation
/// /UC.World?Mode=VR.Emulation
/// Experimental{create class=Commander} address=uc/experimental/Experimental

void Create(HINSTANCE instance, LPTSTR cmd)
{
	wchar_t p[4096];
	GetModuleFileNameW(instance, p, _countof(p));

	//auto dll = CPath::ReplaceFileName(p, L"UC.dll");

	CProductInfo pi;
	pi.Namespace		= UC_NAMESPACE;
	pi.CompanyName		= UC_NAME;
	pi.Copyright		= UC_COPYRIGHT;
	pi.Version			= CVersion::GetFrameworkVerision();
	pi.HumanName		= UOS_PRODUCT_NAME_PUBLIC;
	pi.Name				= UOS_PRODUCT_NAME_INTERNAL;
	pi.Description		= UOS_PRODUCT_DESCRIPTION;
	pi.WebPageHome		= WEB_PAGE_HOMEPAGE;
	pi.WebPageSupport	= WEB_PAGE_CONTACT;
	pi.Stage			= PROJECT_DEVELOPMENT_STAGE;
	pi.Build			= PROJECT_CONFIGURATION;
	pi.Platform			= PROJECT_TARGET_PLATFORM;

	auto main = [&]
				{
					CSupervisor s;
					auto core = new CCore(&s, instance, cmd, SUPERVISOR_FOLDER, CORE_FOLDER, pi);

					if(core->Initialized)
					{
						auto nexus = new CNexus(core, core->Config);
						core->Run();
						delete nexus;
					}

					delete core;
				};

	if(IsDebuggerPresent())
	{
		main();
	} 
	else
	{
		try
		{
			main();
		}
		catch(CException & e)
		{
			MessageBox(null, (e.Source.ClassMethod + L":" + CInt32::ToString(e.Source.Line) + L"\n\n" + e.Message).data(), pi.ToString(L"NVSPB").data(), MB_OK|MB_ICONERROR);
			abort();
		}
	}
}

int wWinMain(HINSTANCE instance, HINSTANCE prev, LPTSTR cmd, int nCmdShow)
{
//	auto & d = CTonDocument(CXonTextReader(L"{a b c} {e f}"));

	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF|_CRTDBG_LEAK_CHECK_DF);
	//_CrtSetBreakAlloc(6086);

	CString c;

	auto x = new CTonDocument(CXonTextReader(cmd));

	if(x->Any(L"Core/HardcodedCommandLine"))
	{
		c += COs::GetEnvironmentValue(L"G_Mmc");
		c += L" ";
		c += L"World{" 
					L"Name=Desktop "
					//L"Name=Mobile.E"
					//L"Name=VR.E"
						//L"Layout=" + COs::GetEnvironmentValue(L"G_Layout") +
						//L"&Layout=" + COs::GetEnvironmentValue(L"UO_DuoLayout") +
						//L"&Layout=PhoneB"
						//L"&Layout=FullScreen"
						//L"&Skin/ShowLabels=n"	
						//L"&ScreenEngine/RenderScaling=0.5"
					L"}";
	
		cmd = c.data();
	}


	//_try
	{
		Create(instance, cmd);
	}
	//__except(RecordExceptionInfo(GetExceptionInformation(), L"WinMain",  mw::GetSupervisorDataFolder()))
	{
		// Do nothing here - RecordExceptionInfo() has already done
		// everything that is needed. Actually this code won't even
		// get called unless you return EXCEPTION_EXECUTE_HANDLER from
		// the __except clause.
	}
	
	//RestoreSystem(mw::GetRestoreFolder());

	delete x;

	return 0;
}

