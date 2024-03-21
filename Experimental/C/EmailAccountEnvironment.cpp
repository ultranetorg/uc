#include "stdafx.h"
#include "EmailAccountEnvironment.h"

using namespace uc;

CEmailAccountEnvironment::CEmailAccountEnvironment(CExperimentalLevel * l, CString const & name) : CEnvironmentWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	Load(l->Style, Server->MapReleasePath(L"EmailAccount.uwm"));
}

CEmailAccountEnvironment::~CEmailAccountEnvironment()
{
	OnDependencyDestroying(Entity);
}

void CEmailAccountEnvironment::SetEntity(CUol & e)
{
	__super::SetEntity(e);
	Entity = __super::Entity->As<CEmailAccount>();
	Entity->Destroying += ThisHandler(OnDependencyDestroying);

	One<CTextEdit>(L"server")->SetText(Entity->Server.ToString());
	One<CTextEdit>(L"user")->SetText(Entity->User);
	One<CTextEdit>(L"password")->SetText(Entity->Password);

	One<CButton>(L"ok")->Pressed += [this](auto)
									{
										Entity->Server		= One<CTextEdit>(L"server")->GetText();
										Entity->User		= One<CTextEdit>(L"user")->GetText();
										Entity->Password	= One<CTextEdit>(L"password")->GetText();

										Entity->Changed();

										Level->World->Hide(Unit, null);
									};

	One<CButton>(L"cancel")->Pressed += [this](auto)
										{
											Level->World->Hide(Unit, null);
										};
}

void CEmailAccountEnvironment::OnDependencyDestroying(CInterObject * o)
{
	if(o == Entity && Entity)
	{
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity = null;
	}
}

void CEmailAccountEnvironment::DetermineSize(CSize & smax, CSize & s)
{
	if(s)
	{
		Express(L"W", [s]{ return s.W; });
		Express(L"H", [s]{ return s.H; });
		UpdateLayout({smax, smax}, false);
	}
}
