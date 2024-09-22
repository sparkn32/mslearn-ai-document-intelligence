using System;
using System.IO;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

// Store connection information. NOTE: This is an example, in production apps please secure your endpoint and key
string endpoint = "https://form******.cognitiveservices.azure.com/"; // Replace with your endpoint
string key = ""; // Replace with your key

AzureKeyCredential credential = new AzureKeyCredential(key);
DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

Console.WriteLine($"Connecting to Forms Recognizer resource: '{endpoint}'\n");

// Form to analyze. Point this to the files you want to analyze
string folderPath = "C:\\App1\\Invoices"; // replace with the path to your folder
DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
FileInfo[] files = dirInfo.GetFiles();

// Variable to keep track of total invoice amount
double confidentInvoiceTotal = 0;

AnalyzeResult result;
foreach (FileInfo file in files) {
    using (FileStream stream = new FileStream(file.FullName, FileMode.Open)) {
        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-invoice", stream);
        result = operation.Value;
    }
    // Variable to keep track of total invoice amount
    for (int i = 0; i < result.Documents.Count; i++) {
        AnalyzedDocument invoice = result.Documents[i];
        Console.WriteLine($"Invoice {i}:");
        if (invoice.Fields.TryGetValue("VendorName", out DocumentField? vendorNameField)) {
            if (vendorNameField.FieldType == DocumentFieldType.String) {
                string vendorName = vendorNameField.Value.AsString();
                Console.WriteLine($"Vendor Name: '{vendorName}', with confidence {vendorNameField.Confidence}.");
            }
        }
        if (invoice.Fields.TryGetValue("InvoiceTotal", out DocumentField? invoiceTotalField)) {
            if (invoiceTotalField.FieldType == DocumentFieldType.Currency && invoiceTotalField.Confidence>=0.75) {
                CurrencyValue invoiceTotal = invoiceTotalField.Value.AsCurrency();
                Console.WriteLine($"Invoice Total: '{invoiceTotal.Amount}', with confidence {invoiceTotalField.Confidence}\n");

                // Add to the total invoice amount, and filter by confidence
                confidentInvoiceTotal+=invoiceTotal.Amount;


            }
        }
    }
}

Console.WriteLine();


Console.WriteLine($"Total confident invoice amount: {confidentInvoiceTotal}");
