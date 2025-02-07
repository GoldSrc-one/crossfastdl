string baseDir = "..";
HashSet<string> extensions = new HashSet<string>() { ".bmp", ".bsp", ".mdl", ".spr", ".tga", ".txt", ".wad", ".wav" };

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
        string fullPath = Path.GetFullPath(Path.Combine(baseDir, gameDir, fileName));
        if(!File.Exists(fullPath))
            continue;

        return Results.File(fullPath);
    }
    return Results.NotFound();
});

app.Run();
