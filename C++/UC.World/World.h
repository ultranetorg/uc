#pragma once
#include "IDropTarget.h"
#include "Unit.h"
#include "WorldView.h"
#include "PositioningArea.h"
#include "WorldCapabilities.h"

namespace uc
{

	enum class EWorldAction
	{
		Null,
		One,
		Three,
		Move
	};

	struct CWorldMode
	{
		CString			Name;
		CTonDocument *	WorldConfig;
		CTonDocument *	EngineConfig;

		const static inline CString DESKTOP		= L"Desktop";
		const static inline CString MOBILE_E	= L"Mobile.E";
		const static inline CString VR_E		= L"VR.E";
	};

	#define WORLD_HISTORY	L"History"
	#define WORLD_BOARD		L"Board"
	#define WORLD_TRAY		L"Tray"

	class CWorldProtocol : public CWorldLevel, public CWorldCapabilities, public virtual IProtocol
	{
		public:
			inline static const CString								InterfaceName = L"World1";

			CList<CWorldMode>										Modes;
						
			CList<CScreenViewport *>								Viewports;
			CScreenViewport *										MainViewport;

			CVisualGraph *											VisualGraph;
			CActiveGraph *											ActiveGraph;

			CWorldView *											ThemeView = null;
			CWorldView *											MainView = null;
			CWorldView *											HudView = null;
			CWorldView *											NearView = null;

			CTonDocument *											WorldConfig;

			CString													Layout;

			CEvent<CUnit *, CTransformation &, CShowParameters *>	UnitOpened;	
			CEvent<CUnit *>											UnitClosed;	
			CEvent<EWorldAction>									ActionActivated;
			CMap<CString, CUol>										Components;
			CEvent<CInputArgs *>									ServiceSurfaceButtonEvent;

			bool													Initializing = false;
			bool													Starting = true;

			CArea *													Area = null;
			CArea *													ServiceBackArea = null;
			CArea *													ThemeArea = null;
			CPositioningArea *										BackArea = null;
			CPositioningArea *										FieldArea = null;
			CPositioningArea *										MainArea = null;
			CPositioningArea *										HudArea = null;
			CPositioningArea *										TopArea = null;
			CArea *													ServiceFrontArea = null;

			CModel *												Sphere = null;

			CMap<EKeyboardControl, std::function<void(EKeyboardControl)>>	GlobalHotKeys;

			virtual CGroup *										CreateGroup(CString const & name)=0;

			virtual CUol											GenerateAvatar(CUol & entity, CString const & type)=0;
			virtual CAvatar *										CreateAvatar(CUol & avatar, CString const & dir)=0;
			virtual void											DestroyAvatar(CAvatar * a)=0;
						
			virtual CUnit *											AllocateUnit(CModel * m)=0;
			virtual CUnit *											AllocateUnit(CUol & entity, CString const & type)=0;
			//virtual CObject<CAvatarization>							RestoreAvatar(CUol & avatar, CString const & type)=0;
			//virtual CAvatarization *								Avatarize(CModel * m)=0;
			virtual void											Dealloc(CUnit * a)=0;
			//virtual CUnit *											FindUnit(CString const & name, CUol & avatar, CUol & entity)=0;
			virtual CUnit *											OpenEntity(CUol & u, CString const & mainSpace, CShowParameters * f)=0;
			//virtual CAllocation *									OpenAllocation(CUol & u, CString const & mainSpace, CShowFeatures & f)=0;
			virtual void											Show(CUnit * a, CString const & mainSpace, CShowParameters * f)=0;
			virtual void											Hide(CUnit * a, CHideParameters * hf)=0;
			
			virtual void											RunAnimation(CElement * n, CAnimated<CTransformation> a)=0;
			
			virtual void											Attach(CElement * m, CUol & l)=0;
			virtual void											Detach(CElement * m, CUol & l)=0;
			virtual bool											IsAttachedTo(CUol & l, CElement * to)=0;
			virtual bool											IsAttachedTo(CUol & l, CUol & to)=0;
			
//			virtual void											AttachSpace(CWorldSpace * s, CVisualGraph * vg, CActiveGraph * ag)=0;
//			virtual void											DetachSpace(CWorldSpace * s, CVisualGraph * vg, CActiveGraph * ag)=0;
			
			virtual void											Drag(CArray<CDragItem> & d)=0;
			virtual void											CancelDragDrop()=0;

			virtual CConnection<CAvatarProtocol>					FindAvatarSystem(CUol & e, CString const & type)=0;

			CWorldProtocol(CNexus * l){}
			virtual ~CWorldProtocol(){}
	};
}
