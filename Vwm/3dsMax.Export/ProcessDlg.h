#pragma once
#include "Source.h"
#include "IProcessHandler.h"

namespace uos
{
	class CProcessDlg : public IWriterProgress, public IType
	{
		public:
			void								Show(CSource * s, IProcessHandler * handler);
		
			void								SetProgressOverall(int i);
//			void								SetProgressPhaseB(int i);
//			void								SetProgressPhaseC(int i);
			void								SetProgressCurrent(int i);
			void								ReportMessage(const wchar_t * msg, ...);
			void								ReportError(const wchar_t * msg, ...);

			UOS_RTTI
			CProcessDlg();
			~CProcessDlg();

		private:
			static CProcessDlg				*	This;
			HWND								m_Dlg;
			int									m_ProgressA;
//			int									m_ProgressB;
//			int									m_ProgressC;
			int									m_ProgressCurrent;
			CSource							*	m_Source;
			IProcessHandler					*	m_Handler;

			void								ReportProgress(int p);

			static INT_PTR CALLBACK				DlgProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam);
			void								Init(HWND dlg);
			void								Destroy();
			void								StartExport();
	};
}
