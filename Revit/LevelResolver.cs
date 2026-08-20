using Autodesk.Revit.DB;

namespace BIMIntelligence.Revit;

public class LevelResolver
{
    private readonly Document _document;

    public LevelResolver(Document document)
    {
        _document = document;
    }

    public string ResolveLevelName(string requestedLevel)
    {
        var levels =
            new FilteredElementCollector(_document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

        Level? exactMatch =
            levels.FirstOrDefault(
                x => x.Name.Equals(
                    requestedLevel,
                    StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return exactMatch.Name;
        }

        // Handle common natural-language form:
        // "Level 1" -> "L1"
        string normalized =
            requestedLevel
                .Trim()
                .Replace("Level ", "L",
                    StringComparison.OrdinalIgnoreCase);

        Level? normalizedMatch =
            levels.FirstOrDefault(
                x => x.Name.Equals(
                    normalized,
                    StringComparison.OrdinalIgnoreCase));

        if (normalizedMatch != null)
        {
            return normalizedMatch.Name;
        }

        throw new InvalidOperationException(
            $"Could not find a Revit level matching '{requestedLevel}'.");
    }
}