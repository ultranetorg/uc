#pragma once
#include "ExperimentalLevel.h"

namespace uc
{
	class CGlobe;

	enum class ETileStatus
	{
		Empty, Scheduled, Requested, Canceled, Loaded
	};

	class CTile : public IType
	{
		public:
			CGlobe *									Globe;
			CExperimentalLevel *						Level;
			int											Lod = -1;
			CVisual *									Visual;
			CTile *										Parent = null;
			CTile *										Tiles[4] = {};
			CFloat3										Apexes[4];
			int											T;
			double										R;
			int											S = 32; // segments per Lg and Lt
			CHttpRequest *								Request = null;
			int											TX;
			int											TY;
			int											PTX;
			int											PTY;
			CMesh *										Mesh = null;
			CMaterial *									Material = null;
			double										ShowTime = NAN;
			CMaterial *									Notloaded;

			double										Ltb;
			double										Lte;
			double										Ltd;
			double										Lgb;
			double										Lge;
			double										Lgd;

			int											N;
			const double								E = atan(sinh(M_PI));
			const double								O = M_PI/2 - E; 

			ETileStatus									Status = ETileStatus::Empty;
			CList<CString> *							Report;

			CArray<CFloat3>								Points;
			CArray<int>									Ix;
			CArray<CFloat2>								UV;

			//CEvent<>									Loaded;

			UOS_RTTI
			CTile(CExperimentalLevel * l, CGlobe * g, CTile * parent, CMaterial * nl, double r, int lod, int ptx, int pty, CList<CString> * report);
			~CTile();

			CFloat3										Sphere(double lt, double lg);
			int											Build(CCamera * c, int lod, CList<CMatrix> & wms);
			void										CancelRequest();
			void										SetOwnUV();
			void										RequestTile();
	};
}