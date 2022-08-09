#pragma once
#include "IShellFriend.h"

using namespace uc;

namespace uc
{ 
	class CFieldAvatar : public CModel
	{
		public:
			CObject<CFieldServer>						Entity;
			CFieldElement *								FieldElement = null;
			CShellLevel *								Level;
			CRectangleMenu *							Menu = null;
			CRectangleMenu *							ServiceMenu = null;
			CText *										WTitle;

			CFieldSurface *								Surface;
			CSolidRectangleMesh *						AMesh;
			CGridRectangleMesh *						VMesh;

			UOS_RTTI
			CFieldAvatar(CShellLevel * l, const CString & name);
			~CFieldAvatar();

			///bool										StartWorldOperation(CActive * s, CPick & is, CInputMessage & im) override;

			void										SetDirectories(CString const & dir) override;
			void										SaveInstance();
			void										LoadInstance();

			void  										UpdateLayout(CLimits const & l, bool apply) override;

			void										SetEntity(CUol & d) override;

			void										ShowGrid(bool s);

			void										OnDependencyDestroying(CBaseNexusObject * o);
			void										OnTitleChanged(CWorldEntity * e);
			void										OnPlacing(CFieldItemElement * fie);
			void										OnMouse(CActive * r, CActive * s, CMouseArgs * a);
	};
}

