using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Service.Fireblocks.Webhook.Services
{
    public static class CryptoProvider
    {
        private static readonly Lazy<string> _fireblocksPubKey;

        static CryptoProvider()
        {
            _fireblocksPubKey = new Lazy<string>(() =>
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webhook_sig.pub");
                var fileContent = File.ReadAllText(path);
                var base64 = MyRsa.ReadPublicKeyFromPem(fileContent);
                return base64;
            }, false);
        }

        internal static void Init()
        {
            Console.WriteLine($"Fireblocks PubKey: {_fireblocksPubKey.Value}");
        }

        public static bool VerifySignature(byte[] data, byte[] signature)
        {
            var lastIndexOfBody = data.Length - 1;
            for (; lastIndexOfBody >= 0; lastIndexOfBody--)
            {
                if (data[lastIndexOfBody] != 0)
                    break;
            }

            lastIndexOfBody += 1;

            var key1 = Convert.FromBase64String(_fireblocksPubKey.Value);
            var parameters = GetPublicKeyRSAParametersBouncy(key1);

            /* Init alg */
            ISigner signer = SignerUtilities.GetSigner("SHA512withRSA");

            /* Populate key */
            signer.Init(false, parameters);
            signer.BlockUpdate(data, 0, lastIndexOfBody);

            return signer.VerifySignature(signature);
        }

        public static RsaKeyParameters GetPublicKeyRSAParametersBouncy(byte[] subjectPublicKeyInfoBytes)
        {
            var publicKeyObject = (DerSequence)Asn1Object.FromByteArray(subjectPublicKeyInfoBytes);
            var rsaPublicKeyParametersBitString = (DerBitString)publicKeyObject[1];

            var rsaPublicKeyParametersObject = (DerSequence)Asn1Object.FromByteArray(rsaPublicKeyParametersBitString.GetBytes());

            var modulus = ((DerInteger)rsaPublicKeyParametersObject[0]).Value.ToByteArray();
            var exponent = ((DerInteger)rsaPublicKeyParametersObject[1]).Value.ToByteArray();

            var modulusBigInt = new BigInteger(modulus);
            var exponentBigInt = new BigInteger(exponent);

            return new RsaKeyParameters(false, modulusBigInt, exponentBigInt);
        }
    }

    internal static class MyRsa
    {
        internal static string ReadPublicKeyFromPem(string pemPublicKey)
        {
            return pemPublicKey
            .Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");
        }
    }
}
