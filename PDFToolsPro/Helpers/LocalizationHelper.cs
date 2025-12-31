using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PDFToolsPro.Helpers;

public class LocalizationHelper : INotifyPropertyChanged
{
    private static LocalizationHelper? _instance;
    public static LocalizationHelper Instance => _instance ??= new LocalizationHelper();

    private CultureInfo _currentCulture = new("ar");
    private bool _isRightToLeft = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            _currentCulture = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsRightToLeft));
            OnPropertyChanged(nameof(FlowDirection));
        }
    }

    public bool IsRightToLeft
    {
        get => _isRightToLeft;
        private set
        {
            _isRightToLeft = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FlowDirection));
        }
    }

    public System.Windows.FlowDirection FlowDirection =>
        IsRightToLeft ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight;

    public bool IsArabic => CurrentCulture.Name.StartsWith("ar");

    // Arabic Strings
    public string AppTitle => IsArabic ? "أدوات PDF الاحترافية" : "PDF Tools Pro";
    public string Compress => IsArabic ? "ضغط" : "Compress";
    public string Merge => IsArabic ? "دمج" : "Merge";
    public string Split => IsArabic ? "تقسيم" : "Split";
    public string Watermark => IsArabic ? "علامة مائية" : "Watermark";
    public string Protect => IsArabic ? "حماية" : "Protect";
    public string About => IsArabic ? "حول" : "About";
    public string SelectFiles => IsArabic ? "اختر الملفات" : "Select Files";
    public string DragFilesHere => IsArabic ? "اسحب الملفات هنا" : "Drag files here";
    public string Or => IsArabic ? "أو" : "or";
    public string Execute => IsArabic ? "تنفيذ" : "Execute";
    public string Cancel => IsArabic ? "إلغاء" : "Cancel";
    public string SelectedFiles => IsArabic ? "الملفات المحددة:" : "Selected Files:";
    public string CompressionQuality => IsArabic ? "جودة الضغط:" : "Compression Quality:";
    public string High => IsArabic ? "عالي" : "High";
    public string Medium => IsArabic ? "متوسط" : "Medium";
    public string Low => IsArabic ? "منخفض" : "Low";
    public string OutputFile => IsArabic ? "ملف الإخراج:" : "Output File:";
    public string Browse => IsArabic ? "استعراض" : "Browse";
    public string Processing => IsArabic ? "جاري المعالجة..." : "Processing...";
    public string Completed => IsArabic ? "تم بنجاح!" : "Completed!";
    public string Error => IsArabic ? "خطأ" : "Error";
    public string CompressCompleted => IsArabic ? "تم ضغط الملف بنجاح!" : "File compressed successfully!";
    public string MergeCompleted => IsArabic ? "تم دمج الملفات بنجاح!" : "Files merged successfully!";
    public string SplitCompleted => IsArabic ? "تم تقسيم الملف بنجاح!" : "File split successfully!";
    public string WatermarkCompleted => IsArabic ? "تم إضافة العلامة المائية بنجاح!" : "Watermark added successfully!";
    public string ProtectCompleted => IsArabic ? "تم حماية الملف بنجاح!" : "File protected successfully!";
    public string UnprotectCompleted => IsArabic ? "تم إزالة الحماية بنجاح!" : "Protection removed successfully!";
    public string SavedTo => IsArabic ? "تم الحفظ في:" : "Saved to:";
    public string OriginalSize => IsArabic ? "الحجم الأصلي:" : "Original Size:";
    public string NewSize => IsArabic ? "الحجم الجديد:" : "New Size:";
    public string Reduction => IsArabic ? "نسبة التقليل:" : "Reduction:";
    public string PageRange => IsArabic ? "نطاق الصفحات:" : "Page Range:";
    public string From => IsArabic ? "من" : "From";
    public string To => IsArabic ? "إلى" : "To";
    public string SplitEachPage => IsArabic ? "تقسيم كل صفحة" : "Split Each Page";
    public string ExtractPages => IsArabic ? "استخراج صفحات" : "Extract Pages";
    public string WatermarkText => IsArabic ? "نص العلامة المائية:" : "Watermark Text:";
    public string WatermarkImage => IsArabic ? "صورة العلامة المائية:" : "Watermark Image:";
    public string Opacity => IsArabic ? "الشفافية:" : "Opacity:";
    public string Angle => IsArabic ? "الزاوية:" : "Angle:";
    public string Position => IsArabic ? "الموقع:" : "Position:";
    public string Center => IsArabic ? "وسط" : "Center";
    public string TopLeft => IsArabic ? "أعلى يسار" : "Top Left";
    public string TopRight => IsArabic ? "أعلى يمين" : "Top Right";
    public string BottomLeft => IsArabic ? "أسفل يسار" : "Bottom Left";
    public string BottomRight => IsArabic ? "أسفل يمين" : "Bottom Right";
    public string Password => IsArabic ? "كلمة المرور:" : "Password:";
    public string ConfirmPassword => IsArabic ? "تأكيد كلمة المرور:" : "Confirm Password:";
    public string ProtectionOptions => IsArabic ? "خيارات الحماية" : "Protection Options";
    public string RequirePasswordToOpen => IsArabic ? "منع المشاهدة (يتطلب كلمة مرور لفتح الملف)" : "Require Password to Open";
    public string PreventPrinting => IsArabic ? "منع الطباعة" : "Prevent Printing";
    public string PreventCopying => IsArabic ? "منع النسخ" : "Prevent Copying";
    public string PreventEditing => IsArabic ? "منع التعديل" : "Prevent Editing";
    public string RemoveProtection => IsArabic ? "إزالة الحماية" : "Remove Protection";
    public string DeveloperInfo => IsArabic ? "تم التطوير بواسطة" : "Developed by";
    public string DeveloperName => "Abdullah Alsubaie";
    public string Version => IsArabic ? "الإصدار" : "Version";
    public string MoveUp => IsArabic ? "نقل للأعلى" : "Move Up";
    public string MoveDown => IsArabic ? "نقل للأسفل" : "Move Down";
    public string Remove => IsArabic ? "إزالة" : "Remove";
    public string Clear => IsArabic ? "مسح الكل" : "Clear All";
    public string CompressDescription => IsArabic ? "تصغير حجم ملفات PDF" : "Reduce PDF file size";
    public string MergeDescription => IsArabic ? "دمج عدة ملفات في ملف واحد" : "Combine multiple files into one";
    public string SplitDescription => IsArabic ? "تقسيم PDF لملفات منفصلة" : "Split PDF into separate files";
    public string WatermarkDescription => IsArabic ? "إضافة علامة مائية للملف" : "Add watermark to file";
    public string ProtectDescription => IsArabic ? "حماية الملف بكلمة مرور" : "Protect file with password";
    public string UseTextWatermark => IsArabic ? "استخدام نص" : "Use Text";
    public string UseImageWatermark => IsArabic ? "استخدام صورة" : "Use Image";
    public string FontSize => IsArabic ? "حجم الخط:" : "Font Size:";
    public string SelectImage => IsArabic ? "اختر صورة" : "Select Image";
    public string TotalPages => IsArabic ? "إجمالي الصفحات:" : "Total Pages:";
    public string Files => IsArabic ? "ملف" : "files";
    public string Merging => IsArabic ? "جاري الدمج" : "Merging";
    public string Compressing => IsArabic ? "جاري الضغط" : "Compressing";
    public string OutputFolder => IsArabic ? "مجلد الحفظ:" : "Output Folder:";
    public string SaveInSameFolder => IsArabic ? "حفظ في نفس المجلد" : "Save in same folder";

    public void SwitchLanguage()
    {
        if (IsArabic)
        {
            CurrentCulture = new CultureInfo("en");
            IsRightToLeft = false;
        }
        else
        {
            CurrentCulture = new CultureInfo("ar");
            IsRightToLeft = true;
        }

        // Notify all properties changed
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public void SetLanguage(string culture)
    {
        CurrentCulture = new CultureInfo(culture);
        IsRightToLeft = culture.StartsWith("ar");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}



