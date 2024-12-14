namespace mdi_maui.Models
{
    internal class CanvasModel
    {
        public string ToolType { get; set; } = string.Empty;
        public Color ToolColor { get; set; } = Colors.Black;
        public int ToolWidth { get; set; } = 5;
        public CanvasModel() { }
    }
}