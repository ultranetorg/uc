#include "stdafx.h"
#include "GdiRect.h"

using namespace uc;

const std::wstring CGdiRect::TypeName = L"int32.rect";

CGdiRect CGdiRect::operator+(const CGdiRect & a)
{
	CGdiRect r;
	r.X = min(X, a.X);
	r.Y = min(Y, a.Y);
	r.W = max(GetRight(), a.GetRight()) - r.X;
	r.H = max(GetButtom(), a.GetButtom()) - r.Y;
	return r;
}

bool CGdiRect::operator!=(const CGdiRect & a) const
{
	return !(*this == a);
}

bool CGdiRect::operator==(const CGdiRect & a) const
{
	return a.X == X && a.Y == Y && a.W == W && a.H == H;
}

std::wstring CGdiRect::GetTypeName()
{
	return TypeName;
}

void CGdiRect::Read(CStream * s)
{
	s->Read(&X, 4 * 4);
}

void CGdiRect::Write(std::wstring & s)
{
	s += CString::Format(L"%d %d %d %d", X, Y, W, H);
}

int64_t CGdiRect::Write(CStream * s)
{
	throw CException(HERE, L"Not implemented");
}

void CGdiRect::Read(const std::wstring & v)
{
	auto parts = CString(v).Split(L" ");
	X = CInt32::Parse(parts[0]);
	Y = CInt32::Parse(parts[1]);
	W = CInt32::Parse(parts[2]);
	H = CInt32::Parse(parts[3]);
}

ISerializable * CGdiRect::Clone()
{
	return new CGdiRect(X, Y, W, H);
}

bool CGdiRect::Equals(const ISerializable & a) const
{
	return X == ((CGdiRect &)a).X && Y == ((CGdiRect &)a).Y && W == ((CGdiRect &)a).W && H == ((CGdiRect &)a).H;
}

CGdiRect::CGdiRect(const CBuffer & b)
{
	if(b.GetSize() != sizeof(CGdiRect))
	{
		throw CException(HERE, L"Wrong size");
	}

	*this = *((CGdiRect *)((CBuffer &)b).GetData());
}

CGdiRect::CGdiRect()
{

}

CGdiRect::CGdiRect(int x, int y, int w, int h)
{
	X = x;
	Y = y;
	W = w;
	H = h;
}

CGdiRect::CGdiRect(const RECT & r)
{
	X = int(r.left);
	Y = int(r.top);
	W = int(r.right - r.left);
	H = int(r.bottom - r.top);
}

void CGdiRect::Set(int x, int y, int w, int h)
{
	X = x;
	Y = y;
	W = w;
	H = h;
}

void CGdiRect::MakeEmpty()
{
	X = 0;
	Y = 0;
	W = 0;
	H = 0;
}

CGdiRect CGdiRect::GetLocal()
{
	CGdiRect r = *this;
	r.X = 0;
	r.Y = 0;
	return r;
}

POINT CGdiRect::GetCenter()
{
	POINT p = {X + W / 2, Y + H / 2};
	return p;
}

bool CGdiRect::IsEmpty()
{
	return W <= 0 || H <= 0;
}

CGdiRect CGdiRect::GetInflated(int dx, int dy)
{
	CGdiRect r;
	r.X = X - dx;
	r.Y = Y - dy;
	r.W = W + dx * 2;
	r.H = H + dy * 2;
	return r;
}

bool CGdiRect::IsGreaterOrEqual(int w, int h)
{
	return W >= w && H >= h;
}

bool CGdiRect::IsIntersectWOB(const CGdiRect & a)
{
	if((a.X < X && X < a.X + a.W) && (a.Y < Y && Y < a.Y + a.H))
		return true;

	if((a.X < X + W && X + W < a.X + a.W) && (a.Y < Y && Y < a.Y + a.H))
		return true;

	if((a.X < X && X < a.X + a.W) && (a.Y < Y + H && Y + H < a.Y + a.H))
		return true;

	if((a.X < X + W && X + W < a.X + a.W) && (a.Y < Y + H && Y + H < a.Y + a.H))
		return true;

	return false;
}

bool CGdiRect::IsIntersect(const CGdiRect & a)
{
	if((a.X <= X && X < a.X + a.W) && (a.Y <= Y && Y < a.Y + a.H))
		return true;

	if((a.X < X + W && X + W < a.X + a.W) && (a.Y <= Y && Y < a.Y + a.H))
		return true;

	if((a.X <= X && X < a.X + a.W) && (a.Y < Y + H && Y + H < a.Y + a.H))
		return true;

	if((a.X < X + W && X + W < a.X + a.W) && (a.Y < Y + H && Y + H < a.Y + a.H))
		return true;

	return false;
}

CGdiRect CGdiRect::Intersect(const CGdiRect & a)
{
	int l = 0, r = 0, t = 0, b = 0;

	if(a.X == X)
		l = X;
	if(a.X + a.W == X + W)
		r = X + W;
	if(a.X < X && X < a.X + a.W)
		l = X;
	if(a.X < X + W && X + W < a.X + a.W)
		r = X + W;
	if(X < a.X && a.X < X + W)
		l = a.X;
	if(X < a.X + a.W && a.X + a.W < X + W)
		r = a.X + a.W;

	if(a.Y == Y)
		t = Y;
	if(a.Y + a.H == Y + H)
		b = Y + H;
	if(a.Y < Y && Y < a.Y + a.H)
		t = Y;
	if(a.Y < Y + H && Y + H < a.Y + a.H)
		b = Y + H;
	if(Y < a.Y && a.Y < Y + H)
		t = a.Y;
	if(Y < a.Y + a.H && a.Y + a.H < Y + H)
		b = a.Y + a.H;

	return CGdiRect(l, t, r - l, b - t);
}

int CGdiRect::GetArea()
{
	return W * H;
}

int CGdiRect::GetWidth()
{
	return W;
}

int CGdiRect::GetHeight()
{
	return H;
}

int CGdiRect::GetRight() const
{
	return X + W;
}

int CGdiRect::GetButtom() const
{
	return Y + H;
}

bool CGdiRect::Contain(int x, int y)
{
	return (X <= x && x < X + W) && (Y <= y && y < Y + H);
}

bool CGdiRect::ContainWOB(int x, int y)
{
	return	(X < x && x < X + W) && (Y < y && y < Y + H);
}

RECT CGdiRect::GetAsRECT()
{
	RECT r;
	SetRect(&r, (int)X, (int)Y, (int)GetRight(), (int)GetButtom());
	return r;
}

CString CGdiRect::ToString()
{
	return CString::Format(L"%d %d %d %d", X, Y, W, H);
}
