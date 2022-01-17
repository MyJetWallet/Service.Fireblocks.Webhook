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
            //byte[] hash;
            //using (SHA512 sha512 = SHA512.Create())
            //{
            //    hash = sha512.ComputeHash(data);
            //}
            //
            //bool b = false;
            //var rsaParam = GetPublicKeyRSAParameters(_fireblocksPubKey.Value);
            //
            //using (var rsa = new RSACryptoServiceProvider())
            //{
            //    rsa.ImportParameters(rsaParam);
            //    var signatureDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            //    signatureDeformatter.SetHashAlgorithm("SHA512");
            //
            //    b = signatureDeformatter.VerifySignature(hash, signature);
            //}

            //var key = Convert.FromBase64String(_fireblocksPubKey.Value);
            //var parameters = GetPublicKeyRSAParameters(key);

            //using var rsa = RSA.Create(parameters);
            //var pubKey = RSAPub
            //rsa.ImportRSAPublicKey(key, out _);

            //var result = rsa.VerifyData(data, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            //var resultX = Verify(data, signature);
            var rsa2 = GetRSAProviderFromPem(_fireblocksPubKey.Value);

            var result = rsa2.VerifyData(data, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

            return result;
        }

        public static bool Verify(byte[] data, byte[] signature)
        {
            var key1 = Convert.FromBase64String(_fireblocksPubKey.Value);
            var parameters = GetPublicKeyRSAParametersBouncy(key1);

            /* Init alg */
            ISigner signer = SignerUtilities.GetSigner("SHA512withRSA");

            /* Populate key */
            signer.Init(false, parameters);

            //if (data.Length < 512)
            //{
            //    var extendedData = new byte[512];
            //    Array.Copy(data, extendedData, data.Length);
            //    signer.BlockUpdate(extendedData, 0, extendedData.Length);
            //} else
            {
                /* Calculate the signature and see if it matches */
                signer.BlockUpdate(data, 0, data.Length);
            }
            return signer.VerifySignature(signature);
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

        public static RSACryptoServiceProvider GetRSAProviderFromPem(String pemstr)
        {
            CspParameters cspParameters = new CspParameters();
            cspParameters.KeyContainerName = "MyKeyContainer";
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParameters);

            Func<RSACryptoServiceProvider, RsaKeyParameters, RSACryptoServiceProvider> MakePublicRCSP = (RSACryptoServiceProvider rcsp, RsaKeyParameters rkp) =>
            {
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            Func<RSACryptoServiceProvider, RsaPrivateCrtKeyParameters, RSACryptoServiceProvider> MakePrivateRCSP = (RSACryptoServiceProvider rcsp, RsaPrivateCrtKeyParameters rkp) =>
            {
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            PemReader reader = new PemReader(new StringReader(pemstr));
            object kp = reader.ReadObject();

            // If object has Private/Public property, we have a Private PEM
            return (kp.GetType().GetProperty("Private") != null) ? MakePrivateRCSP(rsaKey, (RsaPrivateCrtKeyParameters)(((AsymmetricCipherKeyPair)kp).Private)) : MakePublicRCSP(rsaKey, (RsaKeyParameters)kp);
        }
    }

    internal static class MyRsa
    {
        internal static string ReadPublicKeyFromPem(string pemPublicKey)
        {
            return pemPublicKey;
                //.Replace("-----BEGIN PUBLIC KEY-----", "")
                //.Replace("-----END PUBLIC KEY-----", "")
                //.Replace("\n", "")
                //.Replace("\r", "");
        }
    }
}
