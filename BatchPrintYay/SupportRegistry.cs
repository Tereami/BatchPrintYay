using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchPrintYay
{
    public enum OrientationType { Portrait, Landscape, Automatic };

    public static class SupportRegistry
    {
        

        public static void ActivateSettingsForPDFCreator(string outputFolder)
        {
            try
            {
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0\\AutoSave", "Enabled", "True");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "FileNameTemplate", "<InputFilename>");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "OpenViewer", "False");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "OpenWithPdfArchitect", "False");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "TargetDirectory", outputFolder);
            }
            catch
            {
                string msg = "Не удалось настроить PDF-принтер, будут использованы настройки по-умолчанию. Выполните настройку принтера вручную";
                MessageBox msgbox = new MessageBox(msg);
                msgbox.ShowDialog();
            }

        }

        public static void SetOrientationForPdfCreator(OrientationType orientType)
        {
            string keyName = "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0\\PdfSettings";
            string keyTitle = "PageOrientation";
            string text = Enum.GetName(typeof(OrientationType), orientType);
            string check = "";
            try
            {
                check = Microsoft.Win32.Registry.GetValue(keyName, keyTitle, "default") as string;
                if (check == null || check == "default" || string.IsNullOrEmpty(check))
                {
                    return;
                }
                else
                {
                    if (check != text)
                    {
                        Microsoft.Win32.Registry.SetValue(keyName, keyTitle, text);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Не удалось настроить PDFCreator! Если это первый запуск - попробуйте отправить на PDFCreator любой другой документ, например, из Word. После этого запустите печать еще раз.";
                msg += "Устанавливаемое значение ключа реестра: " + text + ", значение ключа реестра: " + check + ". ";
                msg = msg + "Текст ошибки: " + ex.Message;
                MessageBox msgbox = new MessageBox(msg);
                msgbox.ShowDialog();
                throw new Exception(msg);
            }
        }

        public static void RestoreSettingsForPDFCreator()
        {
            try
            {
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0\\AutoSave", "Enabled", "False");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "FileNameTemplate", "<Title>");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "OpenViewer", "True");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "OpenWithPdfArchitect", "False");
                Microsoft.Win32.Registry.SetValue(
                    "HKEY_CURRENT_USER\\Software\\pdfforge\\PDFCreator\\Settings\\ConversionProfiles\\0", "TargetDirectory", "");
            }
            catch
            {
                string msg = "Не удалось восстановить настройки PDFCreator по умолчанию. Попробуйте переустановить принтер.";
                MessageBox msgbox = new MessageBox(msg);
                msgbox.ShowDialog();
                throw new Exception(msg);
            }
        }
    }
}
