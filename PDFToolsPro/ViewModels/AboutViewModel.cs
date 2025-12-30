using CommunityToolkit.Mvvm.ComponentModel;
using PDFToolsPro.Helpers;
using System.Reflection;

namespace PDFToolsPro.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public LocalizationHelper Loc => LocalizationHelper.Instance;

    public string AppName => "PDF Tools Pro";
    
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    
    public string DeveloperName => "Abdullah Alsubaie";
    
    public string Copyright => $"© {DateTime.Now.Year} {DeveloperName}. All rights reserved.";
    
    public string Description => Loc.IsArabic 
        ? "أداة احترافية لإدارة ملفات PDF تشمل الضغط والدمج والتقسيم وإضافة العلامة المائية والحماية."
        : "Professional PDF management tool including compression, merging, splitting, watermarking and protection.";

    public string[] Features => Loc.IsArabic
        ? new[]
        {
            "ضغط ملفات PDF لتقليل الحجم",
            "دمج عدة ملفات PDF في ملف واحد",
            "تقسيم PDF إلى ملفات منفصلة",
            "إضافة علامة مائية نصية أو صورة",
            "حماية الملفات بكلمة مرور",
            "دعم كامل للغتين العربية والإنجليزية"
        }
        : new[]
        {
            "Compress PDF files to reduce size",
            "Merge multiple PDF files into one",
            "Split PDF into separate files",
            "Add text or image watermark",
            "Protect files with password",
            "Full Arabic and English language support"
        };
}



