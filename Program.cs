using System.Text.RegularExpressions;

string baseDir = "..";

Regex[] paths = [
    new(@"gfx/.*(\.bmp|\.tga)$"),
    new(@"maps/.*(\.bsp|\.txt)$"),
    new(@"models/.*(\.mdl)$"),
    new(@"overviews/.*(\.bmp|\.tga|\.txt)$"),
    new(@"sound/.*(\.wav)$"),
    new(@"sprites/.*(\.spr|\.tga|\.txt)$"),
    new(@".*(\.wad)$"),
];

var lowerCaseToFileName = new Dictionary<string, string>();

foreach (var fileName in Directory.GetFiles(baseDir, "*.*", new EnumerationOptions()
{
    RecurseSubdirectories = true
}))
{
    var mixedCaseFileName = Path.GetRelativePath(baseDir, Path.GetFullPath(fileName)).Replace('\\', '/');
    var lowerCaseFileName = mixedCaseFileName.ToLowerInvariant();
    if (mixedCaseFileName == lowerCaseFileName)
        continue;

    if (!paths.Any(p => p.IsMatch(lowerCaseFileName)))
        continue;

    lowerCaseToFileName.Add(lowerCaseFileName, mixedCaseFileName);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression();

var app = builder.Build();

app.UseResponseCompression();

app.MapGet("/{gameDirString}/{**fileName}", (string gameDirString, string fileName) => {

    if(gameDirString.Contains('.') || gameDirString.Contains('/') || gameDirString.Contains('\\') || fileName.Contains("..") || fileName.StartsWith(".") || fileName.Contains(":") || fileName.Contains('\\'))
        return Results.NotFound();

    if (!paths.Any(p => p.IsMatch(fileName)))
        return Results.NotFound();

    var gameDirs = gameDirString.Split('+', StringSplitOptions.RemoveEmptyEntries);
    foreach (var gameDir in gameDirs)
    {
        var relativePath = Path.GetRelativePath(baseDir, Path.GetFullPath(Path.Combine(baseDir, gameDir, fileName))).Replace('\\', '/');
        if (lowerCaseToFileName.TryGetValue(relativePath, out string? mixedCasePath))
            relativePath = mixedCasePath;

        var fullPath = Path.GetFullPath(Path.Combine(baseDir, relativePath));

        if (!File.Exists(fullPath))
            continue;

        return Results.File(fullPath);
    }
    return Results.NotFound();
});

app.Run();
