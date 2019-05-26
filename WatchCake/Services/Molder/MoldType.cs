namespace WatchCake.Services.Molder
{
    /// <summary>
    /// Types of all supported molds.
    /// </summary>
    public enum MoldType
    {
        Append,
        Prepend,
        FloatAdd,
        FloatMult,
        Float1DivX,
        AddIfMissing,
        Spacer,
        DeSpace,
        OnlyFloatChars,
        FakeLength,
        Substr,
        Trim,
        HtmlDecode,
        Before,
        After,
        AfterLast,
        Between,
        Commas2points,
        Replace,
        Remove,
        StripHtmlComments,
        SetIfEmpty,
        ReplaceIfLonger,
        CutEnd,
        CutStart,
        EasyHash,
        TitleCase,
        RegexReplace,
        Override
    }
}
