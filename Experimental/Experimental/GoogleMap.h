#pragma once

namespace uc
{
	class CGoogleMap
	{
		public:
			const int TILE_SIZE = 256;

			CFloat2 ToPixelCoordinates(float lat, float lng, int zoom)
			{
				auto scale = 1 << zoom;

				auto worldCoordinate = Project(lat, lng);

				return CFloat2(	floor(worldCoordinate.x * scale),
								floor(worldCoordinate.y * scale));
			}

			CFloat2 ToTileCoordinates(float lat, float lng, int zoom)
			{
				auto scale = 1 << zoom;

				auto worldCoordinate = Project(lat, lng);

				return CFloat2(	floor(worldCoordinate.x * scale / TILE_SIZE),
								floor(worldCoordinate.y * scale / TILE_SIZE));
			}

			// The mapping between latitude, longitude and pixels is defined by the web
			// mercator projection.
			CFloat2 Project(float lat, float lng)
			{
				float siny = sin(lat * float(M_PI) / 180.f);

				// Truncating to 0.9999 effectively limits latitude to 89.189. This is
				// about a third of a tile past the edge of the world tile.
				siny = min(max(siny, -0.9999f), 0.9999f);

				return CFloat2(	TILE_SIZE * (0.5f + lng / 360.f),
								TILE_SIZE * (0.5f - log((1.f + siny) / (1.f - siny)) / (4.f * float(M_PI))));
			}

		
			CGoogleMap();
			~CGoogleMap();
	};
}
