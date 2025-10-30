using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;

namespace Sany3y.Infrastructure.Services
{
    /// <summary>
    /// Provides reusable export functionality for converting any generic data collection
    /// into downloadable files, including PDF and CSV formats.
    /// </summary>
    /// <remarks>
    /// This class supports dynamic column mapping through lambda selectors and includes
    /// full UTF-8 support for Arabic text in generated documents.
    /// </remarks>
    public class TableExporter : Controller
    {
        /// <summary>
        /// Generates and returns a PDF file from a given data collection.
        /// </summary>
        /// <typeparam name="T">The type of data objects being exported.</typeparam>
        /// <param name="data">The collection of data to export.</param>
        /// <param name="title">The title to be displayed on the top of the PDF document.</param>
        /// <param name="headers">The table headers to display in the PDF.</param>
        /// <param name="selector">
        /// A function that converts each data object into an array of values (one per column).
        /// </param>
        /// <returns>
        /// A <see cref="FileContentResult"/> containing the generated PDF file with UTF-8 (Arabic) support.
        /// </returns>
        public FileContentResult ExportToPDF<T>(IEnumerable<T> data, string title, string[] headers, Func<T, object[]> selector)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // 🧩 1. إنشاء مستند PDF بالعرض
                Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 20, 20);
                PdfWriter.GetInstance(pdfDoc, memoryStream);
                pdfDoc.Open();

                // 🧩 2. تحميل خط عربي يدعم UTF-8
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                Font headerFont = new Font(bf, 12, Font.BOLD);
                Font cellFont = new Font(bf, 10, Font.NORMAL);
                Font titleFont = new Font(bf, 16, Font.BOLD);

                // 🧩 3. إنشاء الجدول
                PdfPTable table = new PdfPTable(headers.Length)
                {
                    WidthPercentage = 100
                };

                // 🧩 4. رؤوس الأعمدة
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 10
                    };
                    table.AddCell(cell);
                }

                // 🧩 5. الصفوف (البيانات)
                foreach (var item in data)
                {
                    object[] values = selector(item);
                    foreach (var value in values)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(value?.ToString() ?? "N/A", cellFont))
                        {
                            Padding = 6,
                            RunDirection = PdfWriter.RUN_DIRECTION_RTL // لدعم الاتجاه العربي
                        };
                        table.AddCell(cell);
                    }
                }

                // 🧩 6. العنوان
                Paragraph titleParagraph = new Paragraph(title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                pdfDoc.Add(titleParagraph);
                pdfDoc.Add(new Paragraph("\n"));
                pdfDoc.Add(table);
                pdfDoc.Close();

                // 🧩 7. إرجاع الملف النهائي
                byte[] bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", $"{title.Replace(" ", "_")}.pdf");
            }
        }

        /// <summary>
        /// Exports a data collection into a downloadable CSV file (UTF-8 encoded).
        /// </summary>
        /// <typeparam name="T">The type of data objects being exported.</typeparam>
        /// <param name="data">The collection of data to export.</param>
        /// <param name="title">The file name (used as the CSV title).</param>
        /// <param name="headers">The column headers to include at the top of the CSV file.</param>
        /// <param name="selector">
        /// A function that converts each data object into an array of values (one per column).
        /// </param>
        /// <returns>
        /// A <see cref="FileContentResult"/> containing the CSV file with UTF-8 encoding for Arabic support.
        /// </returns>
        public FileContentResult ExportToCSV<T>(IEnumerable<T> data, string title, string[] headers, Func<T, object[]> selector)
        {
            StringBuilder csvBuilder = new StringBuilder();

            // 🧩 1. إضافة رؤوس الأعمدة
            csvBuilder.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            // 🧩 2. إضافة الصفوف (البيانات)
            foreach (var item in data)
            {
                object[] values = selector(item);

                // تأمين القيم ضد الفواصل أو علامات التنصيص
                var safeValues = values.Select(v =>
                {
                    string value = v?.ToString() ?? "N/A";
                    value = value.Replace("\"", "\"\""); // escape " → ""
                    return $"\"{value}\""; // نحوطها بعلامات تنصيص
                });

                csvBuilder.AppendLine(string.Join(",", safeValues));
            }

            // 🧩 3. تحويل النص إلى بايتات وإرجاعه كملف CSV
            byte[] bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            string fileName = $"{title.Replace(" ", "_")}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}
