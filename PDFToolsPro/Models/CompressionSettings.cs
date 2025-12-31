namespace PDFToolsPro.Models;

public enum CompressionLevel
{
    High = 0,    // Minimal compression, best quality
    Medium = 1,  // Balanced
    Low = 2      // Maximum compression, lower quality
}

public class CompressionSettings
{
    public CompressionLevel Level { get; set; } = CompressionLevel.Medium;
    
    public int ImageQuality => Level switch
    {
        CompressionLevel.High => 90,
        CompressionLevel.Medium => 60,
        CompressionLevel.Low => 30,
        _ => 60
    };

    public float ScaleFactor => Level switch
    {
        CompressionLevel.High => 1.0f,
        CompressionLevel.Medium => 0.75f,
        CompressionLevel.Low => 0.5f,
        _ => 0.75f
    };

    public bool RemoveMetadata { get; set; } = true;
    public bool RemoveAnnotations { get; set; } = false;
}





