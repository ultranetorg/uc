#pragma once
#include "FieldElement.h"
#include "Board.h"
#include "BoardSurface.h"

namespace uc
{
	class CBoardAvatar : public CFieldableModel
	{
		public:
			CObject<CBoard>								Entity;
			CFieldElement *								FieldElement;
			CBoardSurface *								Surface;
			CShellLevel *								Level;
			CRectangleMenu *							ContextMenu = null;

			UOS_RTTI
			CBoardAvatar(CShellLevel * l, CString const & name);
			~CBoardAvatar();

			void										SetEntity(CUol & o);

			void										OnItemPlacing(CFieldItemElement * fie);
			void										OnDependencyDestroying(CBaseNexusObject * o);

			virtual void								SetDirectories(CString const & dir) override;
			virtual void								SaveInstance() override;
			virtual void								LoadInstance() override;
			
			void										OnItemMouse(CActive * r, CActive * s, CMouseArgs * a);
			void										OnItemAdded(CFieldItemElement * fie);
			void										OnItemRemoved(CFieldItemElement * fie);

			void										OnSurfaceToggleEvent(CActive * r, CActive * s, CMouseArgs *);
	};

}
