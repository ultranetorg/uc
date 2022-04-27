#pragma once

#if 0
namespace uos
{
	#define CRASHREPORT_ERROR_LOG_FILE					L"Error.txt"
	#define CRASHREPORT_MINI_DUMP_FILE					L"Crash.dmp"

	int		RecordExceptionInfo(PEXCEPTION_POINTERS data, LPCTSTR Message, wchar_t * supervisorFolderPath);
	BOOL	LaunchReport(wchar_t * supervisorFolderPath);
}
#endif


/*
// Sample usage - put the code that used to be in main into HandledMain.
// To hook it in to an MFC app add ExceptionAttacher.cpp from the mfctest
// application into your project.
int main(int argc, char *argv[])
{
	int Result = -1;
	__try
	{
		Result = HandledMain(argc, argv);
	}
	__except(RecordExceptionInfo(GetExceptionInformation(), "main thread"))
	{
		// Do nothing here - RecordExceptionInfo() has already done
		// everything that is needed. Actually this code won't even
		// get called unless you return EXCEPTION_EXECUTE_HANDLER from
		// the __except clause.
	}
	return Result;
}
*/
