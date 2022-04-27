#pragma once
#include "FieldItemElement.h"
#include "FieldSurface.h"

using namespace uc;

namespace uc
{
	enum class EFieldPositioningType
	{
		Null, Lay, Stand
	};

	class UOS_SHELL_IMPORTEXPORT CFieldElement : public CRectangle, public uc::IDropTarget, public CFieldWorld
	{
		public:
			CFieldServer *								Entity = null;
			CShellLevel *								Level;
			
			CPolygonalPositioning						Positioning;
			EFieldPositioningType						PositioningType = EFieldPositioningType::Null;

			bool										InMoving = false;
			bool										InSelecting = false;

			CRefList<CFieldItemElement *>				Items;
			CRefList<CFieldItemElement *>				Selectings;
			CString										Directory;
			CString										DefaultAvatarType = AVATAR_ICON2D;
			CSize										IconSize = CSize(24, 24, 24);
			ECardTitleMode								IconTitleMode = ECardTitleMode::No;
			ECardTitleMode								WidgetTitleMode = ECardTitleMode::No;
			CTransformation								ItemTransformation = CTransformation::Identity;
			CFloat6										ItemMargin =  CFloat6(0);
			CFloat6										ItemPadding = CFloat6(0);
			float										ItemZ = Z_STEP * 10;

			CFieldSurface *								Surface = null;
			CAABB										ContentBBox;
			int											OperationID = 0;
			CVisual *									Selection;
			CFrameMesh *								SelectionMesh;

			CEvent<CFieldItemElement *>					Added;
			CEvent<CFieldItemElement *>					Removed;
			CEvent<CFieldItemElement *>					Placing;

			CRectangleMenu *							Menu = null;

			UOS_RTTI
			CFieldElement(CShellLevel * l, CString const & name = GetClassName());
			~CFieldElement();
			
			using CRectangle::UpdateLayout;

			void										SetDirectories(CString const & dir);
			
			virtual void								SetField(CFieldServer * o);
			void										SetSurface(CFieldSurface * e);
			
			CSize										Arrange(CRefList<CFieldItemElement *> & items, EXAlign xa);
			void										TransformItems(CRefList<CFieldItemElement *> & items, CTransformation & t);
			
			virtual CFieldItemElement *					AddItem(CFieldItem * fi);
			void										RemoveItem(CFieldItemElement * h);
			void										DeleteItem(CFieldItemElement * fi);
			CFieldItemElement *							FindItem(CActive * a);

			bool										Test(CArray<CDragItem> & d, CAvatar * n) override;
			CModel * 									Enter(CArray<CDragItem> & d, CAvatar * n) override;
			void										Leave(CArray<CDragItem> & d, CAvatar * n) override;
			void										Drop(CArray<CDragItem> & d, CPick & a) override;

			void										Clear();
			void										Load();
			void										Save();

			CTransformation								GetRandomPosition(CFieldItemElement * fie);
			CTransformation								GetRandomFreePosition(CFieldItemElement * fie);

			void										OnMouseFilter(CActive * r, CActive * s, CMouseArgs * arg);
			void										OnMouse(CActive * r, CActive * s, CMouseArgs *);

			CCardMetrics								GetMetrics(CString const & am, ECardTitleMode tm, const CSize & a);

			CRect										FindBounds(CRefList<CFieldItemElement *> & items);

			void										OnTouch(CActive * r, CActive * s, CTouchArgs * arg);
			void										OnItemStateModified(CActive *, CActive *, CActiveStateArgs *);

			void										StartMovement(CActive * s, CInputArgs * arg, CNodeCapture * c);
			void										ProcessMovement(CInputArgs * arg, CPick & pk);
			void										FinishMovement(CInputArgs * arg);

			void										StartSelecting(CInputArgs * arg);
			void										ProcessSelecting(CNodeCapture * c, CPick & pk);
			void										FinishSelecting();

			virtual void								MoveAvatar(CAvatar * a, CTransformation & p) override;
			virtual void								DeleteAvatar(CAvatar * a) override;
			virtual CPositioning *						GetPositioning() override;
			virtual void								AddIconMenu(IMenuSection * ms, CAvatar * a) override;
			virtual void								AddTitleMenu(IMenuSection * ms, CAvatar * a) override;
			
			void										AddNewMenu(CRectangleMenu * menu, CFloat3 & p);

			CRefList<CFieldItemElement *>				Find(CList<CUol> & items);
			CFieldItemElement *							Find(CUol & o);
	
			void										OnItemAdded(CFieldItem * item);

			virtual void								UpdateLayout(CLimits const & l, bool apply) override;
	};
}
