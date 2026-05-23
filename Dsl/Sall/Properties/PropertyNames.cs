using Sall.Evaluation;

namespace Sall.Properties;

public static class PropertyNames
{
    // Cascading / composited
    public static readonly StringId Opacity = "opacity";
    public static readonly StringId Blur = "blur";
    public static readonly StringId Brightness = "brightness";
    public static readonly StringId Contrast = "contrast";
    public static readonly StringId Saturation = "saturation";
    public static readonly StringId RenderScale = "render_scale";

    // Subtree / traversal
    public static readonly StringId Visibility = "visibility";
    public static readonly StringId Display = "display";
    public static readonly StringId Enabled = "enabled";
    public static readonly StringId HitTestVisible = "hit_test_visible";
    public static readonly StringId ClipChildren = "clip_children";
    public static readonly StringId Mask = "mask";

    // Inherited
    public static readonly StringId FontColor = "font_color";
    public static readonly StringId FontFamily = "font_family";
    public static readonly StringId FontSize = "font_size";
    public static readonly StringId FontWeight = "font_weight";
    public static readonly StringId TextAlign = "text_align";
    public static readonly StringId Cursor = "cursor";
    public static readonly StringId LineHeight = "line_height";
    public static readonly StringId LetterSpacing = "letter_spacing";

    // Self only
    public static readonly StringId BackgroundColor = "background_color";
    public static readonly StringId BorderColor = "border_color";
    public static readonly StringId BorderRadius = "border_radius";
    public static readonly StringId Shadow = "shadow";
    public static readonly StringId Sprite = "sprite";
    public static readonly StringId ImageTint = "image_tint";
    public static readonly StringId ZIndex = "z_index";
    public static readonly StringId Rotation = "rotation";

    // Layout
    public static readonly StringId Width = "width";
    public static readonly StringId Height = "height";
    public static readonly StringId MinWidth = "min_width";
    public static readonly StringId MinHeight = "min_height";
    public static readonly StringId MaxWidth = "max_width";
    public static readonly StringId MaxHeight = "max_height";
    public static readonly StringId Margin = "margin";
    public static readonly StringId Padding = "padding";
    public static readonly StringId Gap = "gap";
    public static readonly StringId FlexDirection = "flex_direction";
    public static readonly StringId JustifyContent = "justify_content";
    public static readonly StringId AlignItems = "align_items";
    public static readonly StringId Position = "position";
    public static readonly StringId Anchor = "anchor";
    public static readonly StringId Dock = "dock";

    // Context
    public static readonly StringId Theme = "theme";
    public static readonly StringId AccentColor = "accent_color";
    public static readonly StringId DefaultFont = "default_font";
    public static readonly StringId Localization = "localization";
    public static readonly StringId DpiScale = "dpi_scale";
    public static readonly StringId InputMode = "input_mode";
}
