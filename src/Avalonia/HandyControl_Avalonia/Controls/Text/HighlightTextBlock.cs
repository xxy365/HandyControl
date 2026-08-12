using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Media;

namespace HandyControl.Controls;

public class HighlightTextBlock : TextBlock
{
    public static readonly StyledProperty<string?> SourceTextProperty =
        AvaloniaProperty.Register<HighlightTextBlock, string?>(nameof(SourceText));

    public static readonly StyledProperty<string?> QueriesTextProperty =
        AvaloniaProperty.Register<HighlightTextBlock, string?>(nameof(QueriesText));

    public static readonly StyledProperty<IBrush?> HighlightBrushProperty =
        AvaloniaProperty.Register<HighlightTextBlock, IBrush?>(nameof(HighlightBrush));

    public static readonly StyledProperty<IBrush?> HighlightTextBrushProperty =
        AvaloniaProperty.Register<HighlightTextBlock, IBrush?>(nameof(HighlightTextBrush));

    public string? SourceText
    {
        get => GetValue(SourceTextProperty);
        set => SetValue(SourceTextProperty, value);
    }

    public string? QueriesText
    {
        get => GetValue(QueriesTextProperty);
        set => SetValue(QueriesTextProperty, value);
    }

    public IBrush? HighlightBrush
    {
        get => GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public IBrush? HighlightTextBrush
    {
        get => GetValue(HighlightTextBrushProperty);
        set => SetValue(HighlightTextBrushProperty, value);
    }

    static HighlightTextBlock()
    {
        SourceTextProperty.Changed.AddClassHandler<HighlightTextBlock>((s, e) => s.RefreshInlines());
        QueriesTextProperty.Changed.AddClassHandler<HighlightTextBlock>((s, e) => s.RefreshInlines());
    }

    private void RefreshInlines()
    {
        Inlines?.Clear();

        if (string.IsNullOrEmpty(SourceText)) return;
        if (string.IsNullOrEmpty(QueriesText))
        {
            Inlines?.Add(SourceText);
            return;
        }

        var sourceText = SourceText;
        var queries = QueriesText!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var intervals = from query in queries.Distinct()
                        from interval in GetQueryIntervals(sourceText!, query)
                        select interval;
        var mergedIntervals = MergeIntervals(intervals.ToList());
        var fragments = SplitTextByOrderedDisjointIntervals(sourceText!, mergedIntervals);

        foreach (var fragment in fragments)
        {
            if (fragment.IsQuery)
            {
                var run = new Run(fragment.Text);
                run.Bind(TextElement.BackgroundProperty, new Binding { Source = this, Path = nameof(HighlightBrush) });
                run.Bind(TextElement.ForegroundProperty, new Binding { Source = this, Path = nameof(HighlightTextBrush) });
                Inlines?.Add(run);
            }
            else
            {
                Inlines?.Add(new Run(fragment.Text));
            }
        }
    }

    private static IEnumerable<Fragment> SplitTextByOrderedDisjointIntervals(string sourceText, List<Range> mergedIntervals)
    {
        if (string.IsNullOrEmpty(sourceText)) yield break;

        if (mergedIntervals?.Any() != true)
        {
            yield return new Fragment { Text = sourceText, IsQuery = false };
            yield break;
        }

        var range0 = mergedIntervals.First();
        int start0 = range0.Start;
        int end0 = range0.End;

        if (start0 > 0) yield return new Fragment { Text = sourceText.Substring(0, start0), IsQuery = false };
        yield return new Fragment { Text = sourceText.Substring(start0, end0 - start0), IsQuery = true };

        int previousEnd = end0;
        foreach (var range in mergedIntervals.Skip(1))
        {
            int start = range.Start;
            int end = range.End;
            yield return new Fragment { Text = sourceText.Substring(previousEnd, start - previousEnd), IsQuery = false };
            yield return new Fragment { Text = sourceText.Substring(start, end - start), IsQuery = true };
            previousEnd = end;
        }

        if (previousEnd < sourceText.Length)
            yield return new Fragment { Text = sourceText.Substring(previousEnd), IsQuery = false };
    }

    private static List<Range> MergeIntervals(List<Range> intervals)
    {
        if (intervals?.Any() != true) return new List<Range>();

        intervals.Sort((x, y) => x.Start != y.Start ? x.Start - y.Start : x.End - y.End);

        var pointer = intervals[0];
        int startPointer = pointer.Start;
        int endPointer = pointer.End;

        var result = new List<Range>();
        foreach (var range in intervals.Skip(1))
        {
            int start = range.Start;
            int end = range.End;

            if (start <= endPointer)
            {
                if (endPointer < end)
                {
                    endPointer = end;
                }
            }
            else
            {
                result.Add(new Range { Start = startPointer, End = endPointer });
                startPointer = start;
                endPointer = end;
            }
        }
        result.Add(new Range { Start = startPointer, End = endPointer });
        return result;
    }

    private static IEnumerable<Range> GetQueryIntervals(string sourceText, string query)
    {
        if (string.IsNullOrEmpty(sourceText) || string.IsNullOrEmpty(query)) yield break;

        int nextStartIndex = 0;
        while (nextStartIndex < sourceText.Length)
        {
            int index = sourceText.IndexOf(query, nextStartIndex, StringComparison.CurrentCultureIgnoreCase);
            if (index == -1) yield break;

            nextStartIndex = index + query.Length;
            yield return new Range { Start = index, End = nextStartIndex };
        }
    }

    private struct Fragment
    {
        public string Text { get; set; }
        public bool IsQuery { get; set; }
    }

    private struct Range
    {
        public int Start { get; set; }
        public int End { get; set; }
    }
}