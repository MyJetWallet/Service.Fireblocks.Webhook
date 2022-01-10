using Org.BouncyCastle.Asn1;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Service.Fireblocks.Webhook.Services
{
    public static class CryptoProvider
    {
        private static readonly Lazy<byte[]> _fireblocksPubKey;

        static CryptoProvider()
        {
            _fireblocksPubKey = new Lazy<byte[]>(() =>
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webhook_sig.pub");
                var fileContent = File.ReadAllText(path);
                var base64 = fileContent.Replace("-----BEGIN PUBLIC KEY-----\n", "").Replace("-----END PUBLIC KEY-----\n", "");
                return Convert.FromBase64String(base64);
            }, false);
        }

        internal static void Init()
        {
            Console.WriteLine($"Fireblocks PubKey: {Convert.ToBase64String(_fireblocksPubKey.Value)}");
        }

        public static bool VerifySignature(byte[] data, byte[] signature)
        {
            byte[] hash;
            using (SHA512 sha256 = SHA512.Create())
            {
                hash = sha256.ComputeHash(data);
            }

            bool b = false;
            var rsaParam = GetPublicKeyRSAParameters(_fireblocksPubKey.Value);

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParam);
                var signatureDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                signatureDeformatter.SetHashAlgorithm("SHA512");

                b = signatureDeformatter.VerifySignature(hash, signature);
            }

            return b;
        }

        public static RSAParameters GetPublicKeyRSAParameters(byte[] subjectPublicKeyInfoBytes)
        {
            var publicKeyObject = (DerSequence)Asn1Object.FromByteArray(subjectPublicKeyInfoBytes);
            var rsaPublicKeyParametersBitString = (DerBitString)publicKeyObject[1];

            var rsaPublicKeyParametersObject = (DerSequence)Asn1Object.FromByteArray(rsaPublicKeyParametersBitString.GetBytes());

            var modulus = ((DerInteger)rsaPublicKeyParametersObject[0]).Value.ToByteArray().Skip(1).ToArray();
            var exponent = ((DerInteger)rsaPublicKeyParametersObject[1]).Value.ToByteArray();

            return new RSAParameters() { Modulus = modulus, Exponent = exponent };
        }
    }
}
