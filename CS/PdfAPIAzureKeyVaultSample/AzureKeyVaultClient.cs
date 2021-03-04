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
using System.Runtime.Serialization;
using System.Text;



namespace PdfAPIAzureKeyVaultSample
{
    public class AzureKeyVaultSigner : Pkcs7SignerBase
    {
        //OID for RSA signing algorithm:
        const string PKCS1RsaEncryption = "1.2.840.113549.1.1.1";

        readonly AzureKeyVaultClient keyVaultClient;
        readonly string keyId;
        readonly byte[][] certificateChain;


        //Must match with key algorithm (RSA or ECDSA)
        //For RSA PKCS1RsaEncryption(1.2.840.113549.1.1.1) OID can be used with any digest algorithm
        //For ECDSA use OIDs from this family http://oid-info.com/get/1.2.840.10045.4.3 
        //Specified digest algorithm must be same with DigestCalculator algorithm.
        protected override IDigestCalculator DigestCalculator => new DevExpress.Office.DigitalSignatures.DigestCalculator(HashAlgorithmType.SHA256); //Digest algorithm
        protected override string SigningAlgorithmOID => PKCS1RsaEncryption;
        protected override IEnumerable<byte[]> GetCertificates() => certificateChain;

        public AzureKeyVaultSigner(AzureKeyVaultClient keyVaultClient, string certificateIdentifier, string keyId, ITsaClient tsaClient = null,
            IOcspClient ocspClient = null, ICrlClient crlClient = null, 
            PdfSignatureProfile profile = PdfSignatureProfile.Pdf) : base(tsaClient, ocspClient, crlClient, profile)
        {
            this.keyVaultClient = keyVaultClient;
            this.keyId = keyId;
            //Get certificate (without piblic key) from Azure Key Vault storage via CertificateClient API or create a new one at runtime
            //You can get the whole certificate chain here
            certificateChain = new byte[][] { keyVaultClient.GetCertificateData(keyId, certificateIdentifier) };
        }
        protected override byte[] SignDigest(byte[] digest)
        {
            return keyVaultClient.Sign( SignatureAlgorithm.RS256, digest);
        }
    }

    public class AzureKeyVaultClient 
    {
        const string rsaKeyId = "";//specify name of Key Vault's RSA key here
        /*
         * Alternatively, you can create a temporary RSA key:
         * rsaKeyId = $"CloudRsaKey-{Guid.NewGuid()}";
            var rsaKeyOptions = new CreateRsaKeyOptions(rsaKeyName, hardwareProtected: false)
            {
                KeySize = 2048,
            };
         */
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
       public byte[] Sign(Azure.Security.KeyVault.Keys.Cryptography.SignatureAlgorithm algorithm, byte[] digest)
        {
            KeyVaultKey cloudRsaKey = client.GetKey(rsaKeyId);
            var rsaCryptoClient = new CryptographyClient(cloudRsaKey.Id, defaultAzureCredential);

            SignResult rsaSignResult = rsaCryptoClient.Sign(algorithm, digest);
            Debug.WriteLine($"Signed digest using the algorithm {rsaSignResult.Algorithm}, with key {rsaSignResult.KeyId}. " +
                $"The resulting signature is {Convert.ToBase64String(rsaSignResult.Signature)}");

            return rsaSignResult.Signature;
        }

        public byte[] GetCertificateData(string keyVaultUrl, string certificateIdentifier)
        {
            var certificateClient = new CertificateClient(new Uri (keyVaultUrl), defaultAzureCredential);
            KeyVaultCertificateWithPolicy cert = certificateClient.GetCertificate(certificateIdentifier);
            
            return cert.Cer;
        }
 
    
    }
}
