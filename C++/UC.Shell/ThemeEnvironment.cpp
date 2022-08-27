#include "StdAfx.h"
#include "ThemeEnvironment.h"

using namespace uc;

CThemeEnvironment::CThemeEnvironment(CShellLevel * l, CString const & name) : CThemeAvatar(l, name)
{
	//Level->World->ModelOpened += ThisHandler(OnModelOpened);
	//Active->MoveInput += ThisHandler(OnMoveInput);
	Tags = {L"all"};
}
	
CThemeEnvironment::~CThemeEnvironment()
{
	//Level->World->ModelOpened -= ThisHandler(OnModelOpened);
}

void CThemeEnvironment::SaveInstance()
{
	__super::SaveInstance();

	CTonDocument d;

	for(auto i : Models)
	{
		d.Add(i.first)->Set(i.second);
	}

	SaveGlobal(d, GetClassName() + L".xon");
}

void CThemeEnvironment::LoadInstance()
{
	__super::LoadInstance();

	CTonDocument d; LoadGlobal(d, GetClassName() + L".xon");

	for(auto i : d.Many(L"Models"))
	{
///		Models[i->Name] = i->As<CFloat3>();
	}

	///if(Models.empty())
	///{
	///	Models[SHELL_FIELD_A] = CFloat3(0, 0, 0);
	///	Models[SHELL_FIELD_B] = CFloat3(0.2f, 0, 0);
	///	Models[SHELL_FIELD_C] = CFloat3(-0.2f, 0, 0);
	///}
	///
	///auto a = Level->World->Allocations.Find([this](auto a){ return a->IsOpen() && Level->World->BackSpace->ContainsDescedant(a->Space) && a->Parents.empty(); });
	///
	///if(a && Models.Contains(a->Model->Name))
	///{
	///	Level->World->MainThemeView->Cameras.First()->SetRotation(Models[a->Model->Name]);
	///}
}

void CThemeEnvironment::OnMoveInput(CActive * r, CActive * s, CMouseArgs * a)
{
	///if((a.Type == EMouseEventType::Hover || a.Type == EMouseEventType::HoverOutside) && a.Capture.Pick.Node == Active && a.Capture.Message.Sender == EInputSender::LeftButton)
	///{
	///	Level->World->ThemeView->Cameras.First()->Rotate(CQuaternion::FromEuler(CFloat3(a.Movement.Delta.x * -0.001f, a.Movement.Delta.y * 0.001f, 0)));
	///
	///	auto a = Level->World->Allocations.Find([this](auto a)
	///											{ 
	///												return a->IsOpen() && Level->World->FindArea(CArea::Fields)->ContainsDescedant(a->Area) && a->Parents.empty(); 
	///											});
	///
	///	if(a)
	///	{
	///		Models[a->Model->Name] = Level->World->ThemeView->Cameras.First()->Rotation;
	///	
	///	}
	///}
}

void CThemeEnvironment::OnModelOpened(CShowParameters *, CModel * a)
{
	///if(Models.Contains(a->Model->Name))
	///{
	///	auto c = Level->World->ThemeView->Cameras.First();
	///
	///	CAnimated<CFloat3> an(c->Rotation, Models[a->Model->Name], CAnimation(1.0f));
	///
	///	Level->Engine->AddJob(	[an, c]() mutable 
	///							{
	///								if(an.Animation.Animating)
	///								{
	///									c->SetRotation(an.GetNext());
	///								}
	///
	///								return an.Animation.Animating;
	///							});
	///}
}

void CThemeEnvironment::OnCameraMoved(CCamera * c)
{

}
