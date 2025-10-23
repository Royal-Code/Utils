using System.Runtime.CompilerServices;
using System.Text;

namespace RoyalCode.Extensions.SourceGenerator;

public static class StringBuilderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder Indent(this StringBuilder builder, int level)
    {
        for (int i = 0; i < level; i++)
            builder.Append("    ");

        return builder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder IndentPlus(this StringBuilder builder, int level)
    {
        return builder.Indent(level).Append("    ");
    }
}