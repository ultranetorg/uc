#pragma once

namespace uc
{
	struct CGetIconMaterialJob
	{
		IType *									Requester = null;
		CString									Path;
		std::function<void(CMaterial *)>		Done;
		std::function<void()>					ImageReady;
		std::function<void()>					IconReady;
		CThread *								Thread = null;
		CImage *								Image = null;
		CMaterial *								Material = null;
		SHFILEINFOW								Sfi = {0};
	};

	class CImageExtractorProtocol : public virtual IProtocol
	{
		public:
			inline static const CString				InterfaceName = L"ImageExtractor1";

			//virtual CImage *						GetIcon(CUol & u, int wh)=0;
			//virtual CTexture *					GetIconTexture(CUol & u, int wh)=0;
			virtual CGetIconMaterialJob *			GetIconMaterial(IType * r, CUrl & u, int wh)=0;

			virtual ~CImageExtractorProtocol(){}
	};
}