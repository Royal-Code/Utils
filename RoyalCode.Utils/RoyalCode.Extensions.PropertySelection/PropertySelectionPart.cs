using System.Reflection;

namespace RoyalCode.Extensions.PropertySelection;

internal class PropertySelectionPart
{
    private readonly string[] parts;
    private readonly Type currentType;
    private readonly int position;
    private readonly PropertySelection? parent;

    internal PropertySelectionPart(string[] parts, Type currentType, int position = 0, PropertySelection? parent = null)
    {
        this.parts = parts;
        this.currentType = currentType;
        this.position = position;
        this.parent = parent;
    }

    internal PropertySelection? Select()
    {
        var currentProperty = string.Empty;

        for (int i = position; i < parts.Length; i++)
        {
            currentProperty += parts[i];
            var info = currentType.GetTypeInfo().PropertyLookup(currentProperty);
            if (info is not null)
            {
                var ps = parent == null ? new PropertySelection(info) : parent.SelectChild(info);
                if (i + 1 < parts.Length)
                {
                    var nextPart = new PropertySelectionPart(parts, info.PropertyType, i + 1, ps);
                    var nextPs = nextPart.Select();
                    if (nextPs is not null)
                        return nextPs;
                }
                else
                {
                    return ps;
                }
            }
        }

        return null;
    }
}