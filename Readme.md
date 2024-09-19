<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/344510041/23.2.2%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T980555)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
# PDF Document API - Use the Azure Key Vault API to Sign a PDF document 

The DevExpress PDF Document API library allows you to retrieve a certificate and sign the document hash using any external client. This example demonstrates how to use the [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/) API to sign a PDF document. 

### Prerequisites

-   An [Azure subscription](https://azure.microsoft.com/free/).
-   An existing Azure Key Vault. If you need to create an Azure Key Vault, you can use the Azure Portal or [Azure CLI](https://docs.microsoft.com/cli/azure).

### Description
First, you need to obtain a certificate or the whole certificate chain using **Azure Key Vault API**. Create a [Pkcs7SignerBase](https://docs.devexpress.com/OfficeFileAPI/DevExpress.Pdf.Pkcs7SignerBase) descendant and retrieve a certificate/ certificate chain in the constructor. Calculate the document hash using the [DevExpress.Office.DigitalSignatures.DigestCalculator](https://docs.devexpress.com/OfficeFileAPI/DevExpress.Office.DigitalSignatures.DigestCalculator) class with one of the following hashing algorithms: **SHA1**, **SHA256**, **SHA384**, and **SHA512**.
Then, sign the calculated document hash with a private key using Azure Key Vault API in the **SignDigest** method of the **Pkcs7SignerBase** class.

  ### Additional information
Note that to run this example, you will need to specify your Azure Key Vault URL, certificate Id, and RSA key. If you don't have a configured Key Vault, we recommend you review these articles to learn how to obtain this information:
 - [Azure Key Vault key client library for
   .NET](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/keyvault/Azure.Security.KeyVault.Keys)
 - [Azure Key Vault Certificate client library for.NET](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/keyvault/Azure.Security.KeyVault.Certificates)

Follow these steps to configure this example with a self-signed certificate generated by Azure Key Vault:
1. Create Azure Key Vault on Azure Portal. Assign the Key Vault URL to the **keyVaultUrl** variable in this example.
2. Generate a self-signed certificate. Assign the certificate name to the **certificateId** variable.
3. Open the certificate properties on Azure Portal. Find the "Key Identifier" field. 
![enter image description here](./Images/Azure%20Key%20Vault%202.png)
4.  Copy the Azure Key Id from the "Key Identifier" string and assign it to the **keyId** variable in this example.
![enter image description here](./Images/Azure%20Key%20Vault.png)
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=pdf-document-api-use-azure-key-vault-api-to-sign-document&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=pdf-document-api-use-azure-key-vault-api-to-sign-document&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
