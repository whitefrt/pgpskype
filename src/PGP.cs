/************************************************************************/
/* pgpSkype                                                             */
/************************************************************************/
// copyright (c) 2013 white_frt
// public domain
//  using bouncycastle

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Diagnostics;

namespace pgpskype
{
    static class PGP
    {
        private static byte[] Compress(byte[] clearData, string fileName, CompressionAlgorithmTag algorithm)
        {
            MemoryStream bOut = new MemoryStream();
            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(algorithm);
            Stream cos = comData.Open(bOut);
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();
            Stream pOut = lData.Open(cos, PgpLiteralData.Binary, fileName, clearData.Length, DateTime.UtcNow);
            pOut.Write(clearData, 0, clearData.Length);
            pOut.Close();
            comData.Close();
            return bOut.ToArray();
        }

        public static string Encrypt(string input, PgpPublicKey key)
        {
            byte[] enc = Encrypt(new MemoryStream(Encoding.UTF8.GetBytes(input)), key);
            if (enc == null)
                return null;
            return Encoding.UTF8.GetString(enc);
        }

        public static byte[] Encrypt(Stream input, PgpPublicKey key)
        {
            input.Seek(0, SeekOrigin.Begin);

//            Console.WriteLine("Encrypting {0} bytes!", input.Length);

            bool armor = true;

            MemoryStream boutput = new MemoryStream();
            Stream output = boutput;
            if (armor)
                output = new ArmoredOutputStream(boutput);

            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] bytes = Compress(ms.ToArray(), "crp.txt", CompressionAlgorithmTag.Zip);

            PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, false, new SecureRandom());
            encGen.AddMethod(key);

            Stream cOut = encGen.Open(output, bytes.Length);

            cOut.Write(bytes, 0, bytes.Length);
            cOut.Close();

            if (armor)
            {
                output.Close();
            }
//            Console.WriteLine("Encrypt output: {0} bytes!", boutput.Length);
            return boutput.ToArray();
        }

        public static string Decrypt(string input, PgpSecretKeyRingBundle key, string pass)
        {
            byte[] dec = Decrypt(new MemoryStream(Encoding.UTF8.GetBytes(input)), key, pass);
            if (dec == null)
                return null;
            return Encoding.UTF8.GetString(dec);
        }

        static byte[] Decrypt(Stream input, PgpSecretKeyRingBundle bundle, string pass)
        {
            input.Seek(0, SeekOrigin.Begin);

//            Console.WriteLine("Decrypting {0} bytes!", input.Length);

            input = PgpUtilities.GetDecoderStream(input);

            PgpObjectFactory pgpF = new PgpObjectFactory(input);
            PgpObject o = pgpF.NextPgpObject();

            PgpEncryptedDataList enc = o as PgpEncryptedDataList;
            if (enc == null)
            {
                enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
            }

            //PgpPbeEncryptedData pbe = (PgpPbeEncryptedData)enc[0];
            PgpPublicKeyEncryptedData pbe = null;
            PgpPrivateKey sKey = null;
            foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
            {
                // Try to get a suitable secret key
                sKey = FindSecretKey(bundle, pked.KeyId, pass);
                if (sKey != null)
                {
                    pbe = pked;
                    break;
                }
            }

            if (sKey == null)
                return null;

            Stream clear = pbe.GetDataStream(sKey);
            PgpObjectFactory plainFact = new PgpObjectFactory(clear);
            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData)
            {
                PgpCompressedData cData = (PgpCompressedData)message;
                PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());

                message = pgpFact.NextPgpObject();
            }

            if (message is PgpLiteralData)
            {
                PgpLiteralData ld = (PgpLiteralData)message;

                Stream unc = ld.GetInputStream();
                byte[] outbytes = Streams.ReadAll(unc);
                Console.WriteLine("Decrypt output: {0} bytes!", outbytes.Length);
                return outbytes;
            }
            return null;
        }

        static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, string pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
                return null;
            PgpPrivateKey priv_key = pgpSecKey.ExtractPrivateKey(pass.ToCharArray());
            if (priv_key == null)
                return null;
            return priv_key;
        }

        public class KeyPairOut
        {
            public string strPublic;
            public string strPrivate;
        }

        static public KeyPairOut GenerateKeyPair(int bits, string identity, string pass)
        {
            MemoryStream pub = new MemoryStream();
            MemoryStream priv = new MemoryStream();

            GenerateKeyPair(bits, priv, pub, identity, pass);

            KeyPairOut kp = new KeyPairOut();
            kp.strPublic = Encoding.UTF8.GetString(pub.GetBuffer());
            kp.strPrivate = Encoding.UTF8.GetString(priv.GetBuffer());
            return kp;
        }

        static public void GenerateKeyPair(int bits, Stream privateOut, Stream publicOt, string identity, string pass)
        {
            IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
            if (kpg == null)
                return;
            kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x10001), new SecureRandom(), bits, 25));

            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            if (kp == null)
                return;

            AsymmetricKeyParameter publicKey = kp.Public;
            AsymmetricKeyParameter privateKey = kp.Private;
            char[] passPhrase = pass.ToCharArray();

            // Encode secret
            Stream secretOut = new ArmoredOutputStream(privateOut);

            PgpSecretKey secretKey = new PgpSecretKey(PgpSignature.DefaultCertification, PublicKeyAlgorithmTag.RsaGeneral, publicKey, privateKey, DateTime.UtcNow,
                                                        identity, SymmetricKeyAlgorithmTag.Aes256, passPhrase, null, null, new SecureRandom());

            secretKey.Encode(secretOut);
            secretOut.Close();

            Stream publicOut = new ArmoredOutputStream(publicOt);

            secretKey.PublicKey.Encode(publicOut);
            publicOut.Close();
        }
    }
}
