#pragma once
#include "CefClient.h"
#include "CefApp.h"

using namespace std;

namespace uc
{
	class CCefElement : public CRectangle
	{
		public:
			CExperimentalLevel *						Level;
			//CSolidRectangleMesh *						Mesh;
			CefRefPtr<CCefClient>								Cef;
			CTexture *									Texture;
			bool										Scrolling = false;

			CFloat2										CapturePoint;

			static int									Count;
			static CefRefPtr<CCef>					App;
			
			CCefElement(CExperimentalLevel * l, CStyle * s, CString const & name = GetClassName());
			~CCefElement();

			void										Navigate(CString const & url);
			void										Back();
			void										Forward();
			void										Reload();
			void										ExecuteJavascript(CString const & js);
			virtual void								UpdateLayout(CLimits const & l, bool apply) override;

			void										OnStateModified(CActive *, CActive *, CActiveStateArgs * a);
			void										OnKeyboard(CActive * r, CActive * s, CKeyboardArgs * a);
			void										OnMouse(CActive * r, CActive * s, CMouseArgs * a);
			void										OnTouch(CActive * r, CActive * s, CTouchArgs * arg);

			class QuitTask : public CefTask
			{
				public:
				QuitTask() { }
				void Execute() override
				{
					CefQuitMessageLoop();
				}
				IMPLEMENT_REFCOUNTING(QuitTask);
			};

	};
}