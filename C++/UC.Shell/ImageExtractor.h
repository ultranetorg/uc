#pragma once

namespace uc
{
	class CLnk
	{
		public:
			CString		Name;
			CString		Target;
			CString		IconSource;
			int			IconIndex;
	};

	class CImageExtractor : public IType, public IImageExtractor, public IIdleWorker
	{
		public:
			CMesh *										DefaultMesh;
			CMaterial *									DefaultMaterial;
			CWorldLevel *								Level;

			CTexture *									Notfound24;
			CTexture *									Notfound48;

			CList<CGetIconMaterialJob *>				Ready;
			CList<CGetIconMaterialJob *>				Fetching;
			std::mutex									Lock;
			std::mutex									FetcherMutex;
			std::condition_variable						FetcherWakeuper;
			CThread *									Thread;

			CDiagnostic *								Diagnostics = null;

			UOS_RTTI
			CImageExtractor(CWorldLevel * l, CServer * s);
			~CImageExtractor();

			void										OnCoreExitQueried();
			CTexture *									GetNotfound(int wh);
			void										FetchIcon(CUrl & f, int wh, CGetIconMaterialJob * job);
			//CTexture *									GetIconTexture(CUol & f, int xy) override;
			CGetIconMaterialJob *						GetIconMaterial(IType * r, CUrl & f, int xy) override;
			CTexture *									GetIconTexture(CImage * i);
			CSolidRectangleMesh *						GetMesh(float w, float h);

			CLnk										Resolve(CString const & lp);

			void										OnDiagnosticsUpdate(CDiagnosticUpdate & a);
			virtual void								DoIdle() override;
			
			bool										IsRequested(IType *);

		private:
			CMap<int, CMap<int, CImage *>>				Images;
			CMap<int, CMap<CString, CImage *>>			Types;
			CRefList<CSolidRectangleMesh *>				Meshes;
			CMap<CImage *, CTexture *>					Textures;
			CMap<CImage *, CMaterial *>					Materials;
			CMap<CImage *, CBitmap *>					Bitmaps;
			//IImageList *								ImageList48;
	};
}
