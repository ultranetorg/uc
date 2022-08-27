#include "StdAfx.h"
#include "PictureWidget.h"

using namespace uc;

CPictureWidget::CPictureWidget(CShellLevel * l, CString const & name) : CWidgetWindow(l->World, l->Server, l->Style, name)
{
	Level = l;

	auto f = new CRectangle(Level->World);
	f->ApplyStyles(Style, {L"Widget"});
	f->Reset(L"B");
	SetFace(f);

	auto m = new CMaterial(Level->Engine->Level, Level->Engine->PipelineFactory->DiffuseTextureShader);
	m->AlphaBlending = true;
	
	Texture = Level->World->Engine->TextureFactory->CreateTexture();

	f->UseInner();
	f->VInner->SetMaterial(m);
	f->VInner->Material->Textures[L"DiffuseTexture"] = Texture;

	m->Free();
	//f->Free();

	Sizer.KeepAspect = true;
}

CPictureWidget::~CPictureWidget()
{
	if(Menu)
	{
		Menu->Close();
		Menu->Free();
	}

	if(Texture)
		Texture->Free();

	Face->Free();
}

void CPictureWidget::SetEntity(CUol & o)
{
	__super::SetEntity(o);
	Entity = __super::Entity->As<CPicture>();

	if(auto s = Level->Storage->ReadFile(Entity->File))
	{
		auto supported = Level->World->Engine->TextureFactory->IsSupported(s);
		Level->Storage->Close(s);

		if(supported)
		{
			Sizer.Resized =	[this]()
							{
								Refresh();
							};
			return;
		}
	}

	Face->As<CRectangle>()->BorderMaterial = Level->World->Materials->GetMaterial(L"1 0 0");
}

void CPictureWidget::UpdateLayout(CLimits const & l, bool apply)
{
	__super::UpdateLayout(l, apply);

	if(Texture->IsEmpty() && apply)
	{
		Refresh();
	}
}

void CPictureWidget::Refresh()
{
	int w = int(Face->W);
	int h = int(Face->H);

	if(w > 1 && h > 1)
	{
		if(auto s = Level->Storage->ReadFile(Entity->File))
		{
			if(Texture->W != w || Texture->H != w)
			{
				Texture->Load(s);
				Texture->Resize(w, h, 0, true);
			}
		
			Level->Storage->Close(s);
		}
	}
}

void CPictureWidget::DetermineSize(CSize & smax, CSize & s)
{
	if(auto f = Level->Storage->ReadFile(Entity->File))
	{
		auto d = Level->Engine->TextureFactory->GetDimension(f);
		Level->Storage->Close(f);
				
		float w = d.W;
		float h = d.H;

		auto a = w/h;

		if(d.W > smax.W)
		{
			w = smax.W;
			h = w / a;
		}
		else if(d.H > smax.H)
		{
			h = smax.H;
			w = h * a;
		}

		if(!Capabilities->FullScreen)
		{
			Express(L"IW", [w]{ return w; });
			Express(L"IH", [h]{ return h; });
			Face->Reset(L"P");

			Face->Visual->SetMaterial(null);
		}
		else
		{
			CFloat6 p = {0, 0, 0, 0, 0, 0};

			if(w < smax.W)
				p.LF = p.RT = (smax.W - w)/2;

			if(h < smax.H)
				p.BM = p.TP = (smax.H - h)/2;

			Face->Express(L"P", [p]{ return p; });

			Express(L"IW", [smax]{ return smax.W; });
			Express(L"IH", [smax]{ return smax.H; });
		}
	}


	UpdateLayout(CLimits::Empty, false);
}