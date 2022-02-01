#Region "using"
Imports Azure.Identity
Imports Azure.Security.KeyVault.Certificates
Imports Azure.Security.KeyVault.Keys
Imports Azure.Security.KeyVault.Keys.Cryptography
Imports DevExpress.Office.DigitalSignatures
Imports DevExpress.Office.Tsp
Imports DevExpress.Pdf
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics

#End Region
Namespace PdfAPIAzureKeyVaultSample

#Region "client"
    Public Class AzureKeyVaultClient

        Public Shared Function CreateClient(ByVal keyVaultUrl As String) As AzureKeyVaultClient
            Return New AzureKeyVaultClient(New KeyClient(New Uri(keyVaultUrl), New DefaultAzureCredential()))
        End Function

        Private ReadOnly client As KeyClient

        Private defaultAzureCredential As DefaultAzureCredential

        Private Sub New(ByVal client As KeyClient)
            Me.client = client
            Dim credentialOptions = New DefaultAzureCredentialOptions With {.ExcludeInteractiveBrowserCredential = False, .ExcludeVisualStudioCodeCredential = True}
            defaultAzureCredential = New DefaultAzureCredential(credentialOptions)
        End Sub

        Public Function Sign(ByVal keyId As String, ByVal algorithm As SignatureAlgorithm, ByVal digest As Byte()) As Byte()
            Dim cloudRsaKey As KeyVaultKey = client.GetKey(keyId)
            Dim rsaCryptoClient = New CryptographyClient(cloudRsaKey.Id, defaultAzureCredential)
            Dim rsaSignResult As SignResult = rsaCryptoClient.Sign(algorithm, digest)
            Call Debug.WriteLine($"Signed digest using the algorithm {rsaSignResult.Algorithm}, with key {rsaSignResult.KeyId}. " & $"The resulting signature is {Convert.ToBase64String(rsaSignResult.Signature)}")
            Return rsaSignResult.Signature
        End Function

        Public Function GetCertificateData(ByVal keyVaultUrl As String, ByVal certificateIdentifier As String) As Byte()
            Dim certificateClient = New CertificateClient(New Uri(keyVaultUrl), defaultAzureCredential)
            Dim cert As KeyVaultCertificateWithPolicy = certificateClient.GetCertificate(certificateIdentifier)
            Return cert.Cer
        End Function
    End Class

#End Region
#Region "signer"
    Public Class AzureKeyVaultSigner
        Inherits Pkcs7SignerBase

        ' OID for RSA signing algorithm:
        Const PKCS1RsaEncryption As String = "1.2.840.113549.1.1.1"

        Private ReadOnly keyVaultClient As AzureKeyVaultClient

        Private ReadOnly keyId As String

        Private ReadOnly certificateChain As Byte()()

        ' Must match with key algorithm (RSA or ECDSA)
        ' For RSA PKCS1RsaEncryption(1.2.840.113549.1.1.1) OID can be used with any digest algorithm
        ' For ECDSA use OIDs from this family http://oid-info.com/get/1.2.840.10045.4.3 
        ' Specified digest algorithm must be same with DigestCalculator algorithm.
        Protected Overrides ReadOnly Property DigestCalculator As IDigestCalculator
            Get
                Return New DigestCalculator(HashAlgorithmType.SHA256)
            End Get
        End Property 'Digest algorithm

        Protected Overrides ReadOnly Property SigningAlgorithmOID As String
            Get
                Return PKCS1RsaEncryption
            End Get
        End Property

        Protected Overrides Function GetCertificates() As IEnumerable(Of Byte())
            Return certificateChain
        End Function

        Public Sub New(ByVal keyVaultClient As AzureKeyVaultClient, ByVal certificateIdentifier As String, ByVal keyId As String, ByVal keyVaultUri As String, ByVal Optional tsaClient As ITsaClient = Nothing, ByVal Optional ocspClient As IOcspClient = Nothing, ByVal Optional crlClient As ICrlClient = Nothing, ByVal Optional profile As PdfSignatureProfile = PdfSignatureProfile.Pdf)
            MyBase.New(tsaClient, ocspClient, crlClient, profile)
            Me.keyVaultClient = keyVaultClient
            Me.keyId = keyId
            ' Get certificate (without a public key) from Azure Key Vault storage
            ' via CertificateClient API or create a new one at runtime
            ' You can get the whole certificate chain here
            certificateChain = New Byte()() {keyVaultClient.GetCertificateData(keyVaultUri, certificateIdentifier)}
        End Sub

        Protected Overrides Function SignDigest(ByVal digest As Byte()) As Byte()
            Return keyVaultClient.Sign(keyId, SignatureAlgorithm.RS256, digest)
        End Function
    End Class
#End Region
End Namespace
