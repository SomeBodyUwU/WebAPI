namespace API.Models;

public class SearchByImageModel
{
    internal string filename { get; set; } //   The filename of file where the match is found
    internal int similarity { get; set; } //   Similarity compared to the search image
    internal int from { get; set; } //   Starting time of the matching scene (seconds)
    internal int to { get; set; } //   Ending time of the matching scene (seconds)
    internal string video { get; set; }
    internal string image { get; set; }
}