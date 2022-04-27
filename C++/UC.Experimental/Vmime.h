#pragma once


namespace uc
{
	// SASL authentication handler
	class CInteractiveAuthenticator : public vmime::security::sasl::defaultSASLAuthenticator
	{
		public:
			mutable vmime::string m_username;
			mutable vmime::string m_password;

			CInteractiveAuthenticator(CString const & u, CString const & p)
			{
				m_username = u.ToAnsi();
				m_password = p.ToAnsi();
			}

			const std::vector <vmime::shared_ptr <vmime::security::sasl::SASLMechanism> >getAcceptableMechanisms(	const std::vector <vmime::shared_ptr <vmime::security::sasl::SASLMechanism> >& available,
																													const vmime::shared_ptr <vmime::security::sasl::SASLMechanism>& suggested) const 
			{

				///std::cout << std::endl << "Available SASL mechanisms:" << std::endl;
				///
				///for(unsigned int i = 0; i < available.size(); ++i) {
				///
				///	std::cout << "  " << available[i]->getName();
				///
				///	if(suggested && available[i]->getName() == suggested->getName()) {
				///		std::cout << "(suggested)";
				///	}
				///}
				///
				///std::cout << std::endl << std::endl;

				return defaultSASLAuthenticator::getAcceptableMechanisms(available, suggested);
			}

			void setSASLMechanism(const vmime::shared_ptr <vmime::security::sasl::SASLMechanism>& mech)
			{
				///std::cout << "Trying '" << mech->getName() << "' authentication mechanism" << std::endl;
				defaultSASLAuthenticator::setSASLMechanism(mech);
			}

			const vmime::string getUsername() const
			{
				return m_username;
			}

			const vmime::string getPassword() const
			{
				return m_password;
			}
	};

	class timeoutHandler : public vmime::net::timeoutHandler
	{
		public:
			timeoutHandler(): m_start(time(NULL))
			{
			}

			bool isTimeOut()
			{
				// This is a cancellation point: return true if you want to cancel
				// the current operation. If you return true, handleTimeOut() will
				// be called just after this, and before actually cancelling the
				// operation

				// 10 seconds timeout
				return (time(NULL) - m_start) >= 10;  // seconds
			}

			void resetTimeOut()
			{
				// Called at the beginning of an operation (eg. connecting,
				// a read() or a write() on a socket...)
				m_start = time(NULL);
			}

			bool handleTimeOut()
			{
				// If isTimeOut() returned true, this function will be called. This
				// allows you to interact with the user, ie. display a prompt to
				// know whether he wants to cancel the operation.

				// If you return false here, the operation will be actually cancelled.
				// If true, the time out is reset and the operation continues.
				return false;
			}

		private:
			time_t m_start;
	};


	class CTimeoutHandlerFactory : public vmime::net::timeoutHandlerFactory
	{
		public:
			vmime::shared_ptr <vmime::net::timeoutHandler> create()
			{
				return vmime::make_shared <timeoutHandler>();
			}
	};

	class CCertificateVerifier : public vmime::security::cert::defaultCertificateVerifier
	{
		public:
			void verify(const vmime::shared_ptr <vmime::security::cert::certificateChain>& chain, const vmime::string& hostname)
			{
				try
				{
					setX509TrustedCerts(m_trustedCerts);
					defaultCertificateVerifier::verify(chain, hostname);
				}
				catch (vmime::security::cert::certificateException&)
				{
					// Obtain subject's certificate
					vmime::shared_ptr <vmime::security::cert::certificate> cert = chain->getAt(0);

					// Accept it, and remember user's choice for later
					if (cert->getType() == "X.509")
					{
						m_trustedCerts.push_back(vmime::dynamicCast <vmime::security::cert::X509Certificate>(cert));
						setX509TrustedCerts(m_trustedCerts);
						///defaultCertificateVerifier::verify(chain, hostname);
					}
				}
			}

		private:
			std::vector<vmime::shared_ptr<vmime::security::cert::X509Certificate>> m_trustedCerts;
	};


	class myTracer : public vmime::net::tracer
	{
		public:
			myTracer(const vmime::shared_ptr <std::ostringstream>& stream, const vmime::shared_ptr <vmime::net::service>& serv, const int connectionId)
				: m_stream(stream),
				m_service(serv),
				m_connectionId(connectionId)
			{
			}

			void traceSend(const vmime::string& line)
			{
				*m_stream << "[" << m_service->getProtocolName() << ":" << m_connectionId
					<< "] C: " << line << std::endl;
			}

			void traceReceive(const vmime::string& line)
			{
				*m_stream << "[" << m_service->getProtocolName() << ":" << m_connectionId
					<< "] S: " << line << std::endl;
			}

		private:
			vmime::shared_ptr <std::ostringstream> m_stream;
			vmime::shared_ptr <vmime::net::service> m_service;
			const int m_connectionId;
	};


	class CTracerFactory : public vmime::net::tracerFactory
	{
		public:
			CTracerFactory(const vmime::shared_ptr <std::ostringstream>& stream) : m_stream(stream)
			{
			}

			vmime::shared_ptr <vmime::net::tracer> create(const vmime::shared_ptr <vmime::net::service>& serv, const int connectionId)
			{
				return vmime::make_shared <myTracer>(m_stream, serv, connectionId);
			}

		private:
			vmime::shared_ptr <std::ostringstream> m_stream;
	};

}
