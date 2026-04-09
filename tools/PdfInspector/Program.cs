using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: PdfInspector <pdf-path> [output-path]");
    return 1;
}

var pdfPath = Path.GetFullPath(args[0]);
var outputPath = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.ChangeExtension(pdfPath, ".txt");

using var reader = new PdfReader(pdfPath);
using var writer = new StreamWriter(outputPath, false, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

for (var page = 1; page <= reader.NumberOfPages; page++)
{
    writer.WriteLine($"===== Page {page} =====");
    writer.WriteLine(PdfTextExtractor.GetTextFromPage(reader, page, new LocationTextExtractionStrategy()));
    writer.WriteLine();
}

Console.WriteLine(outputPath);
return 0;
