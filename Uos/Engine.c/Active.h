#pragma once
#include "Mesh.h"
#include "Geometry.h"
#include "Camera.h"
#include "ScreenEngine.h"
#include "Mouse.h"
#include "Keyboard.h"
#include "TouchScreen.h"
#include "ActiveEvent.h"

namespace uc
{
	class CActiveSpace;
	class CActiveGraph;
	class CEventNode;
	class CActive;

	enum class EActiveState
	{
		Null=0, Normal=1, Active=2
	};

	static wchar_t * ToString(EActiveState e)
	{
		static wchar_t * name[] = {L"", L"Normal", L"Active"};
		return name[int(e)];
	}

	class UOS_ENGINE_LINKING CPick : public CMeshIntersection
	{
		public:
			CActiveSpace *	Space		= null;
			CActive *		Active		= null;
			CMesh *			Mesh		= null;
			CCamera *		Camera		= null;
			CFloat2			Vpp			= {NAN, NAN};
			CFloat2			ScreenPoint	= {NAN, NAN};
			CRay			Ray			= CRay(CFloat3(NAN), CFloat3(NAN));
			float			Z			= FLT_MAX;

			CPick(){}
			CPick(const CPick & a);
			CPick(CPick && a);
			~CPick();

			CFloat3			GetFinalPosition();
			CFloat3			GetWorldPosition();
			CPick &			operator = (const CPick & a);
			bool			operator != (const CPick & a);
			bool			operator == (const CPick & a);
	};
	
	class CNodeCapture : public CShared
	{
		public:
			CPick 			Pick;
			CInputMessage *	Message;
			int				Status = 0;
			
			CNodeCapture(CInputMessage * m)
			{
				Message = m;
				m->Take();
			}

			~CNodeCapture()
			{
				sh_free(Message);
			}

			template<class T> T * InputAs()
			{
				return Message->As<T>();
			}
	};	

	enum class EGraphEvent
	{
		Null, 
		Input,
		Enter, Leave, Captured, Released, GuestHover, Hover, Roaming, 
		State,
		Feedback,
		Click
	};

	static wchar_t * ToString(EGraphEvent e)
	{
		static wchar_t * name[] = {	L"",
									L"Input",
									L"Enter", L"Leave", L"Captured", L"Released", L"GuestHover", L"Hover", L"Roaming", 
									L"State",	
									L"Feedback",
									L"Click"};
		return name[int(e)];
	}

	struct CActiveArgs : CShared, virtual public IType
	{
		CActiveArgs *	Parent = null;
		bool			StopPropagation = false;
		bool			StopRelatedPropagation = false;

		UOS_RTTI
		CActiveArgs(){}
		virtual ~CActiveArgs(){}

		virtual	CString ToString(){ return L""; }
	}; 

	class CActiveStateArgs : public CActiveArgs
	{
		public:
			EActiveState	Old;
			EActiveState	New;
	};

	struct CInputArgs : public CActiveArgs
	{
	}; 

	class CMouseArgs : public CInputArgs
	{
		public:
			CPick				Pick;
			CNodeCapture *		Capture = null;
			EGraphEvent			Event = EGraphEvent::Null;
			
			CMouse *		Device;
			CScreen *		Screen;
			CFloat2			Position;
			CFloat2			PositionDelta;
			CFloat2			RotationDelta;
			EMouseAction	Action;
			EMouseControl	Control;

			CMouseArgs(CInputMessage * m)
			{
				auto mi = m->As<CMouseInput>();

				Device			= mi->Device;
				Screen			= mi->Screen;
				Position		= mi->Position;
				PositionDelta	= mi->PositionDelta;
				RotationDelta	= mi->RotationDelta;
				Action			= mi->Action;
				Control			= mi->Control;
			}
			
			~CMouseArgs()
			{
				sh_free(Capture);
			}

			CString ToString() override
			{
				return uc::ToString(Event);
			}
	};

	class CKeyboardArgs : public CInputArgs
	{
		public:
			CKeyboard *			Device = null;
			int64_t				WM = 0;
			int64_t				Flags = 0;
			EKeyboardAction		Action;
			EKeyboardControl	Control;

			CKeyboardArgs(CInputMessage * m)
			{
				auto ki = m->As<CKeyboardInput>();

				Device	= ki->Device;
				WM		= ki->WM;
				Flags	= ki->Flags;
				Action	= ki->Action;
				Control	= ki->Control;
			}
	};
	
	class CTouchArgs : public CInputArgs
	{
		public:
			EGraphEvent				Event = EGraphEvent::Null;
			CNodeCapture *			Capture = null;
			CTouchInput	*			Input;
			CMap<int, CPick>		Picks;

			CTouchArgs(CInputMessage * m)
			{
				Input = m->As<CTouchInput>();
				Input->Take();
			}

			~CTouchArgs()
			{
				sh_free(Input);
				sh_free(Capture);
			}

			CTouch * GetPimaryTouch()
			{
				return &Input->Touches.Find([](auto & i){ return i.Primary; });
			}

			CPick & GetPick()
			{
				return Picks(Input->Touch.Id);
			}

			CString ToString() override 
			{
				return uc::ToString(Event);
			}
	};

	class CManualArgs : public CInputArgs
	{
		public:
			CManualArgs()
			{
			}
	};


	class UOS_ENGINE_LINKING CActive : public CEngineEntity, public CShared
	{	
		public:
			CString												Name;
			bool												Enabled = true;
			ETransformating										Transformating = ETransformating::Inherit;
			CMatrix												Matrix;
			CMatrix												FinalMatrix = CMatrix::Nan;
			CMesh *												Mesh = null;
			CRefList<CActive *>									Nodes;
			CActive *											Parent = null;
			CActive *											HoverChild = null;
			EActiveState										State = EActiveState::Normal;
			bool												IsPropagator=true;
			IType *												Owner = null;
			CActiveGraph *										Graph = null;
			int													Index = -1;
			bool												ActivatePassOn = false;
			EClipping											Clipping = EClipping::Inherit;
			CMesh *												ClippingMesh = null;
			CList<CActiveSpace *>								Spaces;

			CEvent<CActive *, CActive *, CActiveStateArgs *>	StateChanged;
			CActiveEvent<CActive *, CActive *, CMouseArgs *>	MouseEvent;
			CActiveEvent<CActive *, CActive *, CKeyboardArgs *>	KeyboardEvent;
			CActiveEvent<CActive *, CActive *, CTouchArgs *>	TouchEvent;


			#ifdef _DEBUG
			CTransformation										_Decomposed;
			CString												FullName;
			#endif

			UOS_RTTI
			CActive(CEngineLevel * l, const CString & name);
			virtual ~CActive();

			void												SetName(const CString & name);
			void												SetMesh(CMesh * m);
			void												SetMatrix(CMatrix const & m);
			CMatrix &											GetMatrix();
			EActiveState										GetState();
			CMesh *												GetMesh();
			bool												IsReady();
			void												Enable(bool e);
			void												TransformMatrix(CMatrix &  m);

			CString												GetStatus();

			void												Attach();
			void												Detach();
			
			void												AddNode(CActive * c);
			void												RemoveNode(CActive * c);
			CActive *											GetParent();
			CActive *											GetAncestor(const CString & name);
			CActive *											GetRoot();
			bool												HasAncestor(CActive * n);
			CActive *											GetElderEnabled();
			CActive *											FindFirstDisabledAncestor();
			bool												IsFinallyEnabled();
			CActive *											FindCommonAncestor(CActive * n);
			CActive *											FindChildContaining(CActive * a);

			CAABB												GetFinalBBox2D();
			CAABB												GetClippingBBox2D();

			CFloat3												Transit(CActive * owner, CFloat3 & p);
			CMeshIntersection									GetIntersection(CRay & r);
			CFloat3												GetXyPlaneIntersectionPoint(CMatrix & sm, CCamera * vp, CFloat2 & vpp);
		
			void												SetClipping(CMesh * mesh);
			CMesh *												GetClipping();
			CActive *											GetActualClipper();
			EClipping											GetActualClipping();

			void												SetMeta(const CString & k, const void * v);
			void												SetMeta(const CString & k, const CString & v);
			template<class T> void								SetMeta(T * v)
																{
																	auto s = dynamic_cast<CShared *>(v);
																
																	if(s)
																	{
																		s->Take();
																		Metas.push_back(CMetaItem(T::GetClassName(), s));
																	}
																	else
																		Metas.push_back(CMetaItem(T::GetClassName(), static_cast<void *>(s)));
																}

			CString &											GetMetaString(const CString & k);
			void *												GetMetaPointer(const CString & k);
			template<class T> T *								GetMetaPointer()
																{	
																	auto & m = Metas.Find([](auto & i){ return i.Key == T::GetClassName(); });
																	return m.Pointer ? static_cast<T *>(m.Pointer) : dynamic_cast<T *>(m.Shared); 
																}

			template<class T> bool								HasMetaPointer()
																{ 
																	return Metas.Has([](auto & i){ return i.Key == T::GetClassName(); }); 
																}

			bool												HasMeta(const CString & key)
																{ 
																	return Metas.Has([&key](auto & i){ return i.Key == key; }); 
																}

			template<class T> T *								AncestorOwnerOf()
																{
																	auto p = this;
																	while(p && !dynamic_cast<T *>(p->Owner))
																	{
																		p = p->Parent;
																	}
																	return p ? dynamic_cast<T *>(p->Owner) : null;
																}

			template<class T> T *								HighestOwnerOf()
																{
																	auto p = this;
																	T * a = null; 

																	while(p)
																	{
																		auto o = p->GetOwnerAs<T>();
																		if(o)
																		{
																			a = o;
																		}

																		p = p->Parent;
																	}
																	return a;
																}

			template<class T> T *								GetOwnerAs() { return dynamic_cast<T *>(Owner); }

		private:
			CList<CMetaItem>									Metas;
	};
}