namespace IdentityProvider.Services
{
    using System;
    using System.IdentityModel.Tokens;

    using IdentityInfrastructure;
    using IdentityInfrastructure.Constants;

    public class HmacSigningCredentialsService : SigningCredentials
    {
        public HmacSigningCredentialsService(string base64EncodedKey)
            : this(Convert.FromBase64String(base64EncodedKey))
        {
        }

        public HmacSigningCredentialsService(byte[] key)
            : base(new InMemorySymmetricSecurityKey(key),
                  CreateSignatureAlgorithm(key),
                  CreateDigestAlgorithm(key))
        {
        }

        protected static string CreateSignatureAlgorithm(byte[] key)
        {
            switch (key.Length)
            {
                case 32:
                    return IdentityConstants.HmacSha256Signature;
                case 48:
                    return IdentityConstants.HmacSha384Signature;
                case 64:
                    return IdentityConstants.HmacSha512Signature;
                default:
                    throw new InvalidOperationException("Unsupported key length");
            }
        }

        protected static string CreateDigestAlgorithm(byte[] key)
        {
            switch (key.Length)
            {
                case 32:
                    return IdentityConstants.Sha256Digest;
                case 48:
                    return IdentityConstants.Sha384Digest;
                case 64:
                    return IdentityConstants.Sha512Digest;
                default:
                    throw new InvalidOperationException("Unsupported key length");
            }
        }
    }
}