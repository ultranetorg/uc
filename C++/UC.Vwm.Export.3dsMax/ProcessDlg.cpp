#include "StdAfx.h"
#include "ProcessDlg.h"

namespace uos
{
	CProcessDlg * CProcessDlg::This;

	CProcessDlg::CProcessDlg()
	{
		if(This != null)
		{
			throw CException(HERE, L"Instance already exists");
		}
		This = this;
	}

	CProcessDlg::~CProcessDlg()
	{
		This = null;
	}

	INT_PTR CProcessDlg::DlgProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
	{
		switch(message) 
		{
			case WM_INITDIALOG:
				This->Init(hDlg);
				return false;

			case WM_DESTROY:
				This->Destroy();
				return false;

			case WM_COMMAND:
			{
				switch(LOWORD(wParam))
				{
					case IDOK:
						EndDialog(hDlg, IDOK);
						break;
				}
				break;
			}				
		}
		return 0;
	}

	void CProcessDlg::Init(HWND dlg)
	{
		m_Dlg = dlg;
		m_Source->MaxInterface->RegisterDlgWnd(m_Dlg);

		m_ProgressA			= 0;
		m_ProgressCurrent	= 0;

		SendDlgItemMessage(m_Dlg, IDC_PROGRESS_A,					PBM_SETRANGE, 0, MAKELPARAM(0, 100)); 
		SendDlgItemMessage(m_Dlg, IDC_PROGRESS_CURRENT,				PBM_SETRANGE, 0, MAKELPARAM(0, 100)); 
		SendDlgItemMessage(m_Dlg, IDC_PROGRESS_A,					PBM_SETPOS, 0, 0); 
		SendDlgItemMessage(m_Dlg, IDC_PROGRESS_CURRENT,				PBM_SETPOS, 0, 0); 
		SendDlgItemMessage(m_Dlg, IDC_TXT_MESSAGES,					WM_SETTEXT, 0, NULL);
		SendDlgItemMessage(m_Dlg, IDC_TXT_ERRORS,					WM_SETTEXT, 0, NULL);

		CenterWindow(m_Dlg, m_Source->MaxInterface->GetMAXHWnd());

		ShowWindow(m_Dlg, SW_SHOW);
		UpdateWindow(m_Dlg);
		
		m_Handler->OnProcessInit();
		
		SetFocus(GetDlgItem(m_Dlg, IDOK));
	}
	
	void CProcessDlg::Destroy()
	{
		m_Source->MaxInterface->UnRegisterDlgWnd(m_Dlg);
	}

	void CProcessDlg::Show(CSource * s, IProcessHandler * handler)
	{
		m_Source	= s;
		m_Handler	= handler;
	
		DialogBox(m_Source->Instance, MAKEINTRESOURCE(IDD_PROCESS_FORM), m_Source->MaxInterface->GetMAXHWnd(), DlgProc);
	}	

	void CProcessDlg::SetProgressOverall(int i)
	{
		if(m_ProgressA != i)
		{
			SendDlgItemMessage(m_Dlg, IDC_PROGRESS_A, PBM_SETPOS, i, 0); 
		}
	}

	void CProcessDlg::SetProgressCurrent(int i)
	{
		if(m_ProgressCurrent != i)
		{
			SendDlgItemMessage(m_Dlg, IDC_PROGRESS_CURRENT, PBM_SETPOS, i, 0); 
		}
	}

	void CProcessDlg::ReportMessage(const wchar_t * msg, ...)
	{
		va_list marker;
		va_start(marker, msg);
		wchar_t buf[1024];
		vswprintf_s(buf, 1024, msg, marker);
		va_end(marker);

		SendDlgItemMessageW(m_Dlg, IDC_TXT_MESSAGES, EM_SETSEL, 0, -1);
		SendDlgItemMessageW(m_Dlg, IDC_TXT_MESSAGES, EM_SETSEL, -1, -1);
		SendDlgItemMessageW(m_Dlg, IDC_TXT_MESSAGES, EM_REPLACESEL, 0, (WPARAM)buf);

		UpdateWindow(GetDlgItem(m_Dlg, IDC_TXT_MESSAGES));
	}


	void CProcessDlg::ReportError(const wchar_t * msg, ...)
	{
		va_list marker;
		va_start(marker, msg);
		wchar_t buf[1024];
		vswprintf_s(buf, 1024, msg, marker);
		va_end(marker);

		SendDlgItemMessageW(m_Dlg, IDC_TXT_ERRORS, EM_SETSEL, 0, -1);
		SendDlgItemMessageW(m_Dlg, IDC_TXT_ERRORS, EM_SETSEL, -1, -1);
		SendDlgItemMessageW(m_Dlg, IDC_TXT_ERRORS, EM_REPLACESEL, 0, (WPARAM)buf);

		UpdateWindow(GetDlgItem(m_Dlg, IDC_TXT_ERRORS));
	}

	void CProcessDlg::ReportProgress(int p)
	{
		SetProgressCurrent(p);
	}

}
