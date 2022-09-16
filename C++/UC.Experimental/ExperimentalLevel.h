#pragma once

namespace uc
{
	class CBitfinexProvider;
	class CTradingviewProvider;
	class CCef;

	struct CExperimentalLevel
	{
		CProtocolConnection<CWorldProtocol>				World;
		CProtocolConnection<CShellProtocol>				Shell;
		CProtocolConnection<CFileSystemProtocol>		Storage;
		CProtocolConnection<CImageExtractorProtocol>	ImageExtractor;

		CCore *											Core;
		CNexus *										Nexus;
		CEngine *										Engine;
		CPersistentServer *								Server;
		CStyle *										Style;
		CLog *											Log;
		CBitfinexProvider *								Bitfinex = null;
		CTradingviewProvider *							Tradingview = null;

		CefRefPtr<CCef>									Cef;
		std::atomic_int									CefCounter = 0;
		CThread *										CefThread = null;

		std::condition_variable signal_;
		std::atomic_bool		ready_;
		std::mutex				lock_;

		void										RequireCef();
	};
}
