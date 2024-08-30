# region #using
using Azure.Core;
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
using System.Security.Cryptography.X509Certificates;
#endregion



namespace PdfAPIAzureKeyVaultSample
{
    #region client
    public class AzureKeyVaultClient
    {
        public static AzureKeyVaultClient CreateClient(Uri keyVaultUri)
        {
            var tokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = false,
                ExcludeVisualStudioCodeCredential = true
            });

            return new AzureKeyVaultClient(new KeyClient(keyVaultUri, tokenCredential), tokenCredential);
        }

        readonly KeyClient client;

        TokenCredential clientCredentials;
        AzureKeyVaultClient(KeyClient client, TokenCredential cryptoClientCredential)
        {
            this.client = client;
            this.clientCredentials = cryptoClientCredential;
        }
        public byte[] Sign(string certificateName, SignatureAlgorithm algorithm, byte[] digest, string version = null)
        {
            KeyVaultKey cloudRsaKey = client.GetKey(certificateName, version: version);
            var rsaCryptoClient = new CryptographyClient(cloudRsaKey.Id, clientCredentials);

            SignResult rsaSignResult = rsaCryptoClient.Sign(algorithm, digest);
            Debug.WriteLine($"Signed digest using the algorithm {rsaSignResult.Algorithm}, " +
                $"with key {rsaSignResult.KeyId}. The resulting signature is {Convert.ToBase64String(rsaSignResult.Signature)}");
            return rsaSignResult.Signature;
        }

        public KeyVaultCertificateWithPolicy GetCertificateData(string keyId)
        {
            var certificateClient = new CertificateClient(client.VaultUri, clientCredentials);
            KeyVaultCertificateWithPolicy cert = certificateClient.GetCertificate(keyId);
            return cert;
        }
    }
    #endregion

    #region signer
    public class AzureKeyVaultSigner : Pkcs7SignerBase
    {
        //OID for RSA signing algorithm:
        const string PKCS1RsaEncryption = "1.2.840.113549.1.1.1";

        readonly AzureKeyVaultClient keyVaultClient;
        readonly KeyVaultCertificateWithPolicy certificate;
        List<byte[]> certificateChain = new List<byte[]>();


        //Must match with key algorithm (RSA or ECDSA)
        //For RSA PKCS1RsaEncryption(1.2.840.113549.1.1.1) OID can be used with any digest algorithm
        //For ECDSA use OIDs from this family http://oid-info.com/get/1.2.840.10045.4.3 
        //Specified digest algorithm must be same with DigestCalculator algorithm.
        protected override IDigestCalculator DigestCalculator => new DigestCalculator(HashAlgorithmType.SHA256); //Digest algorithm
        protected override string SigningAlgorithmOID => PKCS1RsaEncryption;
        protected override IEnumerable<byte[]> GetCertificates() => certificateChain;

        /// <summary>
        /// Construct an instance of AzureKeyVaultSigner
        /// </summary>
        /// <param name="keyVaultClient">API client used to communicate with </param>
        /// <param name="certificateName">The name of the Azure Certificate, will not contain any slashes</param>
        /// <param name="certificateVersion">The version of the Azure Certificate, looks like a UUID. Leave empty/null to use the version labelled in Azure Portal as the "Current Version"</param>
        /// <param name="tsaClient"></param>
        /// <param name="ocspClient"></param>
        /// <param name="crlClient"></param>
        /// <param name="profile"></param>
        /// <exception cref="System.ArgumentException"></exception>
        public AzureKeyVaultSigner(AzureKeyVaultClient keyVaultClient, string certificateName, string certificateVersion = null, ITsaClient tsaClient = null, IOcspClient ocspClient = null, ICrlClient crlClient = null, PdfSignatureProfile profile = PdfSignatureProfile.PAdES_BES) : base(tsaClient, ocspClient, crlClient, profile)
        {
            if (string.IsNullOrEmpty(certificateName))
                throw new System.ArgumentException("Certificate name must not be null or empty.");

            if (certificateName.Contains('/'))
                throw new System.ArgumentException("Invalid certificate name. Certificate name must not contain '/' character.");

            if (certificateVersion != null && certificateVersion.Contains('/'))
                throw new System.ArgumentException("Invalid certificate version. Certificate version must not contain '/' character.");

            this.keyVaultClient = keyVaultClient;
            //Get certificate (without public key) via GetCertificateAsync API
            //You can get the whole certificate chain here
            this.certificate = keyVaultClient.GetCertificateData($"{certificateName}/{certificateVersion}");
            BuildCertificateChain();
        }

        private void BuildCertificateChain()
        {
            var x509 = new X509Certificate2(certificate.Cer);
            var chain = new X509Chain();
            {
                chain.Build(x509);

                foreach (var item in chain.ChainElements)
                    certificateChain.Add(item.Certificate.GetRawCertData());
            }
        }

        protected override byte[] SignDigest(byte[] digest)
        {
            // use the name of the retrieved Certificate object to sign the digest
            // as it may differ from the name used to retrieve the certificate if auto-selection occurred.
            // https://learn.microsoft.com/en-us/azure/key-vault/general/about-keys-secrets-certificates#objects-identifiers-and-versioning
            var signature = keyVaultClient.Sign(certificate.Name, SignatureAlgorithm.RS256, digest);
            return signature;
        }
    }
    #endregion
}
