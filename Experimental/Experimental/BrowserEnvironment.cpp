#include "StdAfx.h"
#include "BrowserEnvironment.h"

using namespace uc;

CBrowserEnvironment::CBrowserEnvironment(CExperimentalLevel * l, const CString & name) : CEnvironmentWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	Active->MouseEvent[EListen::NormalAll] += ThisHandler(OnMouse);

	Load(l->Style, Server->MapReleasePath(L"Browser.uwm"));

	BackButton		= One<CButton>(L"back");
	ForwardButton	= One<CButton>(L"forward");
	ReloadButton	= One<CButton>(L"reload");
	AddressEdit		= One<CTextEdit>(L"address");
	CefElement		= One<CCefElement>(L"browser");

	CefElement->Cef->TitleChanged		+= ThisHandler(OnCefTitleChanged);
	CefElement->Cef->AddressChanged		+= ThisHandler(OnCefAddressChanged);

	AddressEdit->Active->KeyboardEvent[EListen::Normal] +=	[this](auto, auto, auto arg)
															{
																if(arg->Control == EKeyboardControl::Return && arg->Action == EKeyboardAction::On)
																{
																	CefElement->Navigate(AddressEdit->Lines[0]);
																}
															};
	
	BackButton->Pressed		+= [this](auto){ CefElement->Back(); };
	ForwardButton->Pressed	+= [this](auto){ CefElement->Forward(); };
	ReloadButton->Pressed	+= [this](auto){ CefElement->Reload(); };
}

CBrowserEnvironment::~CBrowserEnvironment()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}
}

void CBrowserEnvironment::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CBrowser>();

	CefElement->Navigate(Entity->Address.ToString());
}

void CBrowserEnvironment::OnCefTitleChanged(CString const & t)
{
	Entity->SetTitle(t);
}

void CBrowserEnvironment::OnCefAddressChanged(CString & url)
{
	AddressEdit->SetText(url);
	AddressEdit->UpdateLayout();
	Entity->SetAddress(CUrl(url));
}

void CBrowserEnvironment::DetermineSize(CSize & smax, CSize & s)
{
	//if(!Size) - dont care about Size if we UWM used
	{
		if(s)
		{
			Express(L"W", [s]{ return s.W; });
			Express(L"H", [s]{ return s.H; });
		}
		else
		{
			Express(L"W", [smax]{ return smax.H * 0.8f; });
			Express(L"H", [smax]{ return smax.H * 0.8f; });
		}
	}
	UpdateLayout({smax, smax}, false);
}

void CBrowserEnvironment::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(Menu && s->HasAncestor(Menu->Active))
		return;

	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(!Menu)
		{
			Menu = new CRectangleMenu(Level->World, Level->Style, L"ContextMenu");
		}
			
		Menu->Section->Clear();


		Menu->Section->AddItem(L"Back")->Clicked =		[this](auto, auto *){	CefElement->Back();		};
		Menu->Section->AddItem(L"Forward")->Clicked =	[this](auto, auto *){	CefElement->Forward();	};
		Menu->Section->AddItem(L"Reload")->Clicked =	[this](auto, auto *){	CefElement->Reload();	};


		Menu->Section->AddSeparator();
		Menu->Section->AddItem(L"Properties...")->Clicked =	[this](auto a, auto *)
															{
																//auto url = Level->Hub->Url;
																//url[L"object"] = Name.Replace(GetClassName(), CLinkProperties::GetClassName());
																//auto aa = Level->World->Open(url, Level->World->FrontSpace, CShowFeatures(a));
																//aa->Model->Get<CLinkProperties>()->SetSource(Url);
																//aa->Model->UpdateArea(aa->Model->MaxArea, true);
															};
		Menu->Open(arg->Pick);
	}
}
