using System;
using System.Collections.Generic;
using Sall.Evaluation;
using static Sall.Properties.PropertyBehavior;
using static Sall.Properties.PropertyNames;

namespace Sall.Properties;

public static class PropertyBehaviorMap
{
    private static readonly Dictionary<StringId, PropertyBehavior> Behaviors = new()
    {
        // Cascading
        [Opacity] = Accumulated,
        [Blur] = Accumulated,
        [Brightness] = Accumulated,
        [Contrast] = Accumulated,
        [Saturation] = Accumulated,
        [RenderScale] = Accumulated,

        // Subtree
        [Visibility] = Subtree,
        [Display] = Subtree,
        [Enabled] = Subtree,
        [HitTestVisible] = Subtree,
        [ClipChildren] = Subtree,
        [Mask] = Subtree,

        // Inherited
        [FontColor] = Inherited,
        [FontFamily] = Inherited,
        [FontSize] = Inherited,
        [FontWeight] = Inherited,
        [TextAlign] = Inherited,
        [Cursor] = Inherited,
        [LineHeight] = Inherited,
        [LetterSpacing] = Inherited,

        // Self only
        [BackgroundColor] = SelfOnly,
        [BorderColor] = SelfOnly,
        [BorderRadius] = SelfOnly,
        [Shadow] = SelfOnly,
        [Sprite] = SelfOnly,
        [ImageTint] = SelfOnly,
        [ZIndex] = SelfOnly,
        [Rotation] = SelfOnly,

        // Layout
        [Width] = Layout,
        [Height] = Layout,
        [MinWidth] = Layout,
        [MinHeight] = Layout,
        [MaxWidth] = Layout,
        [MaxHeight] = Layout,
        [Margin] = Layout,
        [Padding] = Layout,
        [Gap] = Layout,
        [FlexDirection] = Layout,
        [JustifyContent] = Layout,
        [AlignItems] = Layout,
        [Position] = Layout,
        [Anchor] = Layout,
        [Dock] = Layout,

        // Context
        [Theme] = Context,
        [AccentColor] = Context,
        [DefaultFont] = Context,
        [Localization] = Context,
        [DpiScale] = Context,
        [InputMode] = Context,
    };

    public static void RegisterCustomProperty(StringId propertyName, PropertyBehavior behavior)
    {
        if (Behaviors.ContainsKey(propertyName))
        {
            throw new ArgumentException($"Property name `{propertyName}` already exists and cannot be added");
        }

        Behaviors[propertyName] = behavior;
    }

    public static PropertyBehavior GetBehavior(StringId propertyName)
    {
        return Behaviors.TryGetValue(propertyName, out var behavior)
            ? behavior
            : SelfOnly;
    }

    public static bool IsInherited(StringId propertyName)
    {
        return GetBehavior(propertyName) == Inherited;
    }

    public static bool IsCascading(StringId propertyName)
    {
        return GetBehavior(propertyName) == Accumulated;
    }

    public static bool IsLayout(StringId propertyName)
    {
        return GetBehavior(propertyName) == Layout;
    }

    public static bool IsSubtree(StringId propertyName)
    {
        return GetBehavior(propertyName) == Subtree;
    }

    public static bool IsContext(StringId propertyName)
    {
        return GetBehavior(propertyName) == Context;
    }
}
