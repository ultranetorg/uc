#include "StdAfx.h"
#include "MainDlg.h"

namespace uos
{
	CMainDlg *	CMainDlg::This;

	CMainDlg::CMainDlg(CSource * s)
	{
		if(This != null)
		{
			throw CException(HERE, L"Instance already exists");
		}
		This = this;
		Source = s;
	}

	CMainDlg::~CMainDlg()
	{
		This = null;
	}

	INT_PTR CMainDlg::DlgProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
	{
		switch(message) 
		{
			case WM_INITDIALOG:
				This->Init(hDlg);
				This->OnOK();
				break;

			case WM_DESTROY:
				This->Destroy();
				break;

			case WM_COMMAND:
			{
				switch(LOWORD(wParam))
				{
					case IDOK:
					{
						This->OnOK();
						break;
					}
					case IDCANCEL:
						EndDialog(hDlg, IDCANCEL);
						break;
				}
				break;
			}
		}

		return 0;
	}

	INT_PTR CMainDlg::Show()
	{
		return DialogBox(Source->Instance, MAKEINTRESOURCE(IDD_MAIN_FORM), Source->MaxInterface->GetMAXHWnd(), DlgProc);
	}	

	void CMainDlg::Init(HWND dlg)
	{
		Dlg = dlg;
		Source->MaxInterface->RegisterDlgWnd(Dlg);

		SetFocus(GetDlgItem(Dlg, IDOK));
		CenterWindow(Dlg, Source->MaxInterface->GetMAXHWnd());
		
		SetDlgItemTextW(Dlg, IDC_EDIT_FILE_PATH, Source->DestFilePath.c_str());
	}

	void CMainDlg::Destroy()
	{
		Source->MaxInterface->UnRegisterDlgWnd(Dlg);
	}
	
	void CMainDlg::OnOK()
	{
		wchar_t p[MAX_PATH];
		::GetDlgItemTextW(Dlg, IDC_EDIT_FILE_PATH, p, MAX_PATH);
		
		
		Source->DestFilePath = p;
		EndDialog(Dlg, IDOK);
	}	
}