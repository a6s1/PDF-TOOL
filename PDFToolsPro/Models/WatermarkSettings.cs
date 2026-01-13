namespace PDFToolsPro.Models;

public enum WatermarkPosition
{
    Center,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public enum WatermarkType
{
    Text,
    Image
}

public class WatermarkSettings
{
    public WatermarkType Type { get; set; } = WatermarkType.Text;
    public string Text { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public float Opacity { get; set; } = 0.3f;
    public float Angle { get; set; } = 45f;
    public WatermarkPosition Position { get; set; } = WatermarkPosition.Center;
    public int FontSize { get; set; } = 48;
}






