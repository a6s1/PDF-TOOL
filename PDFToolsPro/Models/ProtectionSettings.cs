namespace PDFToolsPro.Models;

public class ProtectionSettings
{
    public string UserPassword { get; set; } = string.Empty;
    public string OwnerPassword { get; set; } = string.Empty;
    public bool RequirePasswordToOpen { get; set; } = true;  // منع المشاهدة - يتطلب كلمة مرور لفتح الملف
    public bool PreventPrinting { get; set; } = false;
    public bool PreventCopying { get; set; } = false;
    public bool PreventEditing { get; set; } = false;
}



