#pragma once

namespace uc
{
	class INativeMessageHandler
	{
		public:
			bool virtual		ProcessMessage(MSG * m) = 0;
	};
}