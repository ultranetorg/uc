#pragma once
#include "World.h"
#include "IWorldFriend.h"
#include "IUwmServer.h"
#include "CylindricalPositioning.h"
#include "PolygonPositioning.h"
#include "Sphere.h"

namespace uc
{
	class CMobileSkinModel;
	
	class CWorldServer : public CPersistentServer, public CWorldProtocol, public CExecutorProtocol, public IViewStore, public CUwmProtocol, public CAvatarProtocol
	{
		public:
			using CWorldLevel::Storage;
			using CWorldLevel::Nexus;

			CList<CUnit *>								Units;
			CList<CUnit *>								Showings;
			CArray<CUnit *>								Movings;
			CList<CUnit *>								Hidings;

			CPositioningCapture							Capture;
			
			CDiagnostic *								Diagnostic;
			CDiagGrid									DiagGrid;
			IPerformanceCounter *						PfcUpdate;
			
			CTonDocument *								AreasConfig;
			CConfig *									EngineConfig;
			
			CArray<CDragItem>							Drags;
			CObject<CAvatar>							DragAllocation = null;
			CAvatar *									DragDefaultAvatar = null;
			CAvatar *									DragCurrentAvatar = null;
			CElement *									DropTarget = null;

			bool										InSpecialArea = false;
			bool										InStopping = false;
	
			CList<CScreenRenderTarget *>				Targets;
			CMap<CViewport *, CScreenRenderLayer *>		RenderLayers;
			CMap<CViewport *, CActiveLayer *>			ActiveLayers;
								   
			int											ScreenshotId;

			float										Z;
			float										Fov;

			UOS_RTTI
			CWorldServer(CNexus * l, CServerInstance * si);
			~CWorldServer();

			void										Initialize() override;
			void										Start() override;
			IProtocol *									Accept(CString const & iface) override;
			void										Break(IProtocol * iface) override;

			void										EstablishConnections();

			CGroup *									CreateGroup(CString const & name) override;

			virtual void								InitializeViewports(){}
			virtual void								InitializeGraphs(){}
			virtual void								InitializeView(){}
			virtual void								InitializeAreas(){}
			virtual void								InitializeModels(){}

			virtual void								Execute(CXon * command, CExecutionParameters * parameters) override;

			CUol										GenerateAvatar(CUol & entity, CString const & type) override;
			CAvatar *									CreateAvatar(CUol & avatar, CString const & dir) override;
			void										DestroyAvatar(CAvatar * a) override;

			CUnit *										AllocateUnit(CModel * m);
			CUnit *										AllocateUnit(CUol & entity, CString const & type);
			void										Dealloc(CUnit * a);
			CUnit *										FindUnit(CUol & entity) ;
			
			CUnit *										GetUnit(CActive * a);
			CUnit *										FindGroup(CArea * a);

			CUnit *										OpenEntity(CUol & u, CString const & mainSpace, CShowParameters * f) override;
			void										OpenUnit(CUnit * a, CString const & mainSpace, CShowParameters * f);
			void										Show(CUnit * a, CString const & mainSpace, CShowParameters * f) override;
			void										Hide(CUnit * a, CHideParameters * hf) override;
			
			void virtual								StartShowAnimation(CArea * a, CShowParameters * f, CTransformation & from, CTransformation & to);
			void virtual								StartHideAnimation(CArea * a, CHideParameters * f, CTransformation & from, CTransformation & to, std::function<void()> hide);
			
			virtual CList<CUnit *>						CollectHidings(CArea * a, CArea * master){ return {}; }
			//virtual void								OpenAtSameArea(CAllocation * a, CArea * prevArea, CShowParameters * f){}

			void										RunAnimation(CElement * n, CAnimated<CTransformation> a) override;
			void										RunAnimation(CArea * n, CAnimated<CTransformation> a);

			void										Attach(CElement * m, CUol & l) override;
			void										Detach(CElement * m, CUol & l) override;
			bool										IsAttachedTo(CUol & l, CElement * to) override;
			bool										IsAttachedTo(CUol & l, CUol & to) override;

			//void										AttachSpace(CWorldSpace * s, CVisualGraph * vg, CActiveGraph * ag) override;
			//void										DetachSpace(CWorldSpace * s, CVisualGraph * vg, CActiveGraph * ag) override;

			void										Drag(CArray<CDragItem> & d) override;
			void										CancelDragDrop() override;

			void										OnExitRequested();
			void										OnNexusStopping();
			void										OnDiagnosticsUpdating(CDiagnosticUpdate & a);
			//void										OnMoveInput(CActive * r, CActive * s, CMouseArgs *);
			//void										OnToggleInput(CActive * r, CActive * s, CToggleInputArgs &);
			//void										OnStateChanged(CActive *, CActive *, CActiveStateArgs *);
			
			///void										ShowMenu(CCursorEventArgs & a);
			
			virtual CView *								Get(const CString & name) override;
			
			CProtocolConnection<CAvatarProtocol>		FindAvatarSystem(CUol & e, CString const & type) override;

			virtual CElement *							CreateElement(CString const & name, CString const & type) override;

			CPersistentObject *							CreateObject(CString const & name) override;

			virtual CInterObject *						GetEntity(CUol & a) override;
			virtual CList<CUol>							GenerateSupportedAvatars(CUol & e, CString const & type) override;
			virtual CAvatar *							CreateAvatar(CUol & o) override;
	};

	class CWorldClient : public CClient, public virtual IType
	{
		public:
			CWorldServer * Server;

			UOS_RTTI
			CWorldClient(CNexus * nexus, CClientInstance * instance, CWorldServer * server) : CClient(instance)
			{
				Server = server;
			}


			virtual ~CWorldClient()
			{
			}

			IProtocol * Connect(CString const & iface) override
			{
				return Server->Accept(iface);
			}

			void Disconnect(IProtocol * iface) override
			{
				Server->Break(iface);
			}
	};
}
