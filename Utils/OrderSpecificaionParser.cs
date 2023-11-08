namespace Utils;

class OrderSpecificationParser
{

    //parses a string of format B↑ and returns a OrderSpecification
    public static OrderSpecification parseOS(string spec)
    {
        var match = Regex.Match(spec, @"(.*)(↑|↓)?");
        if (match.Success)
        {
            var attribute = new Attribute(match.Groups[1].Value);
            var direction = match.Groups[2].Value switch
            {
                "↓" => OrderDirection.Descending,
                _ => OrderDirection.Ascending
            };
            return new OrderSpecification(attribute, direction);
        }
        else
        {
            throw new Exception("Invalid order specification");
        }
    }
}