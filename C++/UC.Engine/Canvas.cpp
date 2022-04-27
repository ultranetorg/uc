#include "stdafx.h"
#include "Canvas.h"

using namespace uc;

CCanvas::CCanvas(CEngineLevel * l, CDirectSystem * e, CDirectDevice * d, ID3D11Resource * t, CFloat2 & s)
{
	Engine = e;
	Device = d;
	Texture = t;
	Scaling = s;

	Verify(t->QueryInterface(&Surface));
	
	auto  props = D2D1::RenderTargetProperties(D2D1_RENDER_TARGET_TYPE_DEFAULT, D2D1::PixelFormat(DXGI_FORMAT_UNKNOWN, D2D1_ALPHA_MODE_PREMULTIPLIED), 96.f * s.x, 96.f * s.y, D2D1_RENDER_TARGET_USAGE_GDI_COMPATIBLE);
	Verify(e->D2D->CreateDxgiSurfaceRenderTarget(Surface, &props, &RenderTarget));

	RenderTarget->QueryInterface(__uuidof(ID2D1GdiInteropRenderTarget), (void**)&GdiRenderTarget); 

	RenderTarget->BeginDraw();
	RenderTarget->SetAntialiasMode(D2D1_ANTIALIAS_MODE_ALIASED);
	RenderTarget->SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE_ALIASED);

	Surface->GetDesc(&Description);

	//RenderTarget->SetDpi();

	W = Description.Width / s.x;
	H = Description.Height / s.y;
	   
/*
	IDWriteRenderingParams * drp;
	GraphicEngine->DWrite->CreateRenderingParams(&drp);

	auto ctl = drp->GetClearTypeLevel();

	IDWriteRenderingParams * nrp;
	GraphicEngine->DWrite->CreateCustomRenderingParams(drp->GetGamma(), 
													   drp->GetEnhancedContrast(), 
													   drp->GetClearTypeLevel(),
													   DWRITE_PIXEL_GEOMETRY_FLAT, 
													   DWRITE_RENDERING_MODE_ALIASED,
													   &nrp);

	RenderTarget->SetTextRenderingParams(nrp);*/
}

CCanvas::~CCanvas()
{
	RenderTarget->EndDraw();
	RenderTarget->Release();
	GdiRenderTarget->Release();
	Surface->Release();
}

CSolidColorBrush * CCanvas::CreateSolidBrush(CFloat4 & c)
{
	return new CSolidColorBrush(this, c);
}

void CCanvas::DrawLine(CFloat2 & a, CFloat2 & b, float sw, CSolidColorBrush * brush)
{
	RenderTarget->DrawLine(D2D1::Point2F(a.x, H - a.y), D2D1::Point2F(b.x, H - b.y), brush->Brush, sw, null);
}

void CCanvas::FillRectangle(CRect & r, CSolidColorBrush * brush)
{
	RenderTarget->FillRectangle(ToDx(r), brush->Brush);
}

void CCanvas::DrawRectangle(CRect & r, CSolidColorBrush * brush, float sw)
{
	auto t = sw;

	auto rr = ToDx(r);
	rr.left		= rr.left + t;
	rr.top		= rr.top + t;
	rr.right	= rr.right - t;
	rr.bottom	= rr.bottom - t;

	RenderTarget->DrawRectangle(rr, brush->Brush, t);
}

void CCanvas::Clear(CFloat4 & c)
{
	RenderTarget->Clear(D2D1::ColorF(c.x, c.y, c.z, c.w));
}

void CCanvas::DrawText(CString const & t, CFont * f, CSolidColorBrush * b, CRect & r)
{
///	RenderTarget->DrawText(t.data(), t.size(), f->Format, ToDx(r), b->Brush, D2D1_DRAW_TEXT_OPTIONS_NONE);
	DrawText(t, f, b->Color, r, EXAlign::Left, EYAlign::Top, false, false);
}

void CCanvas::DrawText(CString const & t, CFont * f, CFloat4 & c, CRect & r, EXAlign xa, EYAlign ya, bool wrap, bool ells)
{
	ID3D11RenderTargetView * rtv[D3D11_SIMULTANEOUS_RENDER_TARGET_COUNT];

	Device->DxContext->OMGetRenderTargets(_countof(rtv), rtv, null);

	HDC dc;
	Verify(GdiRenderTarget->GetDC(D2D1_DC_INITIALIZE_MODE_COPY, &dc));

	CRect sr;
	sr.X = r.X * Scaling.x;
	sr.Y = r.Y * Scaling.y;
	sr.W = r.W * Scaling.x;
	sr.H = r.H * Scaling.y;

	auto rr = sr.ToRECT(CSize(Description.Width, Description.Height, 0));

	f->Draw(t, dc, rr, c, wrap, ells, TabLength, xa, ya);

	Verify(GdiRenderTarget->ReleaseDC(NULL));
	   
	for(auto i : rtv)
		if(i)
			i->Release();
}

CBitmap * CCanvas::CreateBitmap(CImage * i)
{
	auto b = new CBitmap(this, i->Size);
	b->Load(i);
	return b;
}

void CCanvas::DrawBitmap(CBitmap * b, CFloat2 & p)
{
	RenderTarget->DrawBitmap(b->Bitmap, ToDx(p, b->Size));
}

D2D1_RECT_F CCanvas::ToDx(CFloat2 & p, CSize & s)
{
	return D2D1::RectF(p.x, H - (p.y + s.H), p.x + s.W, H - p.y);
}

D2D1_RECT_F CCanvas::ToDx(CRect & r)
{
	return D2D1::RectF(r.X, H - (r.Y + r.H), r.X + r.W, H - r.Y);
}