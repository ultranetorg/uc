#pragma once
#include "ProcessDlg.h"

namespace uos
{
	struct CVertexKey
	{
		CFloat3		Position;
		CFloat3		Normal;
		CFloat2		UV;
		int				VColor;

		bool operator < (const CVertexKey& r) const
		{   
			if(Position.x != r.Position.x)				// Vertex
				return Position.x < r.Position.x;
			else
				if(Position.y != r.Position.y) 
					return Position.y < r.Position.y;
				else
					if(Position.z != r.Position.z)
						return Position.z < r.Position.z;
					else

						if(Normal.x != r.Normal.x)			// Normal
							return Normal.x < r.Normal.x;
						else
							if(Normal.y != r.Normal.y) 
								return Normal.y < r.Normal.y;
							else
								if(Normal.z != r.Normal.z)
									return Normal.z < r.Normal.z;
								else

									if(UV.x != r.UV.x)			// UV
										return UV.x < r.UV.x;
									else
										if(UV.y != r.UV.y) 
											return UV.y < r.UV.y;
										else

											if(VColor != r.VColor)			// VColor
												return VColor < r.VColor;
											else
												return false;
		}

	};
}

/*
namespace std
{
	template <> struct hash<uos::CVertexKey>
	{
		size_t operator()(uos::CVertexKey const & x) const noexcept
		{
			return ((51 + std::hash<float>()(x.Position.x)) * 51 + std::hash<float>()(x.Position.y));
		}
	};
}*/

namespace uos
{
	typedef std::map<CVertexKey, int>	VertexMap;

	class CVwmMesh
	{
		public:
			CString										FileName;
			CArray<CFloat3>								Positions;
			CArray<CFloat3>								Normals;
			CArray<CFloat2>								UVs;
			CArray<int>									VColors;
			CArray<int>									Indexes;

			VertexMap									Hashmap;
			
			bool										HasNormals;
			bool										HasTVertexes;
			bool										HasCVertexes;

			CProcessDlg	*								UI;
			
			Object *									MaxObject;
			TriObject *									MaxTriObj;
			Mesh *										MaxMesh;
			bool										IsSyntesized;
			CAABB										BBox;
			
			Matrix3										OffsetMatrix;
			Matrix3										OffsetMatrixForNormals;
			
			bool										Failed;
	
			void										Export(IDirectory * w);
			bool										AskIsValid();

			CVwmMesh(INode * node, TriObject * to, CProcessDlg * ui, int id, CSource * s);
			CVwmMesh(INode * node, TriObject * to, int mtlId, int subMtlN, CProcessDlg * ui, int id, CSource * s);
			~CVwmMesh();

		private:
			bool										IsMatrixFlipped;
			MeshNormalSpec*								SpecifiedNormals;

			void										ConstructCommon(INode * node, TriObject * to, CProcessDlg * ui, CSource * s);

			int											VertColorToDWORD(VertColor & vc);
			void										AddFace(int i);
			Point3										GetVertexNormal(int fi, int crni);
	};
}
