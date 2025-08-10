using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using System;

namespace JRadius.Core.Client.Auth
{
    public abstract class RadiusAuthenticator : IRadiusAuthenticator
    {
        protected RadiusClient _client;
        protected RadiusAttribute _username;
        protected RadiusAttribute _password;
        protected RadiusAttribute _classAttribute;
        protected RadiusAttribute _stateAttribute;

        public abstract string GetAuthName();

        public virtual void SetupRequest(RadiusClient client, RadiusPacket p)
        {
            _client = client;

            if (_username == null)
            {
                var a = p.FindAttribute(1); // User-Name
                if (a == null)
                {
                    throw new Exception("You must at least have a User-Name attribute in a Access-Request");
                }
                // TODO: Implement pooling
                _username = a;
            }

            if (_password == null)
            {
                var a = p.FindAttribute(2); // User-Password
                if (a != null)
                {
                    // TODO: Implement pooling
                    _password = a;
                }
            }
        }

        public abstract void ProcessRequest(RadiusPacket p);

        public virtual void ProcessChallenge(RadiusPacket request, RadiusPacket challenge)
        {
            _classAttribute = challenge.FindAttribute(25); // Class
            if (_classAttribute != null)
            {
                // TODO: Implement pooling
                request.OverwriteAttribute(_classAttribute);
            }

            _stateAttribute = challenge.FindAttribute(24); // State
            if (_stateAttribute != null)
            {
                // TODO: Implement pooling
                request.OverwriteAttribute(_stateAttribute);
            }
        }

        public RadiusClient GetClient()
        {
            return _client;
        }

        public void SetClient(RadiusClient client)
        {
            _client = client;
        }

        protected byte[] GetUsername()
        {
            return _username?.GetValue().GetBytes();
        }

        protected byte[] GetPassword()
        {
            if (_password != null)
            {
                return _password.GetValue().GetBytes();
            }
            return new byte[0];
        }

        public void SetUsername(RadiusAttribute userName)
        {
            _username = userName;
        }

        public void SetPassword(RadiusAttribute cleartextPassword)
        {
            _password = cleartextPassword;
        }

        protected byte[] GetClassAttribute()
        {
            return _classAttribute?.GetValue().GetBytes();
        }

        protected byte[] GetStateAttribute()
        {
            return _stateAttribute?.GetValue().GetBytes();
        }
    }
}
