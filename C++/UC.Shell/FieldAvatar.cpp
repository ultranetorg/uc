#include "stdafx.h"
#include "FieldAvatar.h"

using namespace uc;

CFieldAvatar::CFieldAvatar(CShellLevel * l, CString const & name) : CModel(l->World, l->Server, ELifespan::Permanent, name)
{
	Level = l;

	WTitle = new CText(l->World, l->Style, L"Title", true);
	WTitle->SetWrap(true);
	WTitle->SetFont(l->Style->GetFont(L"Title/Font"));
	AddNode(WTitle);
	WTitle->Free();
		
	auto s = l->Style->Document->One(L"Field/Item");

	FieldElement = new CFieldElement(Level);
	FieldElement->ItemMargin		= s->Get<CFloat6>(L"M");
	FieldElement->ItemPadding		= s->Get<CFloat6>(L"P");
	FieldElement->IconSize			= s->Get<CSize>(L"Size");
	FieldElement->IconTitleMode		= ToAvatarTitleMode(s->Get<CString>(L"IconTitleMode"));
	FieldElement->WidgetTitleMode	= ToAvatarTitleMode(s->Get<CString>(L"WidgetTitleMode"));
	FieldElement->Placing			+= ThisHandler(OnPlacing);
	FieldElement->Active->MouseEvent[EListen::NormalAll]	+= ThisHandler(OnMouse);
	AddNode(FieldElement);

	Surface = new CFieldSurface(Level->World, L"Surface");
	AMesh = new CSolidRectangleMesh(Level->Engine->Level);
	VMesh = new CGridRectangleMesh(Level->Engine->Level);
	Surface->Visual->SetMesh(VMesh);
	Surface->Active->SetMesh(AMesh);
	Surface->Express(L"P", [this]{ return CFloat6(4.f); });
	Surface->Express(L"M", [this]{ return CFloat6(4.f); });

	FieldElement->SetSurface(Surface);

	ShowGrid(true);
}
		
CFieldAvatar::~CFieldAvatar()
{
	OnDependencyDestroying(Entity);

	RemoveNode(FieldElement);

	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	if(FieldElement)
	{
		VMesh->Free();
		AMesh->Free();
		FieldElement->Free();
		Surface->Free();
	}
}

void CFieldAvatar::SetDirectories(CString const & dir)
{
	__super::SetDirectories(dir);
	FieldElement->SetDirectories(dir);
}

void CFieldAvatar::SetEntity(CUol & e)
{
	Entity = Level->Server->FindObject(e);
	Entity->Destroying += ThisHandler(OnDependencyDestroying);
	Entity->Retitled += ThisHandler(OnTitleChanged);
	
	FieldElement->SetField(Entity);

	OnTitleChanged(Entity);
}

void CFieldAvatar::OnDependencyDestroying(CInterObject * o)
{
	if(Entity && o == Entity)
	{
		FieldElement->SetField(null);
		Entity->Retitled -= ThisHandler(OnTitleChanged);
		Entity->Destroying -= ThisHandler(OnDependencyDestroying);
		Entity.Clear();
	}
}

void CFieldAvatar::UpdateLayout(CLimits const & l, bool apply)
{	
	__super::UpdateLayout(l, apply);

	if(apply)
	{
		WTitle->Transform((Size.W - WTitle->W)/2, Size.H - WTitle->H, Z_STEP * 3);
	
		auto w = Surface->IW + (Surface->P.LF + Surface->P.RT);
		auto h = Surface->IH + (Surface->P.BM + Surface->P.TP);

		AMesh->Generate(0, 0, Surface->W, Surface->H);
		VMesh->Generate(Surface->M.LF, Surface->M.BM, 0, w, h, int(w/24), int(h/24), 1.f);
	}
}

void CFieldAvatar::OnTitleChanged(CWorldEntity * e)
{
	WTitle->SetText(e->Title);
}

void CFieldAvatar::OnPlacing(CFieldItemElement * fie)
{
	if(Level->World->Initializing)
	{
		auto am = FieldElement->GetMetrics(AVATAR_ICON2D, ECardTitleMode::Bottom, CSize::Nan);
	
		CFloat2 orig;
		CFloat2 step;
		CRect limit;
	
		float xs;
		float ys;
	
		if(fie->Type == AVATAR_ICON2D)
		{
			xs = 12.f;
			ys = FieldElement->ItemPadding.BM + FieldElement->ItemPadding.TP + FieldElement->ItemMargin.BM  + FieldElement->ItemMargin.TP + 12;

			if(!Capabilities->Tight)
			{
				orig = CFloat2(FieldElement->IW * 0.05f, FieldElement->IH - FieldElement->IH * 0.05f);
				step = CFloat2(xs, -(am.FaceSize.H + am.TextSize.H + ys));
				limit = {FieldElement->IW * 0.05f, FieldElement->IH * 0.05f, FieldElement->IW * 0.5f, FieldElement->IH * 0.95f};
			}
			else
			{
				orig = CFloat2(FieldElement->IW * 0.05f, FieldElement->IH - FieldElement->IH * 0.05f);
				step = CFloat2(xs, -(am.FaceSize.H + am.TextSize.H + ys));
				limit = {FieldElement->IW * 0.05f, FieldElement->IH * 0.05f, FieldElement->IW * 0.95f, FieldElement->IH * 0.95f};
			}
		} 
		else
		{
			xs = 12.f;
			ys = 12.f;
	
			orig	= CFloat2(FieldElement->IW * 0.05f, FieldElement->IH - FieldElement->IH * 0.05f);
			step	= CFloat2(xs, -ys);
			limit	= {FieldElement->IW * 0.05f, FieldElement->IH * 0.05f, FieldElement->IW * 0.95f, FieldElement->IH * 0.95f};
		}
	
		bool failed = false;
	
		float x = orig.x;
		float ytop = orig.y;
		float y = ytop - fie->H;
	
		do 
		{
			for(auto i : FieldElement->Items)
			{
				failed = CRect(x, y, fie->W, fie->H).Intersects(CRect(i->Transformation.Position.x, i->Transformation.Position.y, i->W, i->H));
						
				if(failed)
					break;
			}
		
			if(!failed)
			{
				fie->Transform(x, y, FieldElement->ItemZ);
				break;
			}
	
			x += step.x;
	
			if(x + fie->W > limit.GetRight())
			{
				x = orig.x;
				ytop += step.y;
				y = ytop - fie->H;
			}
	
			if(y < limit.GetBottom())
			{
				auto t = FieldElement->GetRandomFreePosition(fie);
				fie->Transform(t);
				break;
			}
		}
		while(true);
	}
}

void CFieldAvatar::LoadInstance()
{
	__super::LoadInstance();
	FieldElement->Load();
}

void CFieldAvatar::SaveInstance()
{
	__super::SaveInstance();
	FieldElement->Save();
}

void CFieldAvatar::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
	if(arg->Control == EMouseControl::RightButton && arg->Event == EGraphEvent::Click)
	{
		if(s == Surface->Active)
		{
			auto p = Active->Transit(arg->Pick.Active, arg->Pick.Point);

			if(Menu == null)
				Menu = new CRectangleMenu(Level->World, Level->Style, L"FieldContextMenu");
			else
				Menu->Section->Clear();

			FieldElement->AddNewMenu(Menu, p);

			Menu->Section->AddSeparator();
			{
				Level->AddModeSwitch(Menu->Section);
			}

			if(Level->World->Complexity == AVATAR_ENVIRONMENT)
			{
				Menu->Section->AddSeparator();
				Menu->Section->AddItem(L"Hide")->Clicked =  [this](auto, auto){ Level->World->Hide(Unit, null); };
				Menu->Section->AddItem(L"Push")->Clicked =	[this, arg](auto, auto) mutable 
															{ 
																Level->World->Show(Unit, CArea::Background, sh_new<CShowParameters>(arg, Level->Style));
															};
			}

			Menu->Open(arg->Pick);
			
			arg->StopPropagation = true;
		}
	}
}

void CFieldAvatar::ShowGrid(bool s)
{
	if(s)
	{
		Surface->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 1 1 0.05"));
		FieldElement->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0 0.8"));
	}
	else
	{
		Surface->Visual->SetMaterial(null);
		FieldElement->Visual->SetMaterial(Level->World->Materials->GetMaterial(L"0 0 0 0.3"));
	}
}

