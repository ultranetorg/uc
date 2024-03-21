#pragma once

namespace uc
{
	class CFieldSurface : public CElement
	{
		public:
			CArray<CFloat2>		Polygon;

			CFieldSurface(CWorldProtocol * l, CString const & name) : CElement(l, name)
			{
				Express(L"W", [this]{ return Slimits.Smax.W; });
				Express(L"H", [this]{ return Slimits.Smax.H; });
			}

			~CFieldSurface()
			{
			}

			virtual void UpdateLayout(CLimits const & l, bool apply) override
			{
				__super::UpdateLayout(l, apply);

				Polygon = {{0, 0}, {0, H}, {W, H}, {W, 0}};
			}
	};
}

