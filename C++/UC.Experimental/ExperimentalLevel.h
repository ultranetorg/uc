#pragma once

namespace uc
{
	class CBitfinexProvider;
	class CTradingviewProvider;
	class CCef;

	struct CExperimentalLevel
	{
		CConnection<CWorldProtocol>				World;
		CConnection<CShellProtocol>				Shell;
		CConnection<CFileSystemProtocol>		Storage;
		CConnection<CImageExtractorProtocol>	ImageExtractor;

		CCore *									Core;
		CNexus *								Nexus;
		CEngine *								Engine;
		CPersistentServer *						Server;
		CStyle *								Style;
		CLog *									Log;
		CBitfinexProvider *						Bitfinex = null;
		CTradingviewProvider *					Tradingview = null;

		CefRefPtr<CCef>							Cef;
		std::atomic_int							CefCounter = 0;
		CThread *								CefThread = null;

		std::condition_variable					signal_;
		std::atomic_bool						ready_;
		std::mutex								lock_;

		void									RequireCef();
	};
}
