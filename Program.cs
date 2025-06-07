string baseDir = "..";
HashSet<string> extensions = new HashSet<string>() { ".bmp", ".bsp", ".mdl", ".spr", ".tga", ".txt", ".wad", ".wav" };

var lowerCaseToFileName = new Dictionary<string, string>();
foreach (var extension in extensions) {
    foreach (var fileName in Directory.GetFiles(baseDir, $"*{extension}", new EnumerationOptions()
    {
        RecurseSubdirectories = true,
        MatchCasing = MatchCasing.CaseInsensitive
    }))
    {
        var mixedCaseFileName = Path.GetRelativePath(baseDir, Path.GetFullPath(fileName));
        var lowerCaseFileName = mixedCaseFileName.ToLowerInvariant();
        if (mixedCaseFileName == lowerCaseFileName)
            continue;

        lowerCaseToFileName.Add(lowerCaseFileName, mixedCaseFileName);
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression();

var app = builder.Build();

app.UseResponseCompression();

app.MapGet("/{gameDirString}/{**fileName}", (string gameDirString, string fileName) => {
    if(!extensions.Contains(Path.GetExtension(fileName)) || gameDirString.Contains('.') || gameDirString.Contains('/') || gameDirString.Contains('\\'))
        return Results.NotFound();

    var gameDirs = gameDirString.Split('+', StringSplitOptions.RemoveEmptyEntries);
    foreach (var gameDir in gameDirs)
    {
        var relativePath = Path.GetRelativePath(baseDir, Path.GetFullPath(Path.Combine(baseDir, gameDir, fileName)));
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
