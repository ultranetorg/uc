#pragma once
#include "Source.h"

namespace uos
{
	class CMainDlg : public IType
	{
		public:
			INT_PTR						Show();

			UOS_RTTI
			CMainDlg(CSource * s);
			~CMainDlg();

		private:
			CSource					*	Source;
			static CMainDlg			*	This;
			HWND						Dlg;

			void						Init(HWND dlg);
			void						OnOK();
			void						Destroy();
			static INT_PTR CALLBACK		DlgProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam);
	};
}
