#include "stdafx.h"
#include "CefElement.h"

using namespace uc;
using namespace std;

CCefElement::CCefElement(CExperimentalLevel * l, CStyle * s, CString const & name) : CRectangle(l->World, name)
{
	Level = l;

	UseInner();
	
	InnerMesh->GenerateUV(true);

	auto m = new CMaterial(&Level->Engine->EngineLevel, Level->Engine->PipelineFactory->DiffuseTextureShader);
	VInner->SetMaterial(m);
	m->Free();
	
	Texture = Level->World->Engine->TextureFactory->CreateTexture();
	m->Textures[L"DiffuseTexture"] = Texture;
	m->Samplers[L"DiffuseSampler"].SetFilter(ETextureFilter::Point, ETextureFilter::Point, ETextureFilter::Point);

	Active->KeyboardEvent[EListen::Normal]	+= ThisHandler(OnKeyboard);
	Active->MouseEvent[EListen::Normal]		+= ThisHandler(OnMouse);
	Active->TouchEvent[EListen::Normal]		+= ThisHandler(OnTouch);
	Active->StateChanged					+= ThisHandler(OnStateModified);

	Cef = CefRefPtr<CCefClient>(new CCefClient(Level, Texture, L""));
}

CCefElement::~CCefElement()
{
	Cef = nullptr;

	//Mesh->Free();
	Texture->Free();
}

void CCefElement::Navigate(CString const & url)
{
	Cef->Navigate(url);
}

void CCefElement::Back()
{
	Cef->Browser->GoBack();
}

void CCefElement::Forward()
{
	Cef->Browser->GoBack();
}

void CCefElement::Reload()
{
	Cef->Browser->Reload();
}

void CCefElement::ExecuteJavascript(CString const & js)
{
	Cef->Browser->GetMainFrame()->ExecuteJavaScript(js, "", 0);
}

void CCefElement::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	if(apply && IW > 0 && IH > 0)
	{
		//Mesh->Generate(O.x, O.y, IW, IH);
		Cef->SetSize(IW, IH);
	}
}

void CCefElement::OnStateModified(CActive *, CActive *, CActiveStateArgs * arg)
{
	Cef->SetFocus(arg->New == EActiveState::Active);
}

void CCefElement::OnKeyboard(CActive * r, CActive * s, CKeyboardArgs * arg)
{
	Cef->SendKeyboradButton(arg);
}

void CCefElement::OnMouse(CActive * r, CActive * s, CMouseArgs * arg)
{
/*
	auto unit = GetUnit();

	if(!unit)
	{
		return;
	}

	auto space = unit->VisualSpaces.Match(arg->Pick.Camera->Viewport).Space;
	
	if(!space)
	{
		return;
	}
	*/
	auto p = CPlane(0, 0, -1).Intersect(arg->Pick.Ray.Transform((VInner->FinalMatrix * arg->Pick.Space->Matrix).GetInversed())).ToXY() - O.ToXY();
	
	if(arg->Capture)
	{
		auto c = arg->Capture->Pick.Point;
	
		if(p.x < 0 || IW < p.x)
		{
			p.x = c.x;
		}
		if(p.y < 0 || IH < p.y)
		{
			p.x = c.y;
		}
	}

	if(arg->Event == EGraphEvent::Input)
	{
		if(arg->Action == EMouseAction::Rotation)
		{
			if(arg->Control == EMouseControl::Wheel)
			{
				Cef->SendWheelEvent(r, s, p, CFloat2(0, arg->RotationDelta.y * 100));
			}
		}
	
		if(arg->Action == EMouseAction::On || arg->Action == EMouseAction::Off)
		{
			Cef->LastMouseArgs = sh_assign(Cef->LastMouseArgs, arg);
			
			Cef->SendMouseButtonEvent(r, s, p, arg->Control, arg->Action);
		}
	}

	if(arg->Event == EGraphEvent::Hover || arg->Event == EGraphEvent::Roaming)
	{
		if(p.IsReal())
		{
			Cef->SendCursorEvent(r, s, p, false /*arg->Event == EGraphEvent::Roaming*/);
		}
	}


}

void CCefElement::OnTouch(CActive * r, CActive * s, CTouchArgs * arg)
{
	auto p = CPlane(0, 0, -1).Intersect(arg->GetPick().Ray.Transform((VInner->FinalMatrix * arg->GetPick().Space->Matrix).GetInversed())).ToXY() - O.ToXY();

	if(arg->Event == EGraphEvent::Captured)
	{
		Scrolling = true;
	}

	if(arg->Event == EGraphEvent::Input)
	{
		if(arg->Input->Action == ETouchAction::Movement)
		{
			if(Scrolling)
			{
				Cef->SendWheelEvent(r, s, p, arg->Input->Touch.Delta * CFloat2(1, -1));
			}
		}

		if(arg->Input->Action == ETouchAction::Added)
		{
			if(arg->Input->Touches.size() == 1)
			{
				CapturePoint = p;
			}
		}
	
		if(arg->Input->Action == ETouchAction::Removed)
		{
			if(arg->Input->Touches.size() == 1)
			{
				if(Scrolling)
				{
					Scrolling = false;
				}
				else
				{
					//Cef->SendCursorEvent(r, s, p, false);
					Cef->SendMouseButtonEvent(r, s, p, EMouseControl::LeftButton, EMouseAction::On);


					//Cef->SendCursorEvent(r, s, p, false);
					Cef->SendMouseButtonEvent(r, s, p, EMouseControl::LeftButton, EMouseAction::Off);
				}

			}
		}
	}
}
