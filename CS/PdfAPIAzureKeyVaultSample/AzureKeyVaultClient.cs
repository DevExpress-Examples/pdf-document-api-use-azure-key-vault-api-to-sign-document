# region #using
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using DevExpress.Office.DigitalSignatures;
using DevExpress.Office.Tsp;
using DevExpress.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion



namespace PdfAPIAzureKeyVaultSample
{
    #region client
    public class AzureKeyVaultClient
    {
        public static AzureKeyVaultClient CreateClient(string keyVaultUrl)
        {
            return new AzureKeyVaultClient(new KeyClient(new Uri(keyVaultUrl), new DefaultAzureCredential()));
        }

        readonly KeyClient client;

        DefaultAzureCredential defaultAzureCredential;
        AzureKeyVaultClient(KeyClient client)
        {
            this.client = client;

            var credentialOptions = new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = false,
                ExcludeVisualStudioCodeCredential = true
            };
            defaultAzureCredential = new DefaultAzureCredential(credentialOptions);
        }
        public byte[] Sign(string keyId, SignatureAlgorithm algorithm, byte[] digest)
        {
            KeyVaultKey cloudRsaKey = client.GetKey(keyId);
            var rsaCryptoClient = new CryptographyClient(cloudRsaKey.Id, defaultAzureCredential);

            SignResult rsaSignResult = rsaCryptoClient.Sign(algorithm, digest);
            Debug.WriteLine($"Signed digest using the algorithm {rsaSignResult.Algorithm}, with key {rsaSignResult.KeyId}. " +
                $"The resulting signature is {Convert.ToBase64String(rsaSignResult.Signature)}");

            return rsaSignResult.Signature;
        }

        public byte[] GetCertificateData(string keyVaultUrl, string certificateIdentifier)
        {
            var certificateClient = new CertificateClient(new Uri(keyVaultUrl), defaultAzureCredential);
            KeyVaultCertificateWithPolicy cert = certificateClient.GetCertificate(certificateIdentifier);

            return cert.Cer;
        }

    }
    #endregion

    #region signer
    public class AzureKeyVaultSigner : Pkcs7SignerBase
    {
        // OID for RSA signature algorithm:
        const string PKCS1RsaEncryption = "1.2.840.113549.1.1.1";

        readonly AzureKeyVaultClient keyVaultClient;
        readonly string keyId;
        readonly byte[][] certificateChain;


        // Must match with key algorithm (RSA or ECDSA)
        // OID for RSA PKCS1RsaEncryption(1.2.840.113549.1.1.1) can have any digest algorithm
        // For ECDSA, use OIDs from this family: http://oid-info.com/get/1.2.840.10045.4.3 
        // Specified digest algorithm must be the same as to DigestCalculator algorithm
        protected override IDigestCalculator DigestCalculator => new DigestCalculator(HashAlgorithmType.SHA256); //Digest algorithm
        protected override string SigningAlgorithmOID => PKCS1RsaEncryption;
        protected override IEnumerable<byte[]> GetCertificates() => certificateChain;

        public AzureKeyVaultSigner(AzureKeyVaultClient keyVaultClient, string certificateIdentifier, string keyId, string keyVaultUri, ITsaClient tsaClient = null,
            IOcspClient ocspClient = null, ICrlClient crlClient = null,
            PdfSignatureProfile profile = PdfSignatureProfile.Pdf) : base(tsaClient, ocspClient, crlClient, profile)
        {
            this.keyVaultClient = keyVaultClient;
            this.keyId = keyId;

            // Get certificate (without a public key) from Azure Key Vault storage
            // or create a new one at runtime
            // You can get the whole certificate chain here
            certificateChain = new byte[][] { keyVaultClient.GetCertificateData(keyVaultUri, certificateIdentifier) };
        }
        protected override byte[] SignDigest(byte[] digest)
        {
            return keyVaultClient.Sign(keyId, SignatureAlgorithm.RS256, digest);
        }
    }
    #endregion
}
