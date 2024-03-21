#include "VwmMesh.h"
#include "MaterialFactory.h"
#include "MeshFactory.h"
#include "MainDlg.h"
#include "ProcessDlg.h"
#include "IProcessHandler.h"
#include "VwmNode.h"

#define MW_VWMEXPORTER_PRODUCT_NAME						L"Mightywill 3ds Max Vwm Exporter"

void DoExport(TCHAR * name, ExpInterface * ei, Interface * i, BOOL suppressPromts, DWORD options);

namespace uos
{
/*	struct MtlMeshKey
	{
		Mtl		*	MaxMtl;
		bool		HasNormals;
		bool		HasTVertexes;
		bool		HasCVertexes;
	};

	struct MtlMeshKeyHashCompare : stdext::hash_compare<MtlMeshKey>
	{
		size_t operator()(const MtlMeshKey& k) const
		{
			return (size_t)k.MaxMtl;
		}

		bool operator()(const MtlMeshKey& l, const MtlMeshKey& r) const
		{   
			if(l.MaxMtl != r.MaxMtl)				// Vertex
				return l.MaxMtl < r.MaxMtl;
			else
				if(l.HasNormals != r.HasNormals)			// Normal
					return l.HasNormals != r.HasNormals;
				else
					if(l.HasTVertexes != r.HasTVertexes) 
						return l.HasTVertexes != r.HasTVertexes;
					else
						if(l.HasCVertexes != r.HasCVertexes)
							return l.HasCVertexes != r.HasCVertexes;
						else
							return false;
		}
	};
*/

	typedef std::set<INode *>				MaxNodeSet;


	class CVwmExporter: public AssetEnumCallback, public IProcessHandler
	{
		public:
//			int											callback(INode *node);
			void										RecordAsset(const MaxSDK::AssetManagement::AssetUser& asset);
			void										Start();
			void										OnProcessInit();

			CVwmExporter(CSource * s);
			~CVwmExporter();

		private:
//			MaxNodeSet									MaxNodes;
			CVwmNodeSet									Roots;
			
			CMaterialFactory *							MaterialFactory;
			CMeshFactory *								MeshFactory;
			CSource *									Source;
			CProcessDlg									UI;
			IProcessHandler *							Handler;
		
			TimeValue									Time;
			CZipDirectory *								Zip;
			CWritePool *								WritePool;

			void										Clear();
			void										OnStartExport();
			void										OnCancelExport();

			void										Enumerate();
			void										Enumerate(CVwmNode * parent, INode * node);
			void										Export();

	};
}
